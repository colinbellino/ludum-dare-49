using System;
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
