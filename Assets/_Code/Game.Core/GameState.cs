﻿using UnityEngine;

namespace Game.Core
{
	public class GameState
	{
		public float InitialMusicVolume;
		public float CurrentMusicVolume;
		public float InitialSoundVolume;
		public float CurrentSoundVolume;
		public Entity Player;
		public Unity.Mathematics.Random Random;
		public bool Running;
		public bool AssistMode;
		public float StepSoundTimestamp;
	}
}
