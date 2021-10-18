using System.IO;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NesScripts.Controls.PathFind;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Tilemaps;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : BaseGameState
	{
		private bool _resetWasPressedThisFrame;
		private EventInstance _music;
		private float _noTransitionTimestamp;
		private bool _turnInProgress;
		private InputEventTrace.ReplayController _controller;

		private static readonly Vector3 CellOffset = new Vector3(0.5f, 0.5f);
		private Entity Player => _state.Entities.Find((entity) => entity.ControlledByPlayer);

		public GameGameplayState(GameFSM fsm, GameSingleton game) : base(fsm, game)
		{
			_music = FMODUnity.RuntimeManager.CreateInstance(_config.MusicLevel);
		}

		public override async UniTask Enter()
		{
			await base.Enter();

			if (Utils.IsDevBuild())
			{
				_ui.SetDebugText("[DEBUG]\n- F1: trigger victory\n- K: start replay");
			}

			// TODO: Remove this ?
			_noTransitionTimestamp = Time.time + 3f;

			// Initialize entities
			_state.KeysInLevel = _state.Entities.FindAll(e => e.TriggerAction == TriggerActions.Key).Count;
			foreach (var entity in _state.Entities)
			{
				entity.transform.position = entity.GridPosition + CellOffset;

				if (entity.CanBeActivated)
				{
					if (entity.ActivatesWhenLevelStart)
					{
						if (
								(entity.ActivatesInSpecificAngerState && entity.TriggerState == Player.AngerState) ||
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
								(entity.ActivatesInSpecificAngerState && entity.TriggerState == Player.AngerState) ||
								(entity.ActivatesInSpecificAngerState == false && entity.TriggerState == AngerStates.None)
							)
							{
								entity.Activated = true;
							}
						}
					}
				}
			}

			_state.Running = true;

			_controls.Gameplay.Enable();
			_controls.Gameplay.Reset.started += ResetStarted;
			_controls.Gameplay.Move.performed += OnMovePerformed;
			_controls.Global.Enable();

			_ui.SetAngerMeter(Player.AngerProgress, Player.AngerState);
			_ui.ShowGameplay();

			_music.getPlaybackState(out var state);
			if (state != PLAYBACK_STATE.PLAYING)
			{
				_music.start();
			}

			_ = _ui.FadeOut(2);

			if (_state.IsReplaying)
			{
				if (_config.TakeScreenshots)
				{
					await UniTask.Delay(2000);

					_ui.HideDebug();
					ScreenCapture.CaptureScreenshot($"Assets/Resources/Levels/{_state.CurrentLevelIndex.ToString() + 1:D2}.png");
					await UniTask.NextFrame();
					_ui.ShowDebug();
				}

#if UNITY_EDITOR
				if (_state.CurrentLevelIndex <= _config.Levels.Length - 1)
				{
					var levelAsset = _config.Levels[_state.CurrentLevelIndex];
					var levelPath = UnityEditor.AssetDatabase.GetAssetPath(levelAsset);
					var replayPath = levelPath.Replace($"{levelAsset.name}.prefab", $"{_state.CurrentLevelIndex.ToString() + 1:D2}.inputtrace");
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
						UnityEngine.Debug.LogWarning("Input trace for this level doesn't exist.");
					}
				}
#endif
			}
		}

		private async void OnMovePerformed(InputAction.CallbackContext context)
		{
			if (_turnInProgress)
			{
				return;
			}

			if (_state.Running == false || _state.Paused)
			{
				return;
			}

			_turnInProgress = true;
			await Loop(context.ReadValue<Vector2>());
			_turnInProgress = false;
		}

		public override void Tick()
		{
			base.Tick();

			if (_controls.Global.Pause.WasPerformedThisFrame())
			{
				if (_state.Paused)
				{
					Time.timeScale = _state.TimeScaleDefault;
					_state.Paused = false;
					// FIXME: FMOD
					// _audioPlayer.SetMusicVolume(_state.IsMusicPlaying ? 1 : 0);
					_game.Pause.Hide();
				}
				else
				{
					Time.timeScale = 0f;
					_state.Paused = true;
					// FIXME: FMOD
					// _audioPlayer.SetMusicVolume(_state.IsMusicPlaying ? 0.1f : 0);
					_game.Pause.Show();
				}
			}

			if (_state.Running && _state.Paused == false)
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
						FMODUnity.RuntimeManager.PlayOneShot(_config.SoundLevelRestart);
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
						Victory();
					}

					if (Keyboard.current.tKey.wasReleasedThisFrame)
					{
						_game.InputRecorder.ClearCapture();
						_game.InputRecorder.StartCapture();
					}

					if (Keyboard.current.kKey.wasReleasedThisFrame)
					{
						_state.IsReplaying = true;
						_state.TimeScaleCurrent = 5f;
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

			_state.Entities.Clear();
			_state.WalkableGrid = null;
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

			if (Player.Dead)
			{
				return;
			}

			var destination = Player.GridPosition + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);

			var playerDidMove = await Turn(Player, destination);
			if (playerDidMove)
			{
				foreach (var entity in _state.Entities)
				{
					if (entity.MoveTowardsPlayer)
					{
						var path = Pathfinding.FindPath(
							_state.WalkableGrid,
							entity.GridPosition, Player.GridPosition,
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

							if (entity == Player)
								FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Player Anger Progress", Player.AngerProgress);

							if (entity.AngerProgress >= 3)
							{
								entity.AngerProgress = 0;
								entity.AngerState = (entity.AngerState == AngerStates.Calm) ? AngerStates.Angry : AngerStates.Calm;
								entity.Animator.SetFloat("AngerState", (entity.AngerState == AngerStates.Calm) ? 0 : 1);
								if (entity == Player)
									FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Player Anger State", Player.AngerState == AngerStates.Calm ? 0 : 1);

								entity.Direction = Vector3Int.down;
								entity.Animator.SetFloat("DirectionX", entity.Direction.x);
								entity.Animator.SetFloat("DirectionY", entity.Direction.y);

								FMODUnity.RuntimeManager.PlayOneShot(entity.SoundTransformation, entity.GridPosition);

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
						if (entity.Activated == false && Player.AngerState == entity.TriggerState)
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
							if (Player.AngerState != entity.TriggerState)
							{
								entity.Activated = false;
							}
						}
					}
				}
			}
			else
			{
				FMODUnity.RuntimeManager.PlayOneShot(Player.SoundCantMoveAudio, Player.GridPosition);
			}

			if (Player.Dead)
			{
				_state.Running = false;
				// _ = _audioPlayer.StopMusic();
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

		private void Victory()
		{
			UnityEngine.Debug.Log("End of the game reached.");
			_music.start();
			_fsm.Fire(GameFSM.Triggers.Won);
		}

		private async UniTask<bool> Turn(Entity entity, Vector3Int destination)
		{
			entity.Direction = destination - entity.GridPosition;
			entity.Animator.SetFloat("DirectionX", entity.Direction.x);
			entity.Animator.SetFloat("DirectionY", entity.Direction.y);

			var destinationTile = _state.Level.Ground.GetTile(destination) as Tile;
			var entitiesAtDestination = _state.Entities.FindAll(e => e.GridPosition == destination);
			if (entitiesAtDestination.Count > 1)
			{
				UnityEngine.Debug.LogError("We don't handle multiple entities in the same position right now. See Resources/Levels/README.md for more informations.");
			}

			var entityAtDestination = entitiesAtDestination.Count > 0 ? entitiesAtDestination[0] : null;

			if (entityAtDestination)
			{
				if (entityAtDestination.Trigger == false)
				{
					UnityEngine.Debug.Log($"Can't move to {destination.ToString()} (occupied).");
					return false;
				}
			}

			if (entity.AngerState == AngerStates.Angry)
				FMODUnity.RuntimeManager.PlayOneShot(entity.SoundWalkAngry, entity.GridPosition);
			else
				FMODUnity.RuntimeManager.PlayOneShot(entity.SoundWalkCalm, entity.GridPosition);

			entity.GridPosition = destination;
			entity.Animator.Play("Walk");
			await DOTween.To(
				() => entity.transform.position,
				x => entity.transform.position = x,
				entity.GridPosition + CellOffset,
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

							FMODUnity.RuntimeManager.PlayOneShot(entityAtDestination.SoundExit, entityAtDestination.GridPosition);
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
								Object.Instantiate(entity.BreakParticle, entity.GridPosition + CellOffset + entity.BreakParticleOffset, Quaternion.identity);
							}

							if (entityAtDestination.BreakableProgress >= entityAtDestination.BreaksAt)
							{
								FMODUnity.RuntimeManager.PlayOneShot(entityAtDestination.SoundBreaking, entityAtDestination.GridPosition);
								entityAtDestination.Trigger = false;
								entityAtDestination.BreakableProgress = 0;
								entityAtDestination.Animator.Play("Breaking");
								await WaitForCurrentAnimation(entityAtDestination);

								await Fall(entity);
							}
							else
							{
								FMODUnity.RuntimeManager.PlayOneShot(entity.SoundBreakGround, entity.GridPosition);
							}
						}
						break;

					case TriggerActions.Key:
						{
							if (entity.ControlledByPlayer)
							{
								entityAtDestination.Dead = true;
								_state.KeysPickedUp += 1;

								FMODUnity.RuntimeManager.PlayOneShot(entityAtDestination.SoundKey, entityAtDestination.GridPosition);
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

					case TriggerActions.Push:
						{
							entityAtDestination.GridPosition += entity.Direction;
							await DOTween.To(
								() => entityAtDestination.transform.position,
								x => entityAtDestination.transform.position = x,
								entityAtDestination.GridPosition + CellOffset,
								1 / entity.MoveSpeed / Time.timeScale
							);
							UnityEngine.Debug.Log("Push!");
						}
						break;

					default: break;
				}
			}
		}

		private async UniTask Fall(Entity entity)
		{
			entity.Dead = true;

			if (entity.AngerState == AngerStates.Angry)
			{
				FMODUnity.RuntimeManager.PlayOneShot(entity.SoundFall, entity.GridPosition);
				await UniTask.Delay(500); // This delay adds a cartoon effect where the entity floats in the air before falling.
			}
			else
			{
				FMODUnity.RuntimeManager.PlayOneShot(entity.SoundBurn, entity.GridPosition);
			}

			entity.Animator.Play("Death");
			await WaitForCurrentAnimation(entity);
		}

		private void NextLevel()
		{
			_state.CurrentLevelIndex += 1;

			if (_state.CurrentLevelIndex == _config.Levels.Length - 1)
			{
				Victory();
				return;
			}

			_state.Running = false;
			_fsm.Fire(GameFSM.Triggers.NextLevel);
		}

		private void ToggleMusic(Entity entity)
		{
			if (entity.AngerState != AngerStates.Angry)
			{
				// transition to calm
				// _ = _audioPlayer.PlayMusic(_config.MusicCalmClip, Utils.GetMusicVolume(_state), false, 0.3f, true);
			}
			else
			{
				// transition to angry
				// _ = _audioPlayer.PlayMusic(_config.MusicAngryClip, Utils.GetMusicVolume(_state), false, 0.3f, true);
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
