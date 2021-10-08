using System.Collections.Generic;
using NesScripts.Controls.PathFind;

namespace Game.Core
{
	public class GameState
	{
		public Unity.Mathematics.Random Random;
		public bool Running;
		public bool IsMusicPlaying = true;
		public bool IsSoundPlaying = true;
		public float CurrentTimeScale;
		public float DefaultTimeScale;
		public bool IsReplaying;
		public Level[] DebugLevels;
		public Level[] AllLevels;

		public int CurrentLevelIndex;
		public Level Level;
		public GridData WalkableGrid;
		public List<Entity> Entities = new List<Entity>(30);
		public int KeysPickedUp;
		public int KeysInLevel;
	}
}
