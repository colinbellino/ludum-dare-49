using Cysharp.Threading.Tasks;
using DG.Tweening;
using NesScripts.Controls.PathFind;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : BaseGameState
	{
		private bool _confirmWasPressedThisFrame;
		private bool _cancelWasPressedThisFrame;
		private bool _resetWasPressedThisFrame;
		private bool _running;

		private static Vector3 cellOffset = new Vector3(0.5f, 0.5f);
		private float _noTransitionTimestamp;
		private Entity _player => _state.Entities.Find((entity) => entity.ControlledByPlayer);

		public GameGameplayState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.SetDebugText(@"[DEBUG]
- F1: trigger victory
- F2: trigger defeat
- ESCAPE: pause/options");

			_noTransitionTimestamp = Time.time + 3f;

			if (_state.CurrentLevelIndex > _config.AllLevels.Length - 1)
			{
				await Victory();
				return;
			}

			// Initialize entities
			_state.KeysInLevel = _state.Entities.FindAll(e => e.TriggerAction == TriggerActions.Key).Count;
			foreach (var entity in _state.Entities)
			{
				entity.transform.position = entity.GridPosition + cellOffset;

				if (entity.CanBeActivated)
				{
					if (entity.ActivatesWhenLevelStart)
					{
						if (
								(entity.ActivatesInSpecificAngerState && entity.TriggerState == _player.AngerState) ||
								(entity.ActivatesInSpecificAngerState == false && entity.TriggerState == AngerStates.None)
							)
						{
							entity.Activated = true;
						}
					}

					if (entity.ActivatesWhenKeyInLevel)
					{
						if (_state.KeysInLevel == 0)
						{
							if (
								(entity.ActivatesInSpecificAngerState && entity.TriggerState == _player.AngerState) ||
								(entity.ActivatesInSpecificAngerState == false && entity.TriggerState == AngerStates.None)
							)
							{
								entity.Activated = true;
							}
						}
					}
				}
			}

			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)UnityEngine.Random.Range(0, int.MaxValue));

			// Start or continue music where we left off
			if (_audioPlayer.IsMusicPlaying() == false)
			{
				_ = _audioPlayer.PlayMusic(_config.MusicCalmClip, false, 1f);
			}
			else
			{
				ToggleMusic(_player);
			}

			_state.Running = true;

			_ui.SetAngerMeter(_player.AngerProgress, _player.AngerState);
			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;
			_controls.Gameplay.Cancel.started += CancelStarted;
			_controls.Gameplay.Reset.started += ResetStarted;
			_controls.Global.Enable();

			_ = _ui.FadeOut(3);

			_running = true;
			while (_running)
			{
				await UniTask.NextFrame();

				if (_state.Running == false)
				{
					continue;
				}

				// var moveControl = (ButtonControl)_controls.Gameplay.Move.activeControl;
				// if (moveControl == null || !moveControl.wasPressedThisFrame)
				// {
				// 	continue;
				// }

				var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
				if (moveInput.magnitude == 0)
				{
					continue;
				}

				if (_player.Dead)
				{
					continue;
				}

				var destination = _player.GridPosition + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);
				var playerDidMove = await Turn(_player, destination);
				if (playerDidMove)
				{
					foreach (var entity in _state.Entities)
					{
						if (entity.MoveTowardsPlayer)
						{
							var path = Pathfinding.FindPath(
								_state.WalkableGrid,
								entity.GridPosition, _player.GridPosition,
								Pathfinding.DistanceType.Manhattan
							);
							await Turn(entity, path[0]);
						}

						if (entity.Dead)
						{
							entity.Animator.Play("Dead");
						}
						else
						{
							if (_state.Running && entity.AffectedByAnger)
							{
								entity.AngerProgress += 1;

								if (entity.AngerProgress >= 3)
								{
									entity.AngerProgress = 0;
									entity.AngerState = (entity.AngerState == AngerStates.Calm) ? AngerStates.Angry : AngerStates.Calm;
									entity.Animator.SetFloat("AngerState", (entity.AngerState == AngerStates.Calm) ? 0 : 1);

									entity.Direction = Vector3Int.down;
									entity.Animator.SetFloat("DirectionX", entity.Direction.x);
									entity.Animator.SetFloat("DirectionY", entity.Direction.y);

									if (entity.TransformationAudioClip)
									{
										_ = _audioPlayer.PlaySoundEffect(entity.TransformationAudioClip);
									}
									entity.Animator.Play("Transform");
									await CurrentAnimation(entity);

									if (entity.ControlledByPlayer)
									{
										ToggleMusic(entity);
									}

									var otherEntityAtPosition = _state.Entities.Find(e => e.GridPosition == entity.GridPosition && e != entity);
									if (_state.Running && otherEntityAtPosition)
									{
										await CheckTrigger(entity, otherEntityAtPosition);
									}
								}

								_ui.SetAngerMeter(entity.AngerProgress, entity.AngerState);
							}
						}
					}

					foreach (var entity in _state.Entities)
					{
						if (entity.ActivatesInSpecificAngerState)
						{
							if (entity.Activated == false && _player.AngerState == entity.TriggerState)
							{
								if (
									(_state.KeysInLevel == 0) ||
									_state.KeysInLevel > 0 && _state.KeysPickedUp >= _state.KeysInLevel)
								{
									entity.Activated = true;
								}
							}
							else
							{
								if (_player.AngerState != entity.TriggerState)
								{
									entity.Activated = false;
								}
							}
						}
					}
				}
				else
				{
					if (_player.CantMoveAudioClip)
					{
						_ = _audioPlayer.PlaySoundEffect(_player.CantMoveAudioClip);
					}
				}
			}
		}

		private UniTask CurrentAnimation(Entity entity)
		{
			var infos = entity.Animator.GetCurrentAnimatorClipInfo(0);
			if (infos.Length == 0)
			{
				return default;
			}
			var clipName = entity.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
			if (entity.AnimationClipLength.ContainsKey(clipName) == false)
			{
				return default;
			}

			return UniTask.Delay(System.TimeSpan.FromSeconds(entity.AnimationClipLength[clipName]));
		}

		public override async void Tick()
		{
			base.Tick();

			if (_controls.Global.Pause.WasPerformedThisFrame())
			{
				if (Time.timeScale == 0f)
				{
					Time.timeScale = 1f;
					_state.Running = true;
					_audioPlayer.SetMusicVolume(_state.IsMusicPlaying ? 1 : 0);
					_ui.HidePause();

					Time.timeScale = _state.AssistMode ? 0.7f : 1f;
				}
				else
				{
					Time.timeScale = 0f;
					_state.Running = false;
					_audioPlayer.SetMusicVolume(_state.IsMusicPlaying ? 0.1f : 0);
					_ui.ShowPause();
				}
			}

			if (_state.Running)
			{
				foreach (var entity in _state.Entities)
				{
					if (entity.BreakableProgress > 0)
					{
						entity.Animator.Play("Damaged");
					}

					if (entity.CanBeActivated)
					{
						entity.Animator.SetBool("Active", entity.Activated);
					}
				}

				// Quick and dirty fix just to prevent spamming gamebreaking stuff
				if (Time.time >= _noTransitionTimestamp)
				{
					if (_resetWasPressedThisFrame)
					{
						if (_config.RestartClip)
						{
							_ = _audioPlayer.PlaySoundEffect(_config.RestartClip);
						}
						_fsm.Fire(GameFSM.Triggers.Retry);
					}

					if (Keyboard.current.f2Key.wasPressedThisFrame)
					{
						_ = NextLevel(true);
					}
				}

				if (Utils.IsDevBuild())
				{
					if (Keyboard.current.f1Key.wasPressedThisFrame)
					{
						await Victory();
					}
				}
			}

			_confirmWasPressedThisFrame = false;
			_cancelWasPressedThisFrame = false;
			_resetWasPressedThisFrame = false;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_running = false;

			_state.Running = false;

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.started -= ConfirmStarted;
			_controls.Gameplay.Cancel.started -= CancelStarted;
			_controls.Gameplay.Reset.started -= ResetStarted;
			_controls.Global.Disable();

			_ = _ui.HideLevelTitle();

			await _ui.FadeIn(Color.black);

			_ui.HideGameplay();

			// Save the track position
			if (_player)
			{
				if (_player.AngerState == AngerStates.Angry)
				{
					_audioPlayer.MusicTimes[_config.MusicCalmClip.GetInstanceID()] = _audioPlayer.MusicTimes[_config.MusicAngryClip.GetInstanceID()];
				}
				else
				{
					_audioPlayer.MusicTimes[_config.MusicAngryClip.GetInstanceID()] = _audioPlayer.MusicTimes[_config.MusicCalmClip.GetInstanceID()];
				}
			}

			foreach (var entity in _state.Entities)
			{
				Object.Destroy(entity.gameObject);
			}
			_state.Entities.Clear();
			_state.WalkableGrid = null;
			_state.TriggerExitAt = 0;
			_state.TriggerRetry = false;
			_state.PlayerDidAct = false;

			if (_state.Level)
			{
				Object.Destroy(_state.Level.gameObject);
				_state.Level = null;
			}
		}

		private void ConfirmStarted(InputAction.CallbackContext context) => _confirmWasPressedThisFrame = true;

		private void CancelStarted(InputAction.CallbackContext context) => _cancelWasPressedThisFrame = true;

		private void ResetStarted(InputAction.CallbackContext context) => _resetWasPressedThisFrame = true;

		private async UniTask Victory()
		{
			_running = false;
			await _audioPlayer.StopMusic(2f);
			_fsm.Fire(GameFSM.Triggers.Won);
		}

		private async UniTask<bool> Turn(Entity entity, Vector3Int destination)
		{
			entity.Direction = destination - entity.GridPosition;
			entity.Animator.SetFloat("DirectionX", entity.Direction.x);
			entity.Animator.SetFloat("DirectionY", entity.Direction.y);

			var destinationTile = _state.Level.Ground.GetTile(destination) as Tile;
			var entitiesAtDestination = _state.Entities.FindAll(entity => entity.GridPosition == destination);
			if (entitiesAtDestination.Count > 1)
			{
				UnityEngine.Debug.LogError("We don't handle multiple entities in the same position right now. See Resources/Levels/README.md for more informations.");
			}

			var entityAtDestination = entitiesAtDestination.Count > 0 ? entitiesAtDestination[0] : null;

			if (destinationTile == null)
			{
				if (entityAtDestination == null)
				{
					UnityEngine.Debug.Log($"Can't move to {destination} (tile is null).");
					return false;
				}
			}
			else
			{
				if (IsTileWalkable(destinationTile) == false)
				{
					UnityEngine.Debug.Log($"Can't move to {destination} (not walkable, {(destinationTile ? destinationTile.name : "null")}).");
					return false;
				}
			}

			if (entityAtDestination)
			{
				if (entityAtDestination.Trigger == false)
				{
					UnityEngine.Debug.Log($"Can't move to {destination} (occupied).");
					return false;
				}

				// if (entityAtDestination.TriggerState != AngerStates.None && entity.AngerState.HasFlag(entityAtDestination.TriggerState) == false)
				// {
				// 	UnityEngine.Debug.Log($"Can't move to {destination} (wrong state).");
				// 	return false;
				// }

				// if (entityAtDestination.CanBeActivated && entityAtDestination.Activated == false)
				// {
				// 	UnityEngine.Debug.Log($"Can't move to {destination} (entity not activated).");
				// 	return false;
				// }
			}

			entity.GridPosition = destination;
			var clips = entity.AngerState == AngerStates.Angry ? entity.WalkAngryAudioClips : entity.WalkCalmAudioClips;
			if (clips.Length > 0)
			{
				_ = _audioPlayer.PlayRandomSoundEffect(clips, entity.GridPosition);
			}
			entity.Animator.Play("Walk");
			await DOTween.To(() => entity.transform.position, x => entity.transform.position = x, entity.GridPosition + cellOffset, 1 / entity.MoveSpeed);
			entity.Animator.Play("Idle");

			await CheckTrigger(entity, entityAtDestination);

			return true;
		}

		private async UniTask CheckTrigger(Entity entity, Entity entityAtDestination)
		{
			if (entityAtDestination && entityAtDestination.Dead == false)
			{
				switch (entityAtDestination.TriggerAction)
				{
					case TriggerActions.Exit:
						{
							if (entity.ControlledByPlayer == false)
							{
								break;
							}

							if (entity.AngerState != entityAtDestination.TriggerState)
							{
								break;
							}

							if (entityAtDestination.Activated == false)
							{
								break;
							}

							if (entityAtDestination.ExitAudioClip)
							{
								_ = _audioPlayer.PlaySoundEffect(entityAtDestination.ExitAudioClip);
							}
							await NextLevel();
						}
						break;

					case TriggerActions.Break:
						{
							if (entity.AngerState != AngerStates.Angry)
							{
								break;
							}

							entityAtDestination.BreakableProgress += 1;

							if (entity.BreakParticle)
							{
								Object.Instantiate(entity.BreakParticle, entity.GridPosition + cellOffset + entity.BreakParticleOffset, Quaternion.identity);
							}
							if (entity.BreakGroundAudioClips.Length > 0)
							{
								_ = _audioPlayer.PlayRandomSoundEffect(entity.BreakGroundAudioClips, entity.GridPosition);
								await UniTask.Delay(300);
							}

							if (entityAtDestination.BreakableProgress >= entityAtDestination.BreaksAt)
							{
								entityAtDestination.Trigger = false;
								entityAtDestination.BreakableProgress = 0;
								entityAtDestination.Animator.Play("Breaking");

								if (entityAtDestination.BreakingAudioClip)
								{
									_ = _audioPlayer.PlaySoundEffect(entityAtDestination.BreakingAudioClip);
									await UniTask.Delay(500);
								}
								await CurrentAnimation(entityAtDestination);

								entity.Dead = true;
								if (entity.FallAudioClip)
								{
									_ = _audioPlayer.PlaySoundEffect(entity.FallAudioClip);
								}
								entity.Animator.Play("Death");
								await CurrentAnimation(entity);

								if (entity.ControlledByPlayer)
								{
									await PlayerDeath();
								}
							}
						}
						break;

					case TriggerActions.Key:
						{
							if (entity.ControlledByPlayer)
							{
								entityAtDestination.Dead = true;
								_state.KeysPickedUp += 1;

								if (entityAtDestination.KeyAudioClip)
								{
									_ = _audioPlayer.PlaySoundEffect(entityAtDestination.KeyAudioClip);
								}
							}
						}
						break;

					case TriggerActions.Fall:
						{
							if (entity.AngerState != AngerStates.Angry)
							{
								break;
							}

							entity.Dead = true;
							if (entity.FallAudioClip)
							{
								_ = _audioPlayer.PlaySoundEffect(entity.FallAudioClip);
							}
							entity.Animator.Play("Death");
							await CurrentAnimation(entity);

							if (entity.ControlledByPlayer)
							{
								await PlayerDeath();
							}

						}
						break;

					case TriggerActions.Burn:
						{
							if (entity.AngerState != AngerStates.Calm)
							{
								break;
							}

							entity.Dead = true;
							if (entity.FallAudioClip)
							{
								_ = _audioPlayer.PlaySoundEffect(entity.FallAudioClip);
							}
							entity.Animator.Play("Death");
							await CurrentAnimation(entity);

							if (entity.ControlledByPlayer)
							{
								await PlayerDeath();
							}
						}
						break;

					case TriggerActions.ActivateBurn:
						{
							if (entity.AngerState != AngerStates.Angry)
							{
								break;
							}

							entityAtDestination.Activated = true;
							entityAtDestination.Animator.SetBool("Active", entityAtDestination.Activated);
							await CurrentAnimation(entityAtDestination);

							entityAtDestination.TriggerAction = TriggerActions.Burn;
						}
						break;

					default: break;
				}
			}
		}

		private async UniTask NextLevel(bool skipDelay = false)
		{
			_state.CurrentLevelIndex += 1;
			_state.Running = false;
			_ = _audioPlayer.StopMusic();
			await _ui.FadeIn(Color.black);
			if (skipDelay == false)
			{
				await UniTask.Delay(3000);
			}
			_fsm.Fire(GameFSM.Triggers.NextLevel);
		}

		private async UniTask PlayerDeath()
		{
			_state.Running = false;
			_ = _audioPlayer.StopMusic();
			await _ui.FadeIn(Color.black);
			_fsm.Fire(GameFSM.Triggers.Retry);
		}

		private void ToggleMusic(Entity entity)
		{
			if (entity.AngerState != AngerStates.Angry)
			{
				_ = _audioPlayer.PlayMusic(_config.MusicCalmClip, false, 0.3f, 1f, true);
			}
			else
			{
				_ = _audioPlayer.PlayMusic(_config.MusicAngryClip, false, 0.3f, 1f, true);
			}
		}

		public static bool IsTileWalkable(Tile tile)
		{
			if (tile.colliderType == Tile.ColliderType.None)
			{
				return false;
			}

			return true;
		}
	}
}
