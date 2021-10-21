using System.Threading;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.StateMachines.Game
{
	public class GameTitleState : BaseGameState
	{
		private CancellationTokenSource _cancellationSource;
		private EventInstance _music;

		public GameTitleState(GameFSM fsm, GameSingleton game) : base(fsm, game)
		{
			_music = FMODUnity.RuntimeManager.CreateInstance(_config.MusicTitle);
		}

		public override async UniTask Enter()
		{
			await base.Enter();

			_cancellationSource = new CancellationTokenSource();

			_ui.StartButton.onClick.AddListener(StartGame);
			_ui.OptionsButton.onClick.AddListener(ToggleOptions);
			_ui.QuitButton.onClick.AddListener(Quit);

			_music.start();

			await UniTask.Delay(2000, cancellationToken: _cancellationSource.Token);
			_ = _ui.FadeOut(2f);
			await UniTask.Delay(1200, cancellationToken: _cancellationSource.Token);
			await _ui.ShowTitle(_cancellationSource.Token);
			for (int i = 0; i < _ui.LevelButtons.Length; i++)
			{
				var button = _ui.LevelButtons[i];
				int levelIndex = i;
				button.onClick.AddListener(() => LoadLevel(levelIndex));
			}

			if (Utils.IsDevBuild())
			{
				_ui.SetDebugText(@"
- F1-F12: load levels
- L: load last level
- K: start replay
- Tab: level selection
");
			}
		}

		public override void Tick()
		{
			if (Keyboard.current.escapeKey.wasReleasedThisFrame)
			{
				if (_game.Pause.IsOpened)
				{
					_game.Pause.Hide();
					_game.Save.SavePlayerSettings(_game.State.PlayerSettings);
				}
				else
					Quit();
			}

			if (Keyboard.current.tabKey.wasReleasedThisFrame) { _ui.ToggleLevelSelection(); }

			if (Utils.IsDevBuild())
			{
				if (Keyboard.current.f1Key.wasReleasedThisFrame) { LoadLevel(0); }
				if (Keyboard.current.f2Key.wasReleasedThisFrame) { LoadLevel(1); }
				if (Keyboard.current.f3Key.wasReleasedThisFrame) { LoadLevel(2); }
				if (Keyboard.current.f4Key.wasReleasedThisFrame) { LoadLevel(3); }
				if (Keyboard.current.f5Key.wasReleasedThisFrame) { LoadLevel(4); }
				if (Keyboard.current.f6Key.wasReleasedThisFrame) { LoadLevel(5); }
				if (Keyboard.current.f7Key.wasReleasedThisFrame) { LoadLevel(6); }
				if (Keyboard.current.f8Key.wasReleasedThisFrame) { LoadLevel(7); }
				if (Keyboard.current.f8Key.wasReleasedThisFrame) { LoadLevel(7); }
				if (Keyboard.current.f9Key.wasReleasedThisFrame) { LoadLevel(8); }
				if (Keyboard.current.f10Key.wasReleasedThisFrame) { LoadLevel(9); }
				if (Keyboard.current.f11Key.wasReleasedThisFrame) { LoadLevel(10); }
				if (Keyboard.current.f12Key.wasReleasedThisFrame) { LoadLevel(11); }
				if (Keyboard.current.lKey.wasReleasedThisFrame) { LoadLevel(_config.Levels.Length - 1); }

				if (Keyboard.current.kKey.wasReleasedThisFrame)
				{
					UnityEngine.Debug.Log("Starting in replay mode.");
					_state.CurrentLevelIndex = 0;
					_state.IsReplaying = true;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}
			}
		}

		public override async UniTask Exit()
		{
			_music.stop(STOP_MODE.ALLOWFADEOUT);

			_cancellationSource.Cancel();
			_cancellationSource.Dispose();

			_ui.StartButton.onClick.RemoveListener(StartGame);
			_ui.OptionsButton.onClick.RemoveListener(ToggleOptions);
			_ui.QuitButton.onClick.RemoveListener(Quit);

			await _ui.HideTitle();
			_ = _ui.HideLevelSelection();
			for (int i = 0; i < _ui.LevelButtons.Length; i++)
			{
				var button = _ui.LevelButtons[i];
				int levelIndex = i;
				button.onClick.RemoveListener(() => LoadLevel(levelIndex));
			}
		}

		private async void LoadLevel(int levelIndex)
		{
			Debug.Log($"Loading level {levelIndex}.");
			_state.CurrentLevelIndex = levelIndex;
			await _ui.FadeIn(Color.black, 0);
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}

		private async void StartGame()
		{
			_state.CurrentLevelIndex = 0;
			await _ui.FadeIn(Color.black);
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}

		private void ToggleOptions()
		{
			if (_game.Pause.IsOpened)
			{
				_game.Pause.Hide();
				_game.Save.SavePlayerSettings(_game.State.PlayerSettings);
			}
			else
				_ = _game.Pause.Show("Options", false);
		}

		private void Quit()
		{
			_fsm.Fire(GameFSM.Triggers.Quit);
		}
	}
}
