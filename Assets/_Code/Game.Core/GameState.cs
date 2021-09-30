using System.Collections.Generic;
using UnityEngine;

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

		public Level Level;
		public bool PlayerDidAct;
		public List<Entity> Entities = new List<Entity>(30);
	}
}
