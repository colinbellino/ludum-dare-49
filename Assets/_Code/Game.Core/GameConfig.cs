using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Game Config")]
	public class GameConfig : ScriptableObject
	{
		[Header("DEBUG")]
		public bool DebugFSM;
		public int LockFPS = 60;
		public bool TakeScreenshots;

		[Header("CONTENT")]
		[UnityEngine.Serialization.FormerlySerializedAs("AllLevels")] public Level[] Levels;
		public TileToEntity TileToEntity;

		[Header("AUDIO")]
		public AudioClip MusicCalmClip;
		public AudioClip MusicAngryClip;
		public AudioClip TitleClip;

		[Header("FMOD")]
		public EventReference SoundMenuConfirm;
		public EventReference SoundLevelRestart;
		public EventReference MusicTitle;
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
