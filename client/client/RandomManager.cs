using System;

namespace Alien
{
	public static class RandomManager
	{
		/**
		 * Random number in certain range
		 * 
		 * @param int min - min in range
		 * @param int max - max in range
		 * 
		 * @return int random integer
		 */
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
