using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Core
{
	public class AudioPlayer
	{
		private readonly GameConfig _config;
		private readonly AudioSource _musicSource;
		private readonly AudioSource _musicSource2;

		public readonly Dictionary<int, float> MusicTimes = new Dictionary<int, float>();

		public AudioPlayer(GameConfig config, AudioSource musicSource, AudioSource musicSource2)
		{
			_config = config;
			_musicSource = musicSource;
			_musicSource2 = musicSource2;
		}

		public void Tick()
		{
			if (_musicSource.clip)
			{
				MusicTimes[_musicSource.clip.GetInstanceID()] = _musicSource.time;
			}
			if (_musicSource2.clip)
			{
				MusicTimes[_musicSource2.clip.GetInstanceID()] = _musicSource2.time;
			}
		}

		public UniTask PlayRandomSoundEffect(AudioClip[] clips, Vector3 position, float volume = 1f)
		{
			var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
			return PlaySoundEffect(clip, position, volume);
		}

		public UniTask PlaySoundEffect(AudioClip clip, float volume = 1f)
		{
			// Default to the center of the screen
			return PlaySoundEffect(clip, Vector3.zero, volume);
		}

		public UniTask PlaySoundEffect(AudioClip clip, Vector3 position, float volume = 1f)
		{
			return PlaySoundClipAtPoint(clip, position, volume);
		}

		public async UniTask PlayMusic(AudioClip clip, bool fromStart = true, float fadeDuration = 0f, float volume = 1f, bool crossFade = false)
		{
			UnityEngine.Debug.Log("PlayMusic " + clip.name);
			if (fadeDuration > 0f)
			{
				if (crossFade)
				{
					// init temp source
					_musicSource2.volume = 0;
					_musicSource2.clip = clip;
					var time = _musicSource.time;
					if (time > clip.length)
					{
						time = 0;
					}
					_musicSource2.time = time;
					_musicSource2.Play();

					_ = _musicSource.DOFade(0, fadeDuration);
					_ = _musicSource2.DOFade(volume, fadeDuration);
					await UniTask.Delay(TimeSpan.FromSeconds(fadeDuration));

					_musicSource.volume = volume;
					_musicSource.clip = _musicSource2.clip;
					_musicSource.time = _musicSource2.time;
					_musicSource.Play();

					// stop temp source
					_musicSource2.clip = null;
					_musicSource2.Stop();
				}
				else
				{
					if (fromStart)
					{
						_musicSource.time = 0f;
					}
					else
					{
						MusicTimes.TryGetValue(clip.GetInstanceID(), out var time);
						_musicSource.time = time;
					}

					_musicSource.clip = clip;
					_musicSource.Play();

					await _musicSource.DOFade(volume, fadeDuration);
				}
			}
			else
			{
				_musicSource.volume = volume;
			}
		}

		public void PauseMusic()
		{
			_musicSource.Pause();
		}

		public void ResumeMusic()
		{
			_musicSource.UnPause();
		}

		public async UniTask StopMusic(float fadeDuration = 0.5f)
		{
			await _musicSource.DOFade(0f, fadeDuration);
			_musicSource.Stop();
		}

		public void SetMusicVolume(float volume)
		{
			_musicSource.volume = volume;
			_musicSource2.volume = volume;
			// _config.AudioMixer.SetFloat("MusicVolume", ConvertToMixerVolume(volume));
		}

		public void SetSoundVolume(float volume)
		{
			// _config.AudioMixer.SetFloat("SoundVolume", ConvertToMixerVolume(volume));
		}

		public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float duration = 0f)
		{
			_config.AudioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] { snapshot }, new float[1] { 1 }, duration);
		}

		public bool IsMusicPlaying() => _musicSource.isPlaying;

		public bool IsCurrentMusic(AudioClip clip) => _musicSource.clip == clip;

		/**
			Convert to the volume range the unity audio mixer is expecting.
			<br />0   => -80
			<br />0.5 => -40
			<br />1   => 0
		*/
		private static float ConvertToMixerVolume(float volume)
		{
			return (volume - 1f) * 80f;
		}

		// TODO: Use polling instead of creating game object each time
		private UniTask PlaySoundClipAtPoint(AudioClip clip, Vector3 position, float volume)
		{
			var gameObject = new GameObject("One shot audio");
			gameObject.transform.position = position;
			var audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = _config.SoundsAudioMixerGroup;
			audioSource.volume = volume;
			audioSource.Play();
			UnityEngine.Object.Destroy(gameObject, clip.length * ((double)Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale));
			return UniTask.Delay(TimeSpan.FromSeconds(clip.length));
		}
	}
}
