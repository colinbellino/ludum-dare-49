using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : BaseGameState
	{
		private bool _confirmWasPressedThisFrame;
		private bool _cancelWasPressedThisFrame;

		public GameGameplayState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			var playerPrefab = Resources.Load<Entity>("Player");
			_state.Player = GameObject.Instantiate(playerPrefab, GameObject.Find("Player Spawn").transform.position, Quaternion.identity);

			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)UnityEngine.Random.Range(0, int.MaxValue));

			if (_config.Music1Clip && _audioPlayer.IsMusicPlaying() == false && _audioPlayer.IsCurrentMusic(_config.Music1Clip) == false)
			{
				_ = _audioPlayer.PlayMusic(_config.Music1Clip, true, 0.5f);
			}

			await _ui.FadeOut();

			_state.Running = true;

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;
			_controls.Gameplay.Cancel.started += CancelStarted;
			_controls.Global.Enable();
		}

		public override void Tick()
		{
			base.Tick();

			if (_controls.Global.Pause.WasPerformedThisFrame())
			{
				if (Time.timeScale == 0f)
				{
					Time.timeScale = 1f;
					_state.Running = true;
					_audioPlayer.ResumeMusic();
					_ui.HidePause();

					Time.timeScale = _state.AssistMode ? 0.7f : 1f;
				}
				else
				{
					Time.timeScale = 0f;
					_state.Running = false;
					_audioPlayer.PauseMusic();
					_ui.ShowPause();
				}
			}

			if (_state.Running == false)
			{
				return;
			}

			if (_state.Player != null)
			{
				HandleInput(_state.Player);
			}

			_confirmWasPressedThisFrame = false;
			_cancelWasPressedThisFrame = false;

			if (Utils.IsDevBuild())
			{
				if (Keyboard.current.f1Key.wasPressedThisFrame)
				{
					Victory();
				}

				if (Keyboard.current.f2Key.wasPressedThisFrame)
				{
					Defeat();
				}
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_state.Running = false;

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.started -= ConfirmStarted;
			_controls.Gameplay.Cancel.started -= CancelStarted;
			_controls.Global.Disable();

			await _ui.FadeIn(Color.white);

			_ui.HideGameplay();

			GameObject.Destroy(_state.Player.gameObject);
		}

		private void ConfirmStarted(InputAction.CallbackContext context) => _confirmWasPressedThisFrame = true;

		private void CancelStarted(InputAction.CallbackContext context) => _cancelWasPressedThisFrame = true;

		private void Victory()
		{
			_ = _audioPlayer.StopMusic();
			_fsm.Fire(GameFSM.Triggers.Won);
		}

		private async void Defeat()
		{
			var position = _state.Player.transform.position;

			if (_config.PlayerDeathClip)
			{
				_ = _audioPlayer.PlaySoundEffect(_config.PlayerDeathClip);
			}

			_state.Running = false;

			var playerDeathPrefab = Resources.Load("Player Death");
			if (playerDeathPrefab)
			{
				GameObject.Instantiate(playerDeathPrefab, position, Quaternion.identity);
			}

			await UniTask.Delay(1000);

			_state.Player.transform.position = GameObject.Find("Player Spawn").transform.position;
			_state.Running = true;

			_fsm.Fire(GameFSM.Triggers.Lost);
		}

		private void HandleInput(Entity entity)
		{
			var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
		}
	}
}
