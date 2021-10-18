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
		[SerializeField] public Button PauseButton1;
		[SerializeField] public Button PauseButton2;
		[SerializeField] public Button PauseButton3;
		[SerializeField] public TMP_Dropdown _resolutionsDropdown;

		private GameConfig _config;
		private List<Resolution> _resolutions;

		public void Inject(GameSingleton game)
		{
			_config = game.Config;
		}

		private void Start()
		{
			Hide();

			_resolutions = Screen.resolutions/* .Where(r => r.refreshRate == 60) */.ToList();
			_resolutionsDropdown.options = _resolutions.Select(r => new TMP_Dropdown.OptionData($"{r.width}x{r.height} {r.refreshRate}Hz")).ToList();
			_resolutionsDropdown.template.gameObject.SetActive(false);
			_resolutionsDropdown.onValueChanged.AddListener(OnResolutionChanged);
		}

		private void OnResolutionChanged(int index)
		{
			var resolution = _resolutions[index];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
		}

		public async void Show()
		{
			_pauseRoot.SetActive(true);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(PauseButton1.gameObject);
		}

		public void Hide()
		{
			_pauseRoot.SetActive(false);
		}
	}
}
