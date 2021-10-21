using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	public class PauseUI : MonoBehaviour
	{
		[SerializeField] private GameObject _pauseRoot;
		[SerializeField] private Button _levelSelectButton;
		[SerializeField] private Button _optionsButton;
		[SerializeField] private Button _quitButton;

		private GameSingleton _game;

		public bool IsOpened => _pauseRoot.activeSelf;

		public async UniTask Init(GameSingleton game)
		{
			_game = game;

			_levelSelectButton.onClick.AddListener(OpenLevelSelect);
			_optionsButton.onClick.AddListener(OpenOptions);
			_quitButton.onClick.AddListener(QuitGame);

			await Hide();
		}

		public async UniTask Show(string title, bool showQuitButton = true)
		{
			_pauseRoot.SetActive(true);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(_levelSelectButton.gameObject);
		}

		public UniTask Hide()
		{
			_pauseRoot.SetActive(false);
			return default;
		}

		public void SelectOptionsGameObject()
		{
			EventSystem.current.SetSelectedGameObject(_optionsButton.gameObject);
		}

		private void OpenLevelSelect() => _game.GameFSM.Fire(StateMachines.Game.GameFSM.Triggers.LevelSelectionRequested);

		private void OpenOptions() => _ = _game.OptionsUI.Show();

		private void QuitGame() => _game.GameFSM.Fire(StateMachines.Game.GameFSM.Triggers.Quit);
	}
}
