using System;

namespace Alien
{
	public static class RandomManager
	{
		public static int GetRandomRange(int min, int max)
		{
			if (min > max)
			{
				min = max;
			}
			return RandomManager._Random.Next(min, max + 1);
		}

		private static Random _Random = new Random();
	}
}
