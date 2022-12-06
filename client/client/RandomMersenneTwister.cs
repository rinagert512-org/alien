using System;

namespace Alien
{
	internal class RandomMersenneTwister
	{
		/**
		 * Constructor of RandomMersenneTwister generator
		 * 
		 * @param uint seed - seed to start randomization
		 */
		public RandomMersenneTwister(uint seed = 5489U)
		{
			this.state = new uint[624];
			this.f = 1812433253U;
			this.m = 397U;
			this.u = 11U;
			this.s = 7U;
			this.b = 2636928640U;
			this.t = 15U;
			this.c = 4022730752U;
			this.l = 18U;
			this.index = 624U;
			this.lower_mask = 2147483647U;
			this.upper_mask = 2147483648U;
			this.state[0] = seed;
			for (int i = 1; i < 624; i++)
			{
				this.state[i] = this.toInt32((long)((ulong)(this.f * (this.state[i - 1] ^ this.state[i - 1] >> 30)) + (ulong)((long)i)));
			}
		}

		/**
		 * Twist
		 * 
		 * @return none
		 */
		private void twist()
		{
			for (uint num = 0U; num < 624U; num += 1U)
			{
				uint num2 = this.toInt32((long)((ulong)((this.state[(int)num] & this.upper_mask) + (this.state[(int)((num + 1U) % 624U)] & this.lower_mask))));
				uint num3 = num2 >> 1;
				if (num2 % 2U != 0U)
				{
					num3 = this.toInt32((long)((ulong)(num3 ^ 2567483615U)));
				}
				this.state[(int)num] = (this.state[(int)((num + this.m) % 624U)] ^ num3);
			}
			this.index = 0U;
		}

		/**
		 * Returns random number
		 * 
		 * @return uint random number
		 */
		public uint GetRandomNumber()
		{
			if (this.index >= 624U)
			{
				this.twist();
			}
			uint num = this.state[(int)this.index];
			num ^= num >> (int)this.u;
			num ^= (num << (int)this.s & this.b);
			num ^= (num << (int)this.t & this.c);
			num ^= num >> (int)this.l;
			this.index += 1U;
			return this.toInt32((long)((ulong)num));
		}

		/**
		 * Random number in certain range
		 * 
		 * @param int min - min border
		 * @param int max - max border
		 * 
		 * @return int random integer
		 */
		public int GetRandomRange(int min, int max)
		{
			int num = max - min;
			uint randomNumber = this.GetRandomNumber();
			return (int)((long)min + (long)((ulong)randomNumber % (ulong)((long)num)));
		}

		/**
		 * To 32bit integer
		 * 
		 * @param long number - number to convert
		 * 
		 * @return uint 32bit integer
		 */
		private uint toInt32(long number)
		{
			unchecked {
				return (uint)(number & (long)((ulong)-1));
			}
		}

		private uint[] state;
		private uint f;
		private uint m;
		private uint u;
		private uint s;
		private uint b;
		private uint t;
		private uint c;
		private uint l;
		private uint index;
		private uint lower_mask;
		private uint upper_mask;
	}
}
