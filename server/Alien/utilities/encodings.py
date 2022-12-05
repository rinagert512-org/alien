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

def decode_possibly_padded_str_into_int(text, alphabet):
    without_padding = decode_str_into_int(text, alphabet)
    if without_padding is not None:
        return without_padding

    if not text.startswith(alphabet[0]):
        raise ValueError(f"Could not decode {text}")

    padding_once = input[1:]
    attempt = decode_str_into_int(padding_once, alphabet)
    if attempt is not None:
        return attempt

    padding_twice = input[2:]
    attempt = decode_str_into_int(padding_twice, alphabet)
    if attempt is not None:
        return attempt
    raise ValueError(f"Could not decode {input}")
