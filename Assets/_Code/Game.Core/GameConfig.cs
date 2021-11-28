using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Game Config")]
	public class GameConfig : ScriptableObject
	{
		[Header("DEBUG")]
		public bool DebugFSM;
		public bool DebugLevels;
		public int LockFPS = 60;

		[Header("CONTENT")]
		public Level[] Levels;
		public TileToEntity TileToEntity;

		[Header("AUDIO")]
		public EventReference SnapshotPause;
		public EventReference SoundMenuConfirm;
		public EventReference SoundLevelRestart;
		public EventReference MusicTitle;
		public EventReference MusicLevel;
	}

	[Serializable]
	public class TileToEntity : SerializableDictionary<TileBase, Entity> { }

	[Serializable]
	public class TileToInfo : SerializableDictionary<TileBase, TileInfo> { }

	[Serializable]
	public class TileInfo
	{
		public bool Walkable;
	}
}
