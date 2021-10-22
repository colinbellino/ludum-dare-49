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
			await _ui.ShowTitle(_state.PlayerSaveData.ClearedLevels.Count == 0 ? "Start" : "Continue", _cancellationSource.Token);

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

		public override async void Tick()
		{
			if (_controls.Global.Cancel.WasPerformedThisFrame())
			{
				if (_game.OptionsUI.IsOpened)
				{
					await _game.OptionsUI.Hide();
					_game.UI.SelectTitleOptionsGameObject();
					_game.Save.SavePlayerSettings(_game.State.PlayerSettings);
				}
				else
					Quit();
			}

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
					_state.IsReplaying = true;
					LoadLevel(0);
				}
			}
		}

		public override UniTask Exit()
		{
			_music.stop(STOP_MODE.ALLOWFADEOUT);

			_cancellationSource.Cancel();
			_cancellationSource.Dispose();

			_ui.StartButton.onClick.RemoveListener(StartGame);
			_ui.OptionsButton.onClick.RemoveListener(ToggleOptions);
			_ui.QuitButton.onClick.RemoveListener(Quit);

			return default;
		}

		private async void LoadLevel(int levelIndex)
		{
			Debug.Log($"Loading level {levelIndex}.");
			_state.CurrentLevelIndex = levelIndex;
			await _ui.FadeIn(Color.black, 0);
			await _ui.HideTitle(0);
			await _game.OptionsUI.Hide(0);
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}

		private async void StartGame()
		{
			await _ui.HideTitle();
			await _ui.FadeIn(Color.black);

			if (_state.PlayerSaveData.ClearedLevels.Count == 0)
			{
				_state.CurrentLevelIndex = 0;
				_fsm.Fire(GameFSM.Triggers.LevelSelected);

				return;
			}

			_fsm.Fire(GameFSM.Triggers.LevelSelectionRequested);
		}

		private void ToggleOptions()
		{
			_ = _game.OptionsUI.Show();
		}

		private void Quit()
		{
			_fsm.Fire(GameFSM.Triggers.Quit);
		}
	}
}
