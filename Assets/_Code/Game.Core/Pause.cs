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
		[SerializeField] public Button SoundButton;
		[SerializeField] public Button MusicButton;
		[SerializeField] public Button FullscreenButton;
		[SerializeField] public TMP_Dropdown ResolutionsDropdown;
		[SerializeField] public Button QuitButton;

		private List<Resolution> _resolutions;

		public bool IsOpened => _pauseRoot.activeSelf;

		private void Start()
		{
			Hide();

			_resolutions = Screen.resolutions/* .Where(r => r.refreshRate == 60) */.ToList();
			ResolutionsDropdown.options = _resolutions.Select(r => new TMP_Dropdown.OptionData($"{r.width}x{r.height} {r.refreshRate}Hz")).ToList();
			ResolutionsDropdown.template.gameObject.SetActive(false);
			ResolutionsDropdown.onValueChanged.AddListener(OnResolutionChanged);
		}

		private void OnResolutionChanged(int index)
		{
			var resolution = _resolutions[index];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRate);
		}

		public async void Show(bool showQuitButton = true)
		{
			_pauseRoot.SetActive(true);
			QuitButton.gameObject.SetActive(showQuitButton);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(SoundButton.gameObject);
		}

		public void Hide()
		{
			_pauseRoot.SetActive(false);
		}
	}
}
