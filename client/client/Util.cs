using System;
using System.Linq;
using System.Threading;

namespace Alien
{
	internal class Util
	{
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
	}
}
