using System;
using System.Threading;

namespace Alien
{
	public class Program
	{
		private static void Main(string[] args)
		{
			bool flag;
			using (new Mutex(true, "rand-id-767223923742314124", out flag))
			{
				if (flag)
				{
					StateMachine stateMachine = new StateMachine();
					Config.Init();
					while (true)
					{
						try
						{
							Util.Log(string.Format("State : {0}", stateMachine.CurrentState));
							switch (stateMachine.CurrentState)
							{
								case MachineState.Begin:
									stateMachine.MoveNext();
									break;
								case MachineState.Sleep:
									stateMachine.MoveNext();
									break;
								case MachineState.Alive:
									stateMachine.MoveNext();
									break;
								case MachineState.Receive:
									stateMachine.MoveNext();
									break;
								case MachineState.Do:
									stateMachine.MoveNext();
									break;
								case MachineState.Send:
									stateMachine.MoveNext();
									break;
								case MachineState.SendAndReceive:
									stateMachine.MoveNext();
									break;
								case MachineState.SecondSleep:
									stateMachine.MoveNext();
									break;
							}
						}
						catch (Exception ex)
						{
							Util.Log(ex.ToString());
						}
					}
				}
			}
		}
	}
}
