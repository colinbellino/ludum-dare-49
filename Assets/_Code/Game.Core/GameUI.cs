using System;
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
		[Header("Pause")]
		[SerializeField] private GameObject _pauseRoot;
		[SerializeField] public Button PauseButton1;
		[SerializeField] public Button PauseButton2;
		[SerializeField] public Button PauseButton3;
		[SerializeField] public Button PauseButton4;
		[Header("Title")]
		[SerializeField] private GameObject _titleRoot;
		[SerializeField] private RectTransform _titleName;
		[SerializeField] private RectTransform _titleMenu;
		[SerializeField] private RectTransform _titleLinks;
		[SerializeField] public Button TitleButton1;
		[SerializeField] public Button TitleButton2;
		[Header("Level Selection")]
		[SerializeField] public GameObject _levelSelectionRoot;
		[SerializeField] public Button[] LevelButtons;
		[Header("Transitions")]
		[SerializeField] private Image _fadeToBlackImage;
		[SerializeField] public TMP_Text FadeText;

		private AudioPlayer _audioPlayer;
		private GameConfig _config;
		private TweenerCore<Color, Color, ColorOptions> _fadeTweener;

		public void Inject(GameSingleton game)
		{
			_audioPlayer = game.AudioPlayer;
			_config = game.Config;
		}

		private async void Start()
		{
			HideDebug();
			HideGameplay();
			HidePause();
			await HideTitle(0);
			await HideLevelSelection(0);

			PauseButton1.onClick.AddListener(PlayButtonClip);
			PauseButton2.onClick.AddListener(PlayButtonClip);
			PauseButton3.onClick.AddListener(PlayButtonClip);
			PauseButton4.onClick.AddListener(PlayButtonClip);
			TitleButton1.onClick.AddListener(PlayButtonClip);
			TitleButton2.onClick.AddListener(PlayButtonClip);
			foreach (var button in LevelButtons)
			{
				button.onClick.AddListener(PlayButtonClip);
			}
		}

		private void PlayButtonClip()
		{
			if (_config.MenuConfirmClip)
			{
				_audioPlayer.PlaySoundEffect(_config.MenuConfirmClip);
			}
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

		public async void ShowPause()
		{
			_pauseRoot.SetActive(true);

			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(PauseButton2.gameObject);
		}
		public void HidePause() { _pauseRoot.SetActive(false); }

		public async UniTask ShowTitle(CancellationToken cancellationToken, float duration = 0.5f)
		{
			EventSystem.current.SetSelectedGameObject(null);
			await UniTask.NextFrame();
			EventSystem.current.SetSelectedGameObject(TitleButton1.gameObject);

			_titleRoot.SetActive(true);
			await _titleName.DOLocalMoveY(20, duration / Time.timeScale).WithCancellation(cancellationToken);
			await _titleLinks.DOLocalMoveY(-330, duration / Time.timeScale).WithCancellation(cancellationToken);
		}
		public async UniTask HideTitle(float duration = 0.5f)
		{
			await _titleName.DOLocalMoveY(128, duration / Time.timeScale);
			await _titleLinks.DOLocalMoveY(-330, duration / Time.timeScale);
			_titleRoot.SetActive(false);
		}

		public async UniTask ShowLevelTitle(string title, float duration = 0.5f)
		{
			FadeText.text = title;
			await FadeText.rectTransform.DOLocalMoveY(-87, duration / Time.timeScale);
		}
		public async UniTask HideLevelTitle(float duration = 0.25f)
		{
			await FadeText.rectTransform.DOLocalMoveY(-120, duration / Time.timeScale);
		}

		public async UniTask ShowLevelSelection(float duration = 0.5f)
		{
			_levelSelectionRoot.SetActive(true);
			for (int i = 0; i < LevelButtons.Length; i++)
			{
				var button = LevelButtons[i];
				if (i < _config.AllLevels.Length)
				{
					var image = button.GetComponentInChildren<RawImage>();
					var text = button.GetComponentInChildren<TMP_Text>();
					var level = _config.AllLevels[i];

					text.text = $"Level {i + 1:D2}";
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

			if (_config.MenuTextAppearClip)
			{
				_ = _audioPlayer.PlaySoundEffect(_config.MenuTextAppearClip);
			}

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
