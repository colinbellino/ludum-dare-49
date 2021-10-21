using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Stateless;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Core.StateMachines.Game
{
	public class GameFSM
	{
		public enum States { Init, Title, SelectLevel, LoadLevel, Gameplay, Quit }
		public enum Triggers { Done, Won, Lost, Retry, NextLevel, LevelSelected, LevelSelectionRequested, Quit }

		private readonly bool _debug;
		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;

		public GameFSM(bool debug, GameSingleton game)
		{
			Assert.IsNotNull(game);

			_debug = debug;
			_states = new Dictionary<States, IState>
			{
				{ States.Init, new GameInitState(this, game) },
				{ States.Title, new GameTitleState(this, game) },
				{ States.SelectLevel, new GameSelectLevelState(this, game) },
				{ States.LoadLevel, new GameLoadLevelState(this, game) },
				{ States.Gameplay, new GameGameplayState(this, game) },
				{ States.Quit, new GameQuitState(this, game) },
			};

			_machine = new StateMachine<States, Triggers>(States.Init);
			_machine.OnTransitioned(OnTransitioned);

			_machine.Configure(States.Init)
				.Permit(Triggers.Done, States.Title);

			_machine.Configure(States.Title)
				.Permit(Triggers.LevelSelected, States.LoadLevel)
				.Permit(Triggers.LevelSelectionRequested, States.SelectLevel)
				.Permit(Triggers.Quit, States.Quit);

			_machine.Configure(States.SelectLevel)
				.Permit(Triggers.LevelSelected, States.LoadLevel)
				.Permit(Triggers.Quit, States.Quit);

			_machine.Configure(States.LoadLevel)
				.Permit(Triggers.Done, States.Gameplay);

			_machine.Configure(States.Gameplay)
				.Permit(Triggers.Won, States.Title)
				.Permit(Triggers.Quit, States.Quit)
				.Permit(Triggers.NextLevel, States.LoadLevel)
				.Permit(Triggers.Retry, States.LoadLevel);

			_currentState = _states[_machine.State];
		}

		public UniTask Start() => _currentState.Enter();

		public void Tick() => _currentState?.Tick();

		public void FixedTick() => _currentState?.FixedTick();

		public void Fire(Triggers trigger)
		{
			if (_machine.CanFire(trigger))
				_machine.Fire(trigger);
			else
				Debug.LogWarning("GameFSM: Invalid transition " + _machine.State + " -> " + trigger);
		}

		private async void OnTransitioned(StateMachine<States, Triggers>.Transition transition)
		{
			if (_currentState != null)
			{
				// Log($"GameFSM: {transition.Source}.Exit (before)");
				await _currentState.Exit();
				// Log($"GameFSM: {transition.Source}.Exit (after)");
			}

			if (_states.ContainsKey(transition.Destination) == false)
				LogError("GameFSM: Missing state class for: " + transition.Destination);

			_currentState = _states[transition.Destination];
			Log($"GameFSM: {transition.Source} -> {transition.Destination}");

			// Log($"GameFSM: {transition.Destination}.Enter (before)");
			await _currentState.Enter();
			// Log($"GameFSM: {transition.Destination}.Enter (after)");
		}

		private void Log(object message)
		{
			if (_debug == false)
				return;
			UnityEngine.Debug.Log(message);
		}

		private void LogError(object message)
		{
			if (_debug == false)
				return;
			UnityEngine.Debug.LogError(message);
		}
	}
}
