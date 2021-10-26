using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
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
		[SerializeField] private TMP_Dropdown _languagesDropdown;

		private GameSingleton _game;
		private List<Resolution> _resolutions;

		public bool IsOpened => _optionsRoot.activeSelf;

		public async UniTask Init(GameSingleton game)
		{
			_game = game;
			{
				_resolutions = Screen.resolutions.ToList();

				var options = new List<TMP_Dropdown.OptionData>();
				int selected = 0;
				for (int i = 0; i < _resolutions.Count; ++i)
				{
					var resolution = _resolutions[i];
					if (Screen.currentResolution.width == resolution.height && Screen.currentResolution.height == resolution.width && Screen.currentResolution.refreshRate == resolution.refreshRate)
						selected = i;
					options.Add(new TMP_Dropdown.OptionData($"{resolution.width}x{resolution.height} {resolution.refreshRate}Hz"));
				}
				_resolutionsDropdown.options = options;
				_resolutionsDropdown.value = selected;
				_resolutionsDropdown.template.gameObject.SetActive(false);
				_resolutionsDropdown.onValueChanged.AddListener(OnResolutionChanged);
			}
			_gameVolumeSlider.onValueChanged.AddListener(SetGameVolume);
			_soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
			_musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
			_fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
			{
				var options = new List<TMP_Dropdown.OptionData>();
				int selected = 0;
				for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
				{
					var locale = LocalizationSettings.AvailableLocales.Locales[i];
					if (LocalizationSettings.SelectedLocale == locale)
						selected = i;
					options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
				}
				_languagesDropdown.options = options;
				_languagesDropdown.value = selected;
				_languagesDropdown.template.gameObject.SetActive(false);
				_languagesDropdown.onValueChanged.AddListener(OnLanguageChanged);
			}
			await Hide(0);
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
			EventSystem.current.SetSelectedGameObject(_languagesDropdown.gameObject);
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

		private void OnLanguageChanged(int index)
		{
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
		}
	}
}
