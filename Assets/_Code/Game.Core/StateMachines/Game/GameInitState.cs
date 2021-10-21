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

			FMODUnity.RuntimeManager.LoadBank("SFX", loadSamples: true);

			_state.GameBus = FMODUnity.RuntimeManager.GetBus("bus:/Game");
			_state.MusicBus = FMODUnity.RuntimeManager.GetBus("bus:/Game/Music");
			_state.SoundBus = FMODUnity.RuntimeManager.GetBus("bus:/Game/SFX");
			_state.TimeScaleCurrent = _state.TimeScaleDefault = 1f;
			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)Random.Range(0, int.MaxValue));
			_state.DebugLevels = new Level[0];
			_state.AllLevels = _config.Levels;

			await _game.UI.Init(_game);
			await _game.Pause.Init(_game);

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

			await _ui.FadeIn(Color.black, 0);

			_fsm.Fire(GameFSM.Triggers.Done);
		}
	}
}
