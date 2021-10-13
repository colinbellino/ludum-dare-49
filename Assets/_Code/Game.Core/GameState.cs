using System.Collections.Generic;
using NesScripts.Controls.PathFind;

namespace Game.Core
{
	public class GameState
	{
		public Unity.Mathematics.Random Random;
		public bool Running;
		public bool Paused;
		public float TimeScaleCurrent;
		public float TiemScaleDefault;
		public bool IsReplaying;
		public Level[] DebugLevels;
		public Level[] AllLevels;

		public float MusicVolume;
		public bool MusicMuted;
		public float SoundVolume;
		public bool SoundMuted;

		public int CurrentLevelIndex;
		public Level Level;
		public GridData WalkableGrid;
		public List<Entity> Entities = new List<Entity>(30);
		public int KeysPickedUp;
		public int KeysInLevel;
	}
}
