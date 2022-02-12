using Cysharp.Threading.Tasks;
using FMOD.Studio;
using UnityEngine;

namespace Game.Core.StateMachines.Game
{
	public class GameSelectLevelState : BaseGameState
	{
		public GameSelectLevelState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			await _ui.ShowLevelSelection(0);
			for (int i = 0; i < _ui.LevelButtons.Length; i++)
			{
				var button = _ui.LevelButtons[i];
				int levelIndex = i;
				button.Button.onClick.AddListener(() => LoadLevel(levelIndex));
			}

			await _ui.FadeOut();
		}

		public override void Tick()
		{
			if (_controls.Global.Cancel.WasPerformedThisFrame())
			{
				_state.LevelMusic.stop(STOP_MODE.ALLOWFADEOUT);
				_fsm.Fire(GameFSM.Triggers.Quit);
			}
		}

		public override async UniTask Exit()
		{
			for (int i = 0; i < _ui.LevelButtons.Length; i++)
			{
				var button = _ui.LevelButtons[i];
				int levelIndex = i;
				button.Button.onClick.RemoveListener(() => LoadLevel(levelIndex));
			}

			await _ui.FadeIn(Color.black);
			await _ui.HideLevelSelection(0);
		}

		private async void LoadLevel(int levelIndex)
		{
			Debug.Log($"Loading level {levelIndex}.");
			_state.CurrentLevelIndex = levelIndex;
			_state.TitleMusic.stop(STOP_MODE.ALLOWFADEOUT);
			await _ui.FadeIn(Color.black);
			_fsm.Fire(GameFSM.Triggers.LevelSelected);
		}
	}
}
