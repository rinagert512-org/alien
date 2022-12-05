import base64
import binascii

from Alien.utilities.mersenne_twister import MersenneTwister

def encode_int_into_str(value, alphabet, apply_padding = False):
    text = ""
    length = len(alphabet)

    while True:
        value, index = divmod(value, length)
        text = alphabet[index] + text
        if value <= 0:
            break

    if apply_padding:
        text = text.rjust(3, alphabet[0])

    return text
