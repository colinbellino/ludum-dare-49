using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class Pause : MonoBehaviour
	{
		[SerializeField] private GameObject _pauseRoot;
		[SerializeField] private TMP_Text _titleText;
		[SerializeField] private Slider _gameSlider;
		[SerializeField] private Slider _soundSlider;
		[SerializeField] private Slider _musicSlider;
		[SerializeField] private Toggle _fullscreenToggle;
		[SerializeField] private TMP_Dropdown _resolutionsDropdown;
		[SerializeField] private Button _quitButton;

		private List<Resolution> _resolutions;
		private GameSingleton _game;

		public bool IsOpened => _pauseRoot.activeSelf;

		public async UniTask Init(GameSingleton game)
		{
			_game = game;

			await Hide();

			_resolutions = Screen.resolutions/* .Where(r => r.refreshRate == 60) */.ToList();
			_resolutionsDropdown.options = _resolutions.Select(r => new TMP_Dropdown.OptionData($"{r.width}x{r.height} {r.refreshRate}Hz")).ToList();
			_resolutionsDropdown.template.gameObject.SetActive(false);
			_resolutionsDropdown.onValueChanged.AddListener(OnResolutionChanged);
			_gameSlider.onValueChanged.AddListener(SetGameVolume);
			_soundSlider.onValueChanged.AddListener(SetSoundVolume);
			_musicSlider.onValueChanged.AddListener(SetMusicVolume);
			_fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
			_quitButton.onClick.AddListener(QuitGame);
		}

		public async UniTask Show(string title, bool showQuitButton = true)
		{
			_titleText.text = title;
			_pauseRoot.SetActive(true);
			_gameSlider.value = _game.State.PlayerSettings.GameVolume;
			_soundSlider.value = _game.State.PlayerSettings.SoundVolume;
			_musicSlider.value = _game.State.PlayerSettings.MusicVolume;
			_fullscreenToggle.isOn = _game.State.PlayerSettings.FullScreen;
			_quitButton.gameObject.SetActive(showQuitButton);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(_gameSlider.gameObject);
		}

		public UniTask Hide()
		{
			_pauseRoot.SetActive(false);
			return default;
		}

		private void SetGameVolume(float value)
		{
			_game.State.GameBus.setVolume(value);
			_game.State.PlayerSettings.GameVolume = value;
		}

		private void SetSoundVolume(float value)
		{
			_game.State.SoundBus.setVolume(value);
			_game.State.PlayerSettings.SoundVolume = value;
		}

		private void SetMusicVolume(float value)
		{
			_game.State.MusicBus.setVolume(value);
			_game.State.PlayerSettings.MusicVolume = value;
		}

		private void ToggleFullscreen(bool value)
		{
			Screen.fullScreen = value;
			_game.State.PlayerSettings.FullScreen = value;
		}

		private void OnResolutionChanged(int index)
		{
			var resolution = _resolutions[index];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
			_game.State.PlayerSettings.ResolutionWidth = resolution.width;
			_game.State.PlayerSettings.ResolutionHeight = resolution.height;
			_game.State.PlayerSettings.ResolutionRefreshRate = resolution.refreshRate;
		}

		private void QuitGame() => _game.GameFSM.Fire(StateMachines.Game.GameFSM.Triggers.Quit);
	}
}
