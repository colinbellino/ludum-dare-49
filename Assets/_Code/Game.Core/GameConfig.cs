using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game/Game Config")]
	public class GameConfig : ScriptableObject
	{
		[Header("Debug")]
		public bool DebugFSM;
		public int LockFPS = 60;

		[Header("Replay")]
		public bool TakeScreenshots;

		[Header("Level")]
		public Level[] AllLevels;

		[Header("Grid")]
		public TileToEntity TileToEntity;

		[Header("Audio")]
		public AudioMixer AudioMixer;
		public AudioMixerGroup MusicAudioMixerGroup;
		public AudioMixerGroup SoundsAudioMixerGroup;
		public AudioMixerSnapshot DefaultAudioSnapshot;
		public AudioMixerSnapshot PauseAudioSnapshot;
		public AudioClip MusicCalmClip;
		public AudioClip MusicAngryClip;
		public AudioClip TitleClip;
		public AudioClip MenuTextAppearClip;
		public AudioClip MenuConfirmClip;
		public AudioClip RestartClip;
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
