using System.Linq;
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

			_ui.PauseButton1.onClick.AddListener(ToggleSounds);
			_ui.PauseButton2.onClick.AddListener(ToggleMusic);
			_ui.PauseButton3.onClick.AddListener(QuitGame);

			_state.CurrentTimeScale = _state.DefaultTimeScale = 1f;

			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)Random.Range(0, int.MaxValue));

			if (IsDevBuild())
			{
				_state.DebugLevels = Resources.LoadAll<Level>("Levels/Debug");
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

			_fsm.Fire(GameFSM.Triggers.Done);
		}

		private void ToggleSounds()
		{
			_audioPlayer.SetSoundVolume(_state.IsSoundPlaying ? 0 : 1);
			_ui.PauseButton1.GetComponentInChildren<TMPro.TMP_Text>().text = "Sound:" + (_state.IsSoundPlaying ? "OFF" : "ON");
			_state.IsSoundPlaying = !_state.IsSoundPlaying;
		}

		private void ToggleMusic()
		{
			_audioPlayer.SetMusicVolume(_state.IsMusicPlaying ? 0 : 0.1f);
			_ui.PauseButton2.GetComponentInChildren<TMPro.TMP_Text>().text = "Music:" + (_state.IsMusicPlaying ? "OFF" : "ON");
			_state.IsMusicPlaying = !_state.IsMusicPlaying;
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
