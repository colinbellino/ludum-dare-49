using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Core
{
	// FIXME: Make every timing in here use  Time.timeScale
	public class GameUI : MonoBehaviour
	{
		[Header("Debug")]
		[SerializeField] private GameObject _debugRoot;
		[SerializeField] private TMP_Text _debugText;
		[Header("Gameplay")]
		[SerializeField] private GameObject _gameplayRoot;
		[SerializeField] private Animator _angerMeterAnimator;
		[SerializeField] private RectTransform _angerMeterCache;
		[Header("Title")]
		[SerializeField] private GameObject _titleRoot;
		[SerializeField] private RectTransform _titleWrapper;
		[SerializeField] public Button StartButton;
		[SerializeField] public Button OptionsButton;
		[SerializeField] public Button QuitButton;
		[Header("Level Selection")]
		[SerializeField] public GameObject _levelSelectionRoot;
		[SerializeField] public Button[] LevelButtons;
		[Header("Transitions")]
		[SerializeField] private GameObject _fadeRoot;
		[SerializeField] private Image _fadeToBlackImage;
		[Header("Level Name")]
		[SerializeField] private GameObject _levelNameRoot;
		[SerializeField] public TMP_Text _levelNameText;

		private GameConfig _config;
		private GameState _state;
		private TweenerCore<Color, Color, ColorOptions> _fadeTweener;

		public void Inject(GameSingleton game)
		{
			_config = game.Config;
			_state = game.State;
		}

		private async void Start()
		{
			HideDebug();
			HideGameplay();
			await HideTitle(0);
			await HideLevelSelection(0);

			StartButton.onClick.AddListener(PlayButtonClip);
			QuitButton.onClick.AddListener(PlayButtonClip);
			foreach (var button in LevelButtons)
			{
				button.onClick.AddListener(PlayButtonClip);
			}
		}

		private void PlayButtonClip()
		{
			FMODUnity.RuntimeManager.PlayOneShot(_config.SoundMenuConfirm);
		}

		public void ShowDebug() { _debugRoot.SetActive(true); }
		public void HideDebug() { _debugRoot.SetActive(false); }
		public void SetDebugText(string value)
		{
			_debugText.text = value;
		}
		public void AddDebugLine(string value)
		{
			_debugText.text += value + "\n";
		}

		public void ShowGameplay() { _gameplayRoot.SetActive(true); }
		public void HideGameplay() { _gameplayRoot.SetActive(false); }

		public void SetAngerMeter(int value, AngerStates angerState)
		{

			if (_angerMeterAnimator.isActiveAndEnabled)
			{
				_angerMeterAnimator.SetFloat("AngerState", (angerState == AngerStates.Calm) ? 0 : 1);
			}
			var cacheSize = _angerMeterCache.sizeDelta;

			switch (value)
			{
				case 2: { cacheSize.x = 19; } break;
				case 1: { cacheSize.x = 10; } break;
				case 0: { cacheSize.x = 0; } break;
				default: break;
			}

			_angerMeterCache.sizeDelta = cacheSize;
		}

		public async UniTask ShowTitle(CancellationToken cancellationToken, float duration = 0.5f)
		{
			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(StartButton.gameObject);

			_titleRoot.SetActive(true);
			await _titleWrapper.DOLocalMoveY(0, duration / Time.timeScale).WithCancellation(cancellationToken);
		}
		public async UniTask HideTitle(float duration = 0.5f)
		{
			await _titleWrapper.DOLocalMoveY(128, duration / Time.timeScale);
			_titleRoot.SetActive(false);
		}

		public async UniTask ShowLevelName(string title, float duration = 0.5f)
		{
			_levelNameRoot.SetActive(true);
			_levelNameText.text = title;
			await _levelNameText.rectTransform.DOLocalMoveY(-80, duration / Time.timeScale);
		}
		public async UniTask HideLevelName(float duration = 0.25f)
		{
			await _levelNameText.rectTransform.DOLocalMoveY(-130, duration / Time.timeScale);
			_levelNameRoot.SetActive(false);
		}

		public async UniTask ShowLevelSelection(float duration = 0.5f)
		{
			_levelSelectionRoot.SetActive(true);

			for (int i = 0; i < LevelButtons.Length; i++)
			{
				var button = LevelButtons[i];
				if (i < _state.AllLevels.Length)
				{
					var image = button.GetComponentInChildren<RawImage>();
					var text = button.GetComponentInChildren<TMP_Text>();
					var level = _state.AllLevels[i];

					text.text = $"Level {Utils.GetLevelIndex(i)}";
					if (_config.Levels.Contains(level) == false)
					{
						text.text = level.name;
					}
					image.texture = level.Screenshot;
				}
				else
				{
					button.gameObject.SetActive(false);
				}
			}
		}
		public async UniTask HideLevelSelection(float duration = 0.5f)
		{
			_levelSelectionRoot.SetActive(false);
		}
		public UniTask ToggleLevelSelection(float duration = 0.5f)
		{
			if (_levelSelectionRoot.activeSelf)
			{
				return HideLevelSelection(duration);
			}
			return ShowLevelSelection(duration);
		}

		public async UniTask FadeIn(Color color, float duration = 1f)
		{
			_fadeRoot.SetActive(true);
			if (_fadeTweener != null)
			{
				_fadeTweener.Rewind(false);
			}
			_fadeTweener = _fadeToBlackImage.DOColor(color, duration / Time.timeScale);
			await _fadeTweener;
		}

		public async UniTask FadeOut(float duration = 1f)
		{
			if (_fadeTweener != null)
			{
				_fadeTweener.Rewind(false);
			}
			_fadeTweener = _fadeToBlackImage.DOColor(Color.clear, duration / Time.timeScale);
			await _fadeTweener;
			_fadeRoot.SetActive(false);
		}

		private async UniTask FadeInPanel(Image panel, TMP_Text text, float duration)
		{
			panel.gameObject.SetActive(true);

			foreach (var t in panel.GetComponentsInChildren<TMP_Text>())
			{
				_ = t.DOFade(1f, 0f);
			}

			_ = panel.DOFade(1f, duration);

			text.maxVisibleCharacters = 0;

			await UniTask.Delay(TimeSpan.FromSeconds(duration));

			var totalInvisibleCharacters = text.textInfo.characterCount;
			var counter = 0;
			while (true)
			{
				var visibleCount = counter % (totalInvisibleCharacters + 1);
				text.maxVisibleCharacters = visibleCount;

				if (visibleCount >= totalInvisibleCharacters)
				{
					break;
				}

				counter += 1;

				await UniTask.Delay(10);
			}

			var buttons = panel.GetComponentsInChildren<Button>();
			for (int i = 0; i < buttons.Length; i++)
			{
				_ = buttons[i].image.DOFade(1f, duration);
			}
		}

		private async UniTask FadeOutPanel(Image panel, float duration)
		{
			_ = panel.DOFade(0f, duration);

			foreach (var graphic in panel.GetComponentsInChildren<Graphic>())
			{
				_ = graphic.DOFade(0f, duration);
			}

			await UniTask.Delay(TimeSpan.FromSeconds(duration));
			panel.gameObject.SetActive(false);
		}
	}
}
