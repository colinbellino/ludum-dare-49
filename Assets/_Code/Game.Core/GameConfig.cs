using System;
using System.Collections.Generic;
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

		[Header("Grid")]
		public TileToEntity TileToEntity;

		[Header("Audio")]
		public AudioMixer AudioMixer;
		public AudioMixerGroup MusicAudioMixerGroup;
		public AudioMixerGroup SoundsAudioMixerGroup;
		public AudioMixerSnapshot DefaultAudioSnapshot;
		public AudioMixerSnapshot PauseAudioSnapshot;
		[Range(0f, 1f)] public float MusicVolume = 1f;
		public AudioClip Music1Clip;
		[Range(0f, 1f)] public float SoundVolume = 1f;
		public AudioClip MenuTextAppearClip;
		public AudioClip MenuConfirmClip;
		public AudioClip PlayerDeathClip;
	}

	[Serializable]
	public class TileToEntity : SerializableDictionary<TileBase, Entity> { }
}
