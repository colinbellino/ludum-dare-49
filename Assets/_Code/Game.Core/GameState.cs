using System.Collections.Generic;
using FMOD.Studio;
using NesScripts.Controls.PathFind;
using UnityEngine;
using System;

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

		public PlayerSettings PlayerSettings;
	}

	[Serializable]
	public struct PlayerSettings
	{
		public float GameVolume;
		public float SoundVolume;
		public float MusicVolume;

		public bool FullScreen;
		public int ResolutionWidth;
		public int ResolutionHeight;
		public int ResolutionRefreshRate;
	}
}
