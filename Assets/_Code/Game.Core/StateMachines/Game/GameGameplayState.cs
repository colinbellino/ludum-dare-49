using System.IO;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NesScripts.Controls.PathFind;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Tilemaps;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : BaseGameState
	{
		private bool _resetWasPressedThisFrame;

		private static Vector3 cellOffset = new Vector3(0.5f, 0.5f);
		private float _noTransitionTimestamp;
		private bool _turnInProgress;
		private InputEventTrace.ReplayController _controller;

		private Entity _player => _state.Entities.Find((entity) => entity.ControlledByPlayer);

		public GameGameplayState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			if (Utils.IsDevBuild())
			{
				_ui.SetDebugText("[DEBUG]\n- F1: trigger victory\n- K: start replay");
			}

			// TODO: Remove this ?
			_noTransitionTimestamp = Time.time + 3f;

			// TODO: Why isn't this done in the level loading state ?
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

			_controls.Gameplay.Enable();
			_controls.Gameplay.Reset.started += ResetStarted;
			_controls.Gameplay.Move.performed += OnMovePerformed;
			_controls.Global.Enable();

			_ui.SetAngerMeter(_player.AngerProgress, _player.AngerState);
			_ui.ShowGameplay();

			_ = _ui.FadeOut(2);

			if (_state.IsReplaying)
			{
				var replayPath = $"{Application.dataPath}/Resources/Levels/{_state.CurrentLevelIndex + 1:D2}.inputtrace";
				// UnityEngine.Debug.Log("Loading input trace: " + replayPath);
				if (File.Exists(replayPath))
				{
					_game.InputRecorder.ClearCapture();
					_game.InputRecorder.LoadCaptureFromFile(replayPath);
					_controller = _game.InputRecorder.capture.Replay();
					_controller.WithAllDevicesMappedToNewInstances();

					var current = default(InputEventPtr);
					while (_game.InputRecorder.capture.GetNextEvent(ref current))
					{
						if (current == null)
						{
							break;
						}

						if (_controller == null)
						{
							break;
						}

						_controller.PlayOneEvent();
						await UniTask.Delay(300);
					}
				}
				else
				{
					UnityEngine.Debug.LogWarning("Input trace for this level doesn't exit.");
				}
			}
		}

		private async void OnMovePerformed(InputAction.CallbackContext context)
		{
			if (_turnInProgress)
			{
				return;
			}

			if (_state.Running == false)
			{
				return;
			}

			_turnInProgress = true;
			await Loop(context.ReadValue<Vector2>());
			_turnInProgress = false;
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

					if (entity.HasActiveAnimation)
					{
						entity.Animator.SetBool("Active", entity.Activated);
					}
				}

				// TODO: Clean this up
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

					if (Keyboard.current.f2Key.wasReleasedThisFrame)
					{
						NextLevel();
					}
				}

				if (Utils.IsDevBuild())
				{
					if (Keyboard.current.f1Key.wasReleasedThisFrame)
					{
						await Victory();
					}

					if (Keyboard.current.tKey.wasReleasedThisFrame)
					{
						_game.InputRecorder.ClearCapture();
						_game.InputRecorder.StartCapture();
					}

					if (Keyboard.current.kKey.wasReleasedThisFrame)
					{
						_state.IsReplaying = true;
						_state.CurrentTimeScale = 5f;
						_fsm.Fire(GameFSM.Triggers.Retry);
					}
				}
			}

			_resetWasPressedThisFrame = false;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_state.Running = false;

			if (_controller != null)
			{
				_controller.Dispose();
				_controller = null;
			}

			if (Utils.IsDevBuild())
			{
				if (_game.InputRecorder.captureIsRunning)
				{
					_game.InputRecorder.StopCapture();
					UnityEngine.Debug.LogWarning("Pausing the game to save the level inputs!");
					Debug.Break();
				}
			}

			_controls.Gameplay.Disable();
			_controls.Gameplay.Reset.started -= ResetStarted;
			_controls.Gameplay.Move.performed -= OnMovePerformed;
			_controls.Global.Disable();

			await _ui.FadeIn(Color.black);

			_ = _ui.HideLevelTitle();
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

			_state.Entities.Clear();
			_state.WalkableGrid = null;
			_state.TriggerExitAt = 0;
			_state.TriggerRetry = false;
			_state.PlayerDidAct = false;
			_state.KeysPickedUp = 0;

			if (_state.Level)
			{
				_state.Level.gameObject.SetActive(false);
				Object.Destroy(_state.Level.gameObject);
				_state.Level = null;
			}
		}

		private void ResetStarted(InputAction.CallbackContext context) => _resetWasPressedThisFrame = true;

		private async UniTask Loop(Vector2 moveInput)
		{
			if (moveInput.magnitude == 0)
			{
				return;
			}

			if (_player.Dead)
			{
				return;
			}

			var destination = _player.GridPosition + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);

			var playerDidMove = await Turn(_player, destination);
			if (playerDidMove)
			{
				foreach (var entity in _state.Entities)
				{
					// if (entity.MoveTowardsPlayer)
					// {
					// 	var path = Pathfinding.FindPath(
					// 		_state.WalkableGrid,
					// 		entity.GridPosition, _player.GridPosition,
					// 		Pathfinding.DistanceType.Manhattan
					// 	);
					// 	await Turn(entity, path[0]);
					// }

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
								await WaitForCurrentAnimation(entity);

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
					if (entity.CanBeActivated && entity.ActivatesInSpecificAngerState)
					{
						if (entity.Activated == false && _player.AngerState == entity.TriggerState)
						{
							if (
								(_state.KeysInLevel == 0) ||
								_state.KeysInLevel > 0 && _state.KeysPickedUp >= _state.KeysInLevel)
							{
								entity.Activated = true;
								entity.CanBeActivated = false;
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

			if (_player.Dead)
			{
				_state.Running = false;
				_ = _audioPlayer.StopMusic();
				await UniTask.Delay(500);
				_fsm.Fire(GameFSM.Triggers.Retry);
			}
		}

		private UniTask WaitForCurrentAnimation(Entity entity)
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

			var length = entity.AnimationClipLength[clipName];
			return UniTask.Delay(System.TimeSpan.FromSeconds(length / Time.timeScale));
		}

		private async UniTask Victory()
		{
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
				// 	if (entityAtDestination == null)
				// 	{
				// 		UnityEngine.Debug.Log($"Can't move to {destination} (tile is null).");
				// 		return false;
				// 	}
				// }
				// else
				// {
				// 	if (IsTileWalkable(destinationTile) == false)
				// 	{
				// 		UnityEngine.Debug.Log($"Can't move to {destination} (not walkable, {(destinationTile ? destinationTile.name : "null")}).");
				// 		return false;
				// 	}
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
			await DOTween.To(
				() => entity.transform.position,
				x => entity.transform.position = x,
				entity.GridPosition + cellOffset,
				1 / entity.MoveSpeed / Time.timeScale
			);
			entity.Animator.Play("Idle");

			if (
				(destinationTile == null && entityAtDestination == null) ||
				(destinationTile != null && IsTileWalkable(destinationTile) == false))
			{
				await Fall(entity);
			}

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
							NextLevel();
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
								await WaitForCurrentAnimation(entityAtDestination);

								await Fall(entity);
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

							await Fall(entity);
						}
						break;

					case TriggerActions.Burn:
						{
							if (entity.AngerState != AngerStates.Calm)
							{
								break;
							}

							await Fall(entity);
						}
						break;

					case TriggerActions.ActivateBurn:
						{
							if (entity.AngerState != AngerStates.Angry)
							{
								break;
							}

							entityAtDestination.Activated = true;
							if (entityAtDestination.HasActiveAnimation)
							{
								entityAtDestination.Animator.SetBool("Active", entityAtDestination.Activated);
							}
							await WaitForCurrentAnimation(entityAtDestination);

							entityAtDestination.TriggerAction = TriggerActions.Burn;
						}
						break;

					default: break;
				}
			}
		}

		private async UniTask Fall(Entity entity)
		{
			entity.Dead = true;
			var clip = entity.AngerState == AngerStates.Angry ? entity.FallAudioClip : entity.BurnAudioClip;
			if (clip)
			{
				_ = _audioPlayer.PlaySoundEffect(clip);
			}
			entity.Animator.Play("Death");
			await WaitForCurrentAnimation(entity);
		}

		private void NextLevel()
		{
			_state.CurrentLevelIndex += 1;
			_state.Running = false;
			_ = _audioPlayer.StopMusic();
			_fsm.Fire(GameFSM.Triggers.NextLevel);
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
