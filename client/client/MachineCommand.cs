using System;

namespace Alien
{
	public enum MachineCommand
	{
		Start,
		Failed,
		HasData,
		DataReceived,
		HasResult,
		DataSended,
		DataSendedAndHasData,
		DataSendedAndReceived
	}
}
