using Cysharp.Threading.Tasks;
using UnityEngine;
using FMOD.Studio;

namespace Game.Core.StateMachines.Game
{
	public class GameCreditsState : BaseGameState
	{
		public GameCreditsState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.SetDebugText("");

			await _ui.ShowCredits();
			await _ui.FadeOut();
		}

		public override void Tick()
		{
			if (_controls.Global.Cancel.WasPerformedThisFrame() || _controls.Global.Confirm.WasPerformedThisFrame())
			{
				_fsm.Fire(GameFSM.Triggers.Done);
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_state.LevelMusic.stop(STOP_MODE.ALLOWFADEOUT);
			await _ui.FadeIn(Color.black);
			await _ui.HideCredits();
		}
	}
}
