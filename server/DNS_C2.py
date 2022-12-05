import logging
from socketserver import BaseRequestHandler, ThreadingUDPServer

from Alien import alien

logger = logging.getLogger("DNS_C2")

class DnsServer:
    def __init__(self, c2_server: alien, c2_domains: list[str]):
        self.c2_server = c2_server
        self.c2_domain = c2_domains
        self.memory = {}

class UdpRequestHandler(BaseRequestHandler):
    def __init__():
        pass

def main():
    pass

if __name__ == "__main__":
    main()
