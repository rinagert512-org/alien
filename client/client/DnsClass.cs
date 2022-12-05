using System;
using System.Linq;
using System.Net;

namespace Alien
{
	internal class DnsClass
	{
		public static void ReadySend(byte[] sendData)
		{
			DnsClass._SendByteIndex = 0;
			DnsClass._SendDataSize = sendData.Length;
			DnsClass._SendData = sendData;
		}

		
		private static string _Domain;
		private static int _Try;
		private static int _ReceiveDataSize;
		private static int _ReceiveByteIndex;
		private static byte[] _ReceiveData;
		private static int _SendDataSize;
		private static int _SendByteIndex;
		private static byte[] _SendData;
	}
}
