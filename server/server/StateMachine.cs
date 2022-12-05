using System;
using System.Collections.Generic;

namespace Alien
{
	internal class StateMachine
	{
		public MachineState CurrentState { get; private set; }
		public StateMachine()
		{
		
		}

		public MachineState GetNext()
		{
			return 0;
		}

		public MachineState MoveNext()
		{
			return 0;
		}
	}
}
