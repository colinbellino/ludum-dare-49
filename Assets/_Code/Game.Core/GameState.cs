using System.Collections.Generic;
using NesScripts.Controls.PathFind;

namespace Game.Core
{
	public class GameState
	{
		public Unity.Mathematics.Random Random;
		public bool Running;
		public bool AssistMode;
		public float StepSoundTimestamp;
		public float NextPlayerInput;
		public int CurrentLevelIndex;
		public Level Level;
		public bool PlayerDidAct;
		public List<Entity> Entities = new List<Entity>(30);
		public GridData WalkableGrid;
		public float TriggerExitAt;
		public bool TriggerRetry;
		public int KeysPickedUp;
		public int KeysInLevel;
		public bool IsMusicPlaying = true;
		public bool IsSoundPlaying = true;
		public bool IsReplaying;
	}
}
