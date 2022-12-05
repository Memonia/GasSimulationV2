using System.ComponentModel;

using ActionDelegate = System.Action;

namespace GasSimulationV2.GUI.Actions
{
	internal class ActionStateMachine
    {
		public event ActionDelegate? OnStart;
		public event ActionDelegate? OnStep;
		public event ActionDelegate? OnPause;
		public event ActionDelegate? OnResume;
		public event ActionDelegate? OnReset;
		
		private ActionState _currentState;

		public ActionStateMachine(
			ActionState initState = ActionState.BeforeStart)
		{
			_currentState = initState;
		}

		public void Switch(Action next)
		{
			switch (next)
			{
				case Action.Start:
					_startSwitch();
					break;	
				
				case Action.Step:
					_stepSwitch();
					break;

				case Action.Pause:
					_pauseSwitch();
					break;

				case Action.Resume:
					_resumeSwitch();
					break;	
				
				case Action.Reset:
					_resetSwitch();
					break;
			}
		}

		private void _startSwitch()
		{
			switch (_currentState)
			{
				case ActionState.BeforeStart:
				case ActionState.AfterReset:
				case ActionState.AfterStep:
					_currentState = ActionState.AfterStart;
					break;

				default:
					_throwInvalidAction(Action.Start);
					return;
			}

			OnStart?.Invoke();
		}

		private void _stepSwitch()
		{
			switch (_currentState)
			{
				case ActionState.BeforeStart:
				case ActionState.AfterReset:
				case ActionState.AfterStep:
					_currentState = ActionState.AfterStep;
					break;

				default:
					_throwInvalidAction(Action.Step);
					return;
			}

			OnStep?.Invoke();
		}

		private void _pauseSwitch()
		{
			switch (_currentState)
			{
				case ActionState.AfterStart:
				case ActionState.AfterStep:
				case ActionState.AfterResume:
					_currentState = ActionState.AfterPause;
					break;

				default:
					_throwInvalidAction(Action.Pause);
					return;
			}

			OnPause?.Invoke();
		}

		private void _resumeSwitch()
		{
			switch (_currentState)
			{
				case ActionState.AfterReset:
				case ActionState.AfterPause:
					_currentState = ActionState.AfterResume;
					break;

				default:
					_throwInvalidAction(Action.Resume);
					return;
			}

			OnResume?.Invoke();
		}

		private void _resetSwitch()
		{
			switch (_currentState)
			{
				case ActionState.BeforeStart:
				case ActionState.AfterStep:
				case ActionState.AfterPause:
					_currentState = ActionState.AfterReset;
					break;

				case ActionState.AfterReset:
					break;

				default:
					_throwInvalidAction(Action.Reset);
					return;
			}

			OnReset?.Invoke();
		}

		private void _throwInvalidAction(Action attemptedAction)
		{
			throw new InvalidEnumArgumentException(
				$"Can't switch to {attemptedAction} at {_currentState}"
			);
		}
	}
}
