using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.StateMachines.Game
{
	public class GameTitleState : BaseGameState
	{
		public GameTitleState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		private bool _running;

		public override async UniTask Enter()
		{
			await base.Enter();

			_running = true;

			await _ui.ShowTitle();

			_ui.TitleButton1.onClick.AddListener(Start);
			_ui.TitleButton2.onClick.AddListener(Quit);

			if (Utils.IsDevBuild())
			{
				_ui.SetDebugText(@"[DEBUG]
- F1-F12: load levels");

				await UniTask.Delay(5000);

				if (_running)
				{
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}
			}
		}

		public override void Tick()
		{
			if (Keyboard.current.f1Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 0;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f2Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 1;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f3Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 2;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f4Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 3;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f5Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 4;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f6Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 5;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f7Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 6;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f8Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 7;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f8Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 7;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f9Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 8;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f10Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 9;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f11Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 10;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}

			if (Keyboard.current.f12Key.wasPressedThisFrame)
			{
				_state.CurrentLevelIndex = 11;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);
			}
		}

		public override async UniTask Exit()
		{
			_running = false;

			_ui.TitleButton1.onClick.RemoveListener(Start);
			_ui.TitleButton2.onClick.RemoveListener(Quit);

			await _ui.HideTitle();
		}

		private void Start()
		{
			_state.CurrentLevelIndex = 0;
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}

		private void Quit()
		{
			_fsm.Fire(GameFSM.Triggers.Quit);
		}
	}
}