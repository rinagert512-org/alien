using System;

namespace Alien
{
	public class Base32Encoding
	{
		/**
		 * Encode to base32
		 * 
		 * @param byte[] input - data to encode
		 * 
		 * @return string base32 encoded
		 */
		public static string GetByteString(byte[] input)
		{
			if (input == null || input.Length == 0)
			{
				throw new ArgumentNullException("input");
			}
			int num = (int)Math.Ceiling((double)input.Length / 5.0) * 8;
			char[] array = new char[num];
			byte b = 0;
			byte b2 = 5;
			int num2 = 0;
			foreach (byte b3 in input)
			{
				b = (byte)((int)b | b3 >> (int)(8 - b2));
				array[num2++] = Base32Encoding._ValueToChar(b);
				if (b2 < 4)
				{
					b = (byte)(b3 >> (int)(3 - b2) & 31);
					array[num2++] = Base32Encoding._ValueToChar(b);
					b2 += 5;
				}
				b2 -= 3;
				b = (byte)((int)b3 << (int)b2 & 31);
			}
			if (num2 != num)
			{
				array[num2++] = Base32Encoding._ValueToChar(b);
			}
			return new string(array).Replace("\0", "");
		}

		/**
		 * Convert int from 1 to 32 to char
		 * 
		 * @param byte b - num
		 * 
		 * @return char character
		 */
		private static char _ValueToChar(byte b)
		{
			if (b < 26)
			{
				return (char)(b + 97);
			}
			if (b < 32)
			{
				return (char)(b + 24);
			}
			throw new ArgumentException("Byte is not a value Base32 value.", "b");
		}
	}
}
