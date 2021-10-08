using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.StateMachines.Game
{
	public class GameTitleState : BaseGameState
	{
		public GameTitleState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		private CancellationTokenSource _cancellationSource;

		public override async UniTask Enter()
		{
			await base.Enter();

			_cancellationSource = new CancellationTokenSource();

			_ui.TitleButton1.onClick.AddListener(Start);
			_ui.TitleButton2.onClick.AddListener(Quit);

			_ = _audioPlayer.PlayMusic(_config.TitleClip, true, 3f, 1f, false);
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
				_ui.SetDebugText(@"[DEBUG]
- F1-F12: load levels
- Tab: toggle level selection
- K: start replay");
			}
		}

		public override void Tick()
		{
			if (Keyboard.current.escapeKey.wasReleasedThisFrame)
			{
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
					_state.CurrentTimeScale = 10f;
					_fsm.Fire(GameFSM.Triggers.LevelSelected);
				}
			}
		}

		public override async UniTask Exit()
		{
			_ = _ui.FadeIn(Color.black);
			await _audioPlayer.StopMusic(2f);

			_ui.TitleButton1.onClick.RemoveListener(Start);

			_cancellationSource.Cancel();
			_cancellationSource.Dispose();

			_ui.TitleButton1.onClick.RemoveListener(Start);
			_ui.TitleButton2.onClick.RemoveListener(Quit);

			await _ui.HideTitle();
			_ = _ui.HideLevelSelection();
			for (int i = 0; i < _ui.LevelButtons.Length; i++)
			{
				var button = _ui.LevelButtons[i];
				int levelIndex = i;
				button.onClick.RemoveListener(() => LoadLevel(levelIndex));
			}
		}

		private void LoadLevel(int levelIndex)
		{
			Debug.Log($"Loading level {levelIndex}.");
			_state.CurrentLevelIndex = levelIndex;
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}

		private void Start()
		{
			_state.CurrentLevelIndex = 0;
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}

		private async void Quit()
		{
			await _audioPlayer.StopMusic(2f);
			_fsm.Fire(GameFSM.Triggers.Quit);
		}
	}
}
