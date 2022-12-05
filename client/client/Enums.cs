using System;

namespace Alien
{
	internal class Enums
	{
		public enum DomainType
		{
			FirstAlive,
			Send,
			Receive,
			SendAndReceive,
			MainAlive
		}

		public enum DelayType
		{
			Alive,
			Communicate,
			SecondCheck,
			Retry
		}

		public enum TaskType
		{
			Cmd = 70
		}

		public enum SendResult
		{
			Failed,
			DataSended,
			Sending,
			HasData,
			DataSendedAndHasData
		}
	}
}
