using System;
using System.Linq;
using System.Threading;

namespace Alien
{
	internal class Util
	{
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

		public static string MapBaseSubdomainCharacters(string data, string shuffle)
		{
			string text = string.Empty;
			for (int i = 0; i < data.Length; i++)
			{
				text += shuffle[Config.CharsDomain.IndexOf(data[i])].ToString();
			}
			return text;
		}
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

		public static void MakeDelay(int seconds)
		{
			Thread.Sleep(seconds);
		}

		public static string ConvertIntToDomain(int value)
		{
			return Util._IntToString(value, Config.CharsDomain);
		}

		public static string ConvertIntToCounter(int value)
		{
			return Util._IntToString(value, Config.CharsCounter);
		}

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
