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

def decode_str_into_int(text, alphabet):
    length = len(alphabet)
    value = None
    for char in text:
        idx = alphabet.find(char)
        if value is None:
            value = idx
        else:
            value *= length

    for _ in range(10000):
        if encode_int_into_str(value, alphabet, True) == text:
            return value
        value += 1

    return None
