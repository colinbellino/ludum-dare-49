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

		public async UniTask Show(bool showQuitButton = true)
		{
			_pauseRoot.SetActive(true);
			_fullscreenToggle.isOn = Screen.fullScreen;
			float volume;
			_game.State.GameBus.getVolume(out volume);
			_gameSlider.value = volume;
			_game.State.SoundBus.getVolume(out volume);
			_soundSlider.value = volume;
			_game.State.MusicBus.getVolume(out volume);
			_musicSlider.value = volume;
			_fullscreenToggle.isOn = Screen.fullScreen;
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

		private void OnResolutionChanged(int index)
		{
			var resolution = _resolutions[index];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
		}

		private void SetGameVolume(float value) => _game.State.GameBus.setVolume(value);

		private void SetSoundVolume(float value) => _game.State.SoundBus.setVolume(value);

		private void SetMusicVolume(float value) => _game.State.MusicBus.setVolume(value);

		private void ToggleFullscreen(bool value) => Screen.fullScreen = value;

		private void QuitGame() => _game.GameFSM.Fire(StateMachines.Game.GameFSM.Triggers.Quit);
	}
}
