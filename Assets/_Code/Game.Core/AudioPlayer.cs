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
		private readonly AudioMixer _mixer;
		private readonly AudioSource _musicTitleSource;
		private readonly AudioSource _musicCalmSource;
		private readonly AudioSource _musicAngrySource;
		private readonly AudioMixerGroup _musicMixerGroup;
		private readonly AudioMixerGroup _soundMixerGroup;
		private readonly GameConfig _config;
		private readonly List<AudioSource> _soundSources;
		private readonly GameObject _soundParent;
		public readonly Dictionary<int, float> MusicTimes = new Dictionary<int, float>();

		public AudioPlayer(
			AudioSource musicTitleSource, AudioSource musicCalmSource, AudioSource musicAngrySource,
			GameConfig config, int soundPoolSize = 30)
		{
			_config = config;
			_musicTitleSource = musicTitleSource;
			_musicCalmSource = musicCalmSource;
			_musicAngrySource = musicAngrySource;

			_soundParent = new GameObject("Sound Effects");
			_soundSources = new List<AudioSource>(soundPoolSize);
			for (int i = 0; i < soundPoolSize; i++)
			{
				var gameObject = new GameObject("Sound Effect Audio Source " + i);
				gameObject.transform.parent = _soundParent.transform;
				gameObject.SetActive(false);
				var audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.outputAudioMixerGroup = _config.SoundsAudioMixerGroup;

				_soundSources.Add(audioSource);
			}
		}

		public void Tick()
		{
			_musicTitleSource.pitch = Time.timeScale;
			_musicCalmSource.pitch = Time.timeScale;

			if (_musicTitleSource.clip)
			{
				MusicTimes[_musicTitleSource.clip.GetInstanceID()] = _musicTitleSource.time;
			}
			if (_musicCalmSource.clip)
			{
				MusicTimes[_musicCalmSource.clip.GetInstanceID()] = _musicCalmSource.time;
			}
		}

		public UniTask PlayRandomSoundEffect(AudioClip[] clips, Vector3 position)
		{
			var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
			return PlaySoundEffect(clip, position);
		}

		public UniTask PlaySoundEffect(AudioClip clip)
		{
			// Default to the center of the screen
			return PlaySoundEffect(clip, Vector3.zero);
		}

		public UniTask PlaySoundEffect(AudioClip clip, Vector3 position)
		{
			return PlaySoundClipAtPoint(clip, position);
		}

		public async UniTask PlayTitleMusic(AudioClip clip, float duration = 3f)
		{
			_musicTitleSource.volume = 0f;
			_musicTitleSource.time = 0f;
			_musicTitleSource.clip = clip;
			_musicTitleSource.Play();
			await _musicTitleSource.DOFade(1, duration);
		}

		public async UniTask StopTitleMusic(float duration = 3f)
		{
			await _musicTitleSource.DOFade(0, duration);
		}

		public async UniTask PlayMusic(AudioClip clip, float volume, bool fromStart = true, float fadeDuration = 0f, bool crossFade = false)
		{
			if (fadeDuration > 0f)
			{
				if (crossFade)
				{
					// init temp source
					_musicCalmSource.volume = 0;
					_musicCalmSource.clip = clip;
					var time = _musicTitleSource.time;
					if (time > clip.length)
					{
						time = 0;
					}
					_musicCalmSource.time = time;
					_musicCalmSource.Play();

					_ = _musicTitleSource.DOFade(0, fadeDuration);
					_ = _musicCalmSource.DOFade(volume, fadeDuration);
					await UniTask.Delay(TimeSpan.FromSeconds(fadeDuration));

					_musicTitleSource.volume = volume;
					_musicTitleSource.clip = _musicCalmSource.clip;
					_musicTitleSource.time = _musicCalmSource.time;
					_musicTitleSource.Play();

					// stop temp source
					_musicCalmSource.clip = null;
					_musicCalmSource.Stop();
				}
				else
				{
					if (fromStart)
					{
						_musicTitleSource.time = 0f;
					}
					else
					{
						MusicTimes.TryGetValue(clip.GetInstanceID(), out var time);
						_musicTitleSource.time = time;
					}

					_musicTitleSource.clip = clip;
					_musicTitleSource.Play();

					await _musicTitleSource.DOFade(volume, fadeDuration);
				}
			}
			else
			{
				_musicTitleSource.volume = volume;
			}
		}

		// public void PauseMusic()
		// {
		// 	_musicTitleSource.Pause();
		// }

		// public void ResumeMusic()
		// {
		// 	_musicTitleSource.UnPause();
		// }

		// public async UniTask StopMusic(float fadeDuration = 0.5f)
		// {
		// 	await _musicTitleSource.DOFade(0f, fadeDuration);
		// 	_musicTitleSource.Stop();
		// }

		// public void SetSoundVolume(float volume)
		// {
		// 	// FIXME:
		// 	// _config.AudioMixer.SetFloat("SoundVolume", ConvertToMixerVolume(volume));
		// }

		// public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float duration = 0f)
		// {
		// 	_config.AudioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] { snapshot }, new float[1] { 1 }, duration);
		// }

		// public bool IsMusicPlaying() => _musicTitleSource.isPlaying;

		public bool IsCurrentMusic(AudioClip clip) => _musicTitleSource.clip == clip;

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

		private async UniTask PlaySoundClipAtPoint(AudioClip clip, Vector3 position)
		{
			var audioSource = GetSoundEffectSource();

			audioSource.gameObject.SetActive(true);
			audioSource.gameObject.transform.position = position;
			audioSource.clip = clip;
			// audioSource.volume = volume;
			audioSource.pitch = Time.timeScale;
			audioSource.Play();
			// UnityEngine.Object.Destroy(gameObject, clip.length * ((double)Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale));
			await UniTask.Delay(TimeSpan.FromSeconds(clip.length));

			audioSource.gameObject.SetActive(false);
		}

		private AudioSource GetSoundEffectSource()
		{
			foreach (var audioSource in _soundSources)
			{
				if (audioSource.gameObject.activeSelf == false)
				{
					return audioSource;
				}
			}

			UnityEngine.Debug.LogWarning("Sound effect pool too small");
			return null;
		}
	}
}
