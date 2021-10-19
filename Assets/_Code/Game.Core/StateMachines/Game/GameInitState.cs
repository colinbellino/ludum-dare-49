using Cysharp.Threading.Tasks;
using UnityEngine;
using static Game.Core.Utils;

namespace Game.Core.StateMachines.Game
{
	public class GameInitState : BaseGameState
	{
		public GameInitState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.TimeScaleCurrent = _state.TimeScaleDefault = 1f;
			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)Random.Range(0, int.MaxValue));
			_state.DebugLevels = new Level[0];
			_state.AllLevels = _config.Levels;
			_state.MusicVolume = 1;
			_state.SoundVolume = 1;

			if (IsDevBuild())
			{
				_state.DebugLevels = Resources.LoadAll<Level>("Levels/Debug");
				_state.AllLevels = new Level[_config.Levels.Length + _state.DebugLevels.Length];
				_config.Levels.CopyTo(_state.AllLevels, 0);
				_state.DebugLevels.CopyTo(_state.AllLevels, _config.Levels.Length);

				_ui.ShowDebug();

				if (_config.LockFPS > 0)
				{
					Debug.Log($"Locking FPS to {_config.LockFPS}");
					Application.targetFrameRate = _config.LockFPS;
					QualitySettings.vSyncCount = 1;
				}
				else
				{
					Application.targetFrameRate = 999;
					QualitySettings.vSyncCount = 0;
				}
			}

			_game.Pause.SoundButton.onClick.AddListener(ToggleSounds);
			_game.Pause.MusicButton.onClick.AddListener(ToggleMusic);
			_game.Pause.FullscreenButton.onClick.AddListener(ToggleFullscreen);
			_game.Pause.QuitButton.onClick.AddListener(QuitGame);

			_ = _ui.FadeIn(Color.black, 0);

			_fsm.Fire(GameFSM.Triggers.Done);
		}

		private void ToggleSounds()
		{
			_state.SoundMuted = !_state.SoundMuted;
			_game.Pause.SoundButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Sound:" + (_state.SoundMuted ? "OFF" : "ON");
		}

		private void ToggleMusic()
		{
			_state.MusicMuted = !_state.MusicMuted;
			_game.Pause.MusicButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Music:" + (_state.MusicMuted ? "OFF" : "ON");
		}

		private void ToggleFullscreen()
		{
			Screen.fullScreen = !Screen.fullScreen;
			_game.Pause.FullscreenButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Fullscreen:" + (Screen.fullScreen ? "OFF" : "ON");
		}

		private void QuitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			UnityEngine.Application.Quit();
#endif
		}
	}
}
