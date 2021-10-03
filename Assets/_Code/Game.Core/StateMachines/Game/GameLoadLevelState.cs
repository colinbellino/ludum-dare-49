using Cysharp.Threading.Tasks;
using UnityEngine;
using static Game.Core.Utils;

namespace Game.Core.StateMachines.Game
{
	public class GameLoadLevelState : BaseGameState
	{
		public GameLoadLevelState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_fsm.Fire(GameFSM.Triggers.Done);
		}
	}
}
