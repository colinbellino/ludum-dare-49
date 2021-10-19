using System.Collections.Generic;
using FMOD.Studio;
using NesScripts.Controls.PathFind;

namespace Game.Core
{
	public class GameState
	{
		public Unity.Mathematics.Random Random;
		public bool Running;
		public bool Paused;
		public float TimeScaleCurrent;
		public float TimeScaleDefault;
		public bool IsReplaying;
		public Level[] DebugLevels;
		public Level[] AllLevels;

		public Bus GameBus;
		public Bus MusicBus;
		public Bus SoundBus;

		public int CurrentLevelIndex;
		public Level Level;
		public GridData WalkableGrid;
		public List<Entity> Entities = new List<Entity>(30);
		public int KeysPickedUp;
		public int KeysInLevel;
	}
}
