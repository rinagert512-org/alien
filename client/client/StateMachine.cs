﻿using System;
using System.Collections.Generic;

namespace Alien
{
	internal class StateMachine
	{
		public MachineState CurrentState { get; private set; }
		public StateMachine()
		{
			this.CurrentState = MachineState.Begin;
			this.transitions = new Dictionary<StateMachine.StateTransition, MachineState>
			{
				{
					new StateMachine.StateTransition(MachineState.Begin, MachineCommand.Start),
					MachineState.Alive
				},
				{
					new StateMachine.StateTransition(MachineState.Sleep, MachineCommand.Start),
					MachineState.Alive
				},
				{
					new StateMachine.StateTransition(MachineState.Alive, MachineCommand.Failed),
					MachineState.Sleep
				},
				{
					new StateMachine.StateTransition(MachineState.Alive, MachineCommand.HasData),
					MachineState.Receive
				},
				{
					new StateMachine.StateTransition(MachineState.Receive, MachineCommand.Failed),
					MachineState.Sleep
				},
				{
					new StateMachine.StateTransition(MachineState.Receive, MachineCommand.DataReceived),
					MachineState.Do
				},
				{
					new StateMachine.StateTransition(MachineState.Do, MachineCommand.Failed),
					MachineState.SecondSleep
				},
				{
					new StateMachine.StateTransition(MachineState.Do, MachineCommand.HasResult),
					MachineState.Send
				},
				{
					new StateMachine.StateTransition(MachineState.Send, MachineCommand.Failed),
					MachineState.Sleep
				},
				{
					new StateMachine.StateTransition(MachineState.Send, MachineCommand.HasData),
					MachineState.SendAndReceive
				},
				{
					new StateMachine.StateTransition(MachineState.Send, MachineCommand.DataSended),
					MachineState.Do
				},
				{
					new StateMachine.StateTransition(MachineState.Send, MachineCommand.DataSendedAndHasData),
					MachineState.Receive
				},
				{
					new StateMachine.StateTransition(MachineState.SendAndReceive, MachineCommand.Failed),
					MachineState.Sleep
				},
				{
					new StateMachine.StateTransition(MachineState.SendAndReceive, MachineCommand.DataReceived),
					MachineState.Send
				},
				{
					new StateMachine.StateTransition(MachineState.SendAndReceive, MachineCommand.DataSended),
					MachineState.Receive
				},
				{
					new StateMachine.StateTransition(MachineState.SendAndReceive, MachineCommand.DataSendedAndReceived),
					MachineState.Do
				},
				{
					new StateMachine.StateTransition(MachineState.SecondSleep, MachineCommand.Start),
					MachineState.Alive
				}
			};
		}

		/**
		 * Get next state
		 * 
		 * @param MachineCommand command - command to move
		 * 
		 * @return MachineState - new state
		 */
		public MachineState GetNext(MachineCommand command)
		{
			StateMachine.StateTransition key = new StateMachine.StateTransition(this.CurrentState, command);
			MachineState result;
			if (!this.transitions.TryGetValue(key, out result))
			{
				throw new Exception("Invalid transition: " + this.CurrentState.ToString() + " -> " + command.ToString());
			}
			return result;
		}

		/**
		 * Move to next state
		 * 
		 * @param MachineCommand command - command to move
		 * 
		 * @return MachineState - new state
		 */
		public MachineState MoveNext(MachineCommand command)
		{
			this.CurrentState = this.GetNext(command);
			return this.CurrentState;
		}

		private Dictionary<StateMachine.StateTransition, MachineState> transitions;

		private class StateTransition
		{
			public StateTransition(MachineState currentState, MachineCommand command)
			{
				this.CurrentState = currentState;
				this.Command = command;
			}

			public override int GetHashCode()
			{
				return 17 + 31 * this.CurrentState.GetHashCode() + 31 * this.Command.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				StateMachine.StateTransition stateTransition = obj as StateMachine.StateTransition;
				return stateTransition != null 
					&& this.CurrentState == stateTransition.CurrentState 
					&& this.Command == stateTransition.Command;
			}

			private readonly MachineState CurrentState;
			private readonly MachineCommand Command;
		}
	}
}
