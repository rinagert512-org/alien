using System;
using System.Linq;
using System.Threading;

namespace Alien
{
	internal class Util
	{
		/**
		 * Shuffle text with mersenne twister
		 * 
		 * @param int seed - seed
		 * 
		 * @return string - shuffled text
		 */
		public static string Shuffle(int seed)
		{
			string text = Config.CharsDomain;
			int length = text.Length;
			string text2 = string.Empty;
			RandomMersenneTwister randomMersenneTwister = new RandomMersenneTwister((uint)seed);
			for (int i = 0; i < length; i++)
			{
				int randomRange = randomMersenneTwister.GetRandomRange(0, text.Length);
				text2 += text[randomRange].ToString();
				text = text.Remove(randomRange, 1);
			}
			return text2;
		}

		/**
		 * Maps text to shuffle
		 * 
		 * @param string data - data to be shuffled
		 * @param string shuffle - shuffle alphabet
		 * 
		 * @return string shuffled data
		 */
		public static string MapBaseSubdomainCharacters(string data, string shuffle)
		{
			string text = string.Empty;
			for (int i = 0; i < data.Length; i++)
			{
				text += shuffle[Config.CharsDomain.IndexOf(data[i])].ToString();
			}
			return text;
		}

		/**
		 * Logger function
		 * 
		 * @param string log - message to log
		 * 
		 * @return none
		 */
		public static void Log(string log)
		{
			Console.Write(
				string.Concat(new string[]
				{
					"[ ",
					DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
					" ] ",
					log,
					Environment.NewLine
				})
			);
		}

		/*
		 * Byte array to integer
		 * 
		 * @param byte[] value - byte array
		 * 
		 * @return int converted 
		 */
		public static int GetInt(byte[] value)
		{
			return BitConverter.ToInt32(Util._FixLittleEndian(value), 0);
		}

		/**
		 * Fix little endian
		 * 
		 * @param byte[] value - little endian
		 * 
		 * @return byte[] fixed le
		 */
		private static byte[] _FixLittleEndian(byte[] value)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(value);
				return value.Concat(new byte[1]).ToArray<byte>();
			}
			return new byte[1].Concat(value).ToArray<byte>();
		}

		/**
		 * Makes delay
		 * 
		 * @param int seconds - seconds to sleep
		 * 
		 * @return none
		 */
		public static void MakeDelay(int seconds)
		{
			Thread.Sleep(seconds);
		}

		/**
		 * Convert int in domain chars
		 * 
		 * @param int value - int to convert
		 * 
		 * @return string converted int
		 */
		public static string ConvertIntToDomain(int value)
		{
			return Util._IntToString(value, Config.CharsDomain);
		}

		/**
		 * Convert int in counter chars
		 * 
		 * @param int value - int to convert
		 * 
		 * @return string converted int
		 */
		public static string ConvertIntToCounter(int value)
		{
			return Util._IntToString(value, Config.CharsCounter);
		}

		/**
		 * Convert int to base
		 * 
		 * @param int value - int to convert
		 * @param string baseString - base
		 * 
		 * @return string converted int
		 */
		private static string _IntToString(int value, string baseString)
		{
			string text = string.Empty;
			int length = baseString.Length;
			do
			{
				text = baseString[value % length].ToString() + text;
				value /= length;
			}
			while (value > 0);
			return text;
		}
	}
}
