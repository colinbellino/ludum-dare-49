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

			_ui.TitleButton1.onClick.AddListener(Start);
			_ui.TitleButton2.onClick.AddListener(Quit);

			if (_running)
			{
				_ = _audioPlayer.PlayMusic(_config.TitleClip, true, 3f, 1f, false);
				await UniTask.Delay(2000);
				_ = _ui.FadeOut(2f);
				await UniTask.Delay(1200);
				await _ui.ShowTitle();

				if (Utils.IsDevBuild())
				{
					_ui.SetDebugText(@"[DEBUG]
- F1-F12: load levels");
				}
			}
		}

		public override void Tick()
		{
			if (Keyboard.current.escapeKey.wasReleasedThisFrame)
			{
				Quit();
			}

			if (Utils.IsDevBuild())
			{
				if (Keyboard.current.f1Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 0;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f2Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 1;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f3Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 2;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f4Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 3;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f5Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 4;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f6Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 5;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f7Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 6;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f8Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 7;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f8Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 7;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f9Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 8;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f10Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 9;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f11Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 10;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}

				if (Keyboard.current.f12Key.wasReleasedThisFrame)
				{
					_state.CurrentLevelIndex = 11;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}
			}
		}

		public override async UniTask Exit()
		{
			_running = false;

			_ui.TitleButton1.onClick.RemoveListener(Start);
			_ui.TitleButton2.onClick.RemoveListener(Quit);

			await _ui.HideTitle();
		}

		private async void Start()
		{
			_ = _ui.FadeIn(Color.black, 1f);
			await _audioPlayer.StopMusic(2f);
			_state.CurrentLevelIndex = 0;
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}

		private async void Quit()
		{
			_ = _ui.FadeIn(Color.black, 1f);
			await _audioPlayer.StopMusic(2f);

			_fsm.Fire(GameFSM.Triggers.Quit);
		}
	}
}
