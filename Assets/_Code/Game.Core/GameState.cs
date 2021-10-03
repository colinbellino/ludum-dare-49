using System.Collections.Generic;
using NesScripts.Controls.PathFind;

namespace Game.Core
{
	public class GameState
	{
		public float InitialMusicVolume;
		public float CurrentMusicVolume;
		public float InitialSoundVolume;
		public float CurrentSoundVolume;
		public Unity.Mathematics.Random Random;
		public bool Running;
		public bool AssistMode;
		public float StepSoundTimestamp;
		public int CurrentLevelIndex;
		public Level Level;
		public bool PlayerDidAct;
		public List<Entity> Entities = new List<Entity>(30);
		public GridData WalkableGrid;
		public float TriggerExitAt;
		public float TriggerRetryAt;
		public int Keys;
	}
}
