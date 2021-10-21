using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class OptionsUI : MonoBehaviour
	{
		[SerializeField] private GameObject _optionsRoot;
		[SerializeField] private Slider _gameVolumeSlider;
		[SerializeField] private Slider _soundVolumeSlider;
		[SerializeField] private Slider _musicVolumeSlider;
		[SerializeField] private Toggle _fullscreenToggle;
		[SerializeField] private TMP_Dropdown _resolutionsDropdown;

		private GameSingleton _game;
		private List<Resolution> _resolutions;

		public bool IsOpened => _optionsRoot.activeSelf;

		public async UniTask Init(GameSingleton game)
		{
			_game = game;

			await Hide(0);

			_resolutions = Screen.resolutions/* .Where(r => r.refreshRate == 60) */.ToList();
			_resolutionsDropdown.options = _resolutions.Select(r => new TMP_Dropdown.OptionData($"{r.width}x{r.height} {r.refreshRate}Hz")).ToList();
			_resolutionsDropdown.template.gameObject.SetActive(false);
			_resolutionsDropdown.onValueChanged.AddListener(OnResolutionChanged);
			_gameVolumeSlider.onValueChanged.AddListener(SetGameVolume);
			_soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
			_musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
			_fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
		}

		public async UniTask Show(float duration = 0.5f)
		{
			_optionsRoot.SetActive(true);

			_gameVolumeSlider.value = _game.State.PlayerSettings.GameVolume;
			_soundVolumeSlider.value = _game.State.PlayerSettings.SoundVolume;
			_musicVolumeSlider.value = _game.State.PlayerSettings.MusicVolume;
			_fullscreenToggle.isOn = _game.State.PlayerSettings.FullScreen;

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(_gameVolumeSlider.gameObject);
		}

		public UniTask Hide(float duration = 0.5f)
		{
			_optionsRoot.SetActive(false);
			return default;
		}

		private void SetGameVolume(float value)
		{
			_game.State.PlayerSettings.GameVolume = value;
			_game.State.GameBus.setVolume(value);
		}

		private void SetSoundVolume(float value)
		{
			_game.State.PlayerSettings.SoundVolume = value;
			_game.State.SoundBus.setVolume(value);
		}

		private void SetMusicVolume(float value)
		{
			_game.State.PlayerSettings.MusicVolume = value;
			_game.State.MusicBus.setVolume(value);
		}

		private void ToggleFullscreen(bool value)
		{
			_game.State.PlayerSettings.FullScreen = value;
			Screen.fullScreen = value;
		}

		private void OnResolutionChanged(int index)
		{
			var resolution = _resolutions[index];
			_game.State.PlayerSettings.ResolutionWidth = resolution.width;
			_game.State.PlayerSettings.ResolutionHeight = resolution.height;
			_game.State.PlayerSettings.ResolutionRefreshRate = resolution.refreshRate;
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
		}
	}
}
