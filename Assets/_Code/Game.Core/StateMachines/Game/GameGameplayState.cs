using System.IO;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using FMOD.Studio;
using NesScripts.Controls.PathFind;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Tilemaps;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : BaseGameState
	{
		private bool _turnInProgress;
		private InputEventTrace.ReplayController _controller;
		private static string PLAYER_STATE_PARAM = "Player State";
		private static string PLAYER_ANGRY_PARAM = "Player Angry";
		private static string PLAYER_CALM_PARAM = "Player Calm";

		private static readonly Vector3 CellOffset = new Vector3(0.5f, 0.5f);
		private Entity Player => _state.Entities.Find((entity) => entity.ControlledByPlayer);

		public GameGameplayState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			// Initialize entities
			_state.KeysInLevel = _state.Entities.FindAll(e => e.TriggerAction == TriggerActions.Key).Count;
			foreach (var entity in _state.Entities)
			{
				entity.transform.position = entity.GridPosition + CellOffset;
				entity.MoodValue = entity.MoodMax = _state.Level.MoodMax;

				if (entity == Player)
					entity.Mood = _state.Level.DefaultPlayerMood;

				if (entity.Animator)
				{
					entity.Direction = Vector3Int.down;
					entity.Animator.SetFloat("DirectionX", entity.Direction.x);
					entity.Animator.SetFloat("DirectionY", entity.Direction.y);
					entity.Animator.SetFloat("AngerState", entity.Mood == Moods.Calm ? 0 : 1);
				}

				if (entity.CanBeActivated)
				{
					if (entity.ActivatesWhenLevelStart)
					{
						if (
								(entity.ActivatesInSpecificAngerState && entity.TriggerState == Player.Mood) ||
								(entity.ActivatesInSpecificAngerState == false && entity.TriggerState == Moods.None)
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
								(entity.ActivatesInSpecificAngerState && entity.TriggerState == Player.Mood) ||
								(entity.ActivatesInSpecificAngerState == false && entity.TriggerState == Moods.None)
							)
							{
								entity.Activated = true;
							}
						}
					}
				}
			}

			UpdatePlayerFMODParams(Player);

			_state.Running = true;

			_controls.Gameplay.Enable();
			_controls.Gameplay.Move.performed += OnMovePerformed;

			_ui.SetAngerMeter(Player.MoodValue, Player.MoodMax, Player.Mood);
			_ui.ShowGameplay();

			_game.LevelWalls.SetActive(true);

			_ = _ui.FadeOut(2);

#if UNITY_EDITOR
			if (_state.IsReplaying)
			{
				_state.TimeScaleCurrent = 10f;

				if (_state.TakeScreenshots)
				{
					await UniTask.Delay(2000);

					_ui.HideDebug();
					_ui.HideGameplay();
					await _ui.HideLevelName(0);
					_game.LevelWalls.SetActive(false);
					var levelDecoration = GameObject.Find("Decoration");
					if (levelDecoration != null)
						levelDecoration.SetActive(false);

					var levelAsset = _config.Levels[_state.CurrentLevelIndex];
					var levelPath = UnityEditor.AssetDatabase.GetAssetPath(levelAsset);
					var screenshotPath = levelPath.Replace($"{levelAsset.name}.prefab", $"{Utils.GetLevelIndex(_state.CurrentLevelIndex)}.png");
					ScreenCapture.CaptureScreenshot(screenshotPath);
					await UniTask.NextFrame();
				}

				if (_state.CurrentLevelIndex <= _config.Levels.Length - 1)
				{
					var levelAsset = _config.Levels[_state.CurrentLevelIndex];
					var levelPath = UnityEditor.AssetDatabase.GetAssetPath(levelAsset);
					var replayPath = levelPath.Replace($"{levelAsset.name}.prefab", $"{Utils.GetLevelIndex(_state.CurrentLevelIndex)}.inputtrace");
					UnityEngine.Debug.Log("Loading input trace from: " + replayPath);
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
			}
#endif

			_state.LevelMusic.getPlaybackState(out var state);
			if (state != PLAYBACK_STATE.PLAYING)
			{
				_state.LevelMusic.start();
			}
			_state.LevelMusic.setPitch(_state.TimeScaleCurrent);
		}

		public override void Tick()
		{
			base.Tick();

			if (Utils.IsDevBuild())
			{
				FMODUnity.RuntimeManager.StudioSystem.getParameterByName(PLAYER_ANGRY_PARAM, out var angryProgress);
				FMODUnity.RuntimeManager.StudioSystem.getParameterByName(PLAYER_CALM_PARAM, out var calmProgress);
				_ui.SetDebugText($@"
- F1: load next level
- F2: trigger credits
- T: start record
- K: toggle replay
- R: restart level

AngerState: {(Player.Mood == Moods.Calm ? 0 : 1)}
AngerProgress(raw): {Player.MoodValue}
AngerProgress(calm): {calmProgress}
AngerProgress(angry): {angryProgress}
");
			}

			if (_state.Running)
			{
				if (_controls.Global.Pause.WasPerformedThisFrame())
				{
					if (_state.Paused)
					{
						_state.TimeScaleCurrent = _state.TimeScaleDefault;
						_state.Paused = false;
						_game.PauseUI.Hide();
						_state.PauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
					}
					else
					{
						_state.TimeScaleCurrent = 0f;
						_state.Paused = true;
						_ = _game.PauseUI.Show();
						_state.PauseSnapshot.start();
					}
				}

				if (_controls.Global.Cancel.WasPerformedThisFrame())
				{
					if (_game.OptionsUI.IsOpened)
					{
						_game.OptionsUI.Hide();
						_game.PauseUI.SelectOptionsGameObject();
						_game.Save.SavePlayerSettings(_game.State.PlayerSettings);
					}
				}

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

				if (_controls.Gameplay.Reset.WasPerformedThisFrame())
				{
					AudioHelpers.PlayOneShot(_config.SoundLevelRestart);
					_fsm.Fire(GameFSM.Triggers.Retry);
				}

				if (Utils.IsDevBuild())
				{
					if (Keyboard.current.f1Key.wasReleasedThisFrame)
					{
						NextLevel();
					}

					if (Keyboard.current.f2Key.wasReleasedThisFrame)
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
						if (_state.IsReplaying == false)
						{
							_state.IsReplaying = true;
							_fsm.Fire(GameFSM.Triggers.Retry);
						}
						else
						{
							_state.IsReplaying = false;
							_fsm.Fire(GameFSM.Triggers.Retry);
						}
					}
				}
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_controls.Gameplay.Disable();
			_controls.Gameplay.Move.performed -= OnMovePerformed;

			_state.TimeScaleCurrent = _state.TimeScaleDefault;
			_state.Running = false;
			_state.Paused = false;
			_state.PauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);

			if (_controller != null)
			{
				_controller.Dispose();
				_controller = null;
			}

#if UNITY_EDITOR
			if (_game.InputRecorder.captureIsRunning)
			{
				_game.InputRecorder.StopCapture();
				UnityEngine.Debug.LogWarning("Pausing the game to save the level inputs!");
				Debug.Break();
			}
#endif

			await _ui.FadeIn(Color.black);
			await _ui.HideLevelName(0);
			_ui.HideGameplay();
			await _game.PauseUI.Hide(0);
			await _game.OptionsUI.Hide(0);

			FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_STATE_PARAM, 0);
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_ANGRY_PARAM, 0);
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_CALM_PARAM, 0);

			_game.LevelWalls.SetActive(false);

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

		private static void UpdatePlayerFMODParams(Entity player)
		{
			var isCalm = player.Mood == Moods.Calm;
			if (isCalm)
			{
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_STATE_PARAM, 0);
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_ANGRY_PARAM, player.MoodMax - player.MoodValue);
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_CALM_PARAM, player.MoodValue);
			}
			else
			{
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_STATE_PARAM, 1);
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_ANGRY_PARAM, player.MoodValue);
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName(PLAYER_CALM_PARAM, player.MoodMax - player.MoodValue);
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
			await PlayerTurn(context.ReadValue<Vector2>());
			_turnInProgress = false;
		}

		private async UniTask PlayerTurn(Vector2 moveInput)
		{
			if (moveInput.magnitude == 0)
				return;

			if (Player.Dead)
				return;

			var playerDestination = Player.GridPosition + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);
			var playerDidMove = await Turn(Player, playerDestination);
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
							if (entity.PreventMoodChangeThisFrame == false)
								entity.MoodValue -= 1;

							entity.PreventMoodChangeThisFrame = false;

							if (entity == Player)
								UpdatePlayerFMODParams(Player);

							if (entity.MoodValue < 1)
							{
								entity.MoodValue = entity.MoodMax;
								entity.Mood = entity.Mood == Moods.Calm ? Moods.Angry : Moods.Calm;
								var isCalm = entity.Mood == Moods.Calm;
								entity.Animator.SetFloat("AngerState", isCalm ? 0 : 1);

								if (entity == Player)
								{
									UpdatePlayerFMODParams(Player);
								}

								entity.Direction = Vector3Int.down;
								entity.Animator.SetFloat("DirectionX", entity.Direction.x);
								entity.Animator.SetFloat("DirectionY", entity.Direction.y);

								AudioHelpers.PlayOneShot(entity.SoundTransformation, entity.GridPosition);

								entity.Animator.Play("Transform");
								await WaitForCurrentAnimation(entity);

								var otherEntityAtPosition = _state.Entities.Find(e => e.GridPosition == entity.GridPosition && e != entity);
								if (_state.Running && otherEntityAtPosition)
								{
									await CheckTrigger(entity, otherEntityAtPosition);
								}
							}

							_ui.SetAngerMeter(entity.MoodValue, entity.MoodMax, entity.Mood);
						}
					}
				}

				foreach (var entity in _state.Entities)
				{
					if (entity.CanBeActivated && entity.ActivatesInSpecificAngerState)
					{
						if (entity.Activated == false && Player.Mood == entity.TriggerState)
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
							if (Player.Mood != entity.TriggerState)
							{
								entity.Activated = false;
							}
						}
					}
				}
			}
			else
			{
				AudioHelpers.PlayOneShot(Player.SoundCantMoveAudio, Player.GridPosition);
			}

			if (Player.Dead)
			{
				_state.Running = false;
				await UniTask.Delay(500);
				_fsm.Fire(GameFSM.Triggers.Retry);
			}
		}

		private UniTask WaitForCurrentAnimation(Entity entity)
		{
			var infos = entity.Animator.GetCurrentAnimatorClipInfo(0);
			if (infos.Length == 0)
				return default;

			var clipName = entity.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
			if (entity.AnimationClipLength.ContainsKey(clipName) == false)
				return default;

			var length = entity.AnimationClipLength[clipName];
			return UniTask.Delay(System.TimeSpan.FromSeconds(length / _state.TimeScaleCurrent));
		}

		private void Victory()
		{
			UnityEngine.Debug.Log("End of the game reached.");
			_state.LevelMusic.start();
			_fsm.Fire(GameFSM.Triggers.Won);
		}

		private async UniTask<bool> Turn(Entity entity, Vector3Int destination)
		{
			entity.Direction = destination - entity.GridPosition;
			entity.Animator.SetFloat("DirectionX", entity.Direction.x);
			entity.Animator.SetFloat("DirectionY", entity.Direction.y);

			var destinationTile = _state.Level.Ground.GetTile(destination) as Tile;
			var entitiesAtDestination = _state.Entities.FindAll(e => e.GridPosition == destination);

			foreach (var entityAtDestination in entitiesAtDestination)
			{
				if (entityAtDestination.Trigger == false)
				{
					UnityEngine.Debug.Log($"Can't move to {destination.ToString()} (occupied).");
					return false;
				}
			}

			// Move
			{
				if (entity.Mood == Moods.Angry)
					AudioHelpers.PlayOneShot(entity.SoundWalkAngry, entity.GridPosition);
				else
					AudioHelpers.PlayOneShot(entity.SoundWalkCalm, entity.GridPosition);

				entity.GridPosition = destination;
				entity.Animator.Play("Walk");
				await DOTween.To(
					() => entity.transform.position,
					x => entity.transform.position = x,
					entity.GridPosition + CellOffset,
					1 / entity.MoveSpeed / _state.TimeScaleCurrent
				);
				entity.Animator.Play("Idle");

				if (
					(destinationTile == null && entitiesAtDestination.Count == 0) ||
					(destinationTile != null && IsTileWalkable(destinationTile) == false)
				)
				{
					await Fall(entity);
				}
			}

			foreach (var entityAtDestination in entitiesAtDestination)
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

							if (entity.Mood != entityAtDestination.TriggerState)
							{
								break;
							}

							if (entityAtDestination.Activated == false)
							{
								break;
							}

							AudioHelpers.PlayOneShot(entityAtDestination.SoundExit, entityAtDestination.GridPosition);
							NextLevel();
						}
						break;

					case TriggerActions.Break:
						{
							if (entity.Mood != Moods.Angry)
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
								AudioHelpers.PlayOneShot(entityAtDestination.SoundBreaking, entityAtDestination.GridPosition);
								entityAtDestination.Trigger = false;
								entityAtDestination.BreakableProgress = 0;
								entityAtDestination.Animator.Play("Breaking");
								await WaitForCurrentAnimation(entityAtDestination);

								await Fall(entity);
							}
							else
							{
								AudioHelpers.PlayOneShot(entity.SoundBreakGround, entity.GridPosition);
							}
						}
						break;

					case TriggerActions.Key:
						{
							if (entity.ControlledByPlayer)
							{
								entityAtDestination.Dead = true;
								_state.KeysPickedUp += 1;

								AudioHelpers.PlayOneShot(entityAtDestination.SoundKey, entityAtDestination.GridPosition);
							}
						}
						break;

					case TriggerActions.Fall:
						{
							if (entity.Mood != Moods.Angry)
							{
								break;
							}

							await Fall(entity);
						}
						break;

					case TriggerActions.Burn:
						{
							if (entity.Mood != Moods.Calm)
							{
								break;
							}

							await Fall(entity);
						}
						break;

					case TriggerActions.ActivateBurn:
						{
							if (entity.Mood != Moods.Angry)
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
								1 / entity.MoveSpeed / _state.TimeScaleCurrent
							);
							UnityEngine.Debug.Log("Push!");
						}
						break;

					case TriggerActions.IncreaseMood:
						{
							entity.PreventMoodChangeThisFrame = true;
							entity.MoodValue = Mathf.Clamp(entity.MoodValue + entityAtDestination.IncreaseMood, 0, entity.MoodMax);
							AudioHelpers.PlayOneShot(entityAtDestination.SoundIncreaseMood, entityAtDestination.GridPosition);
						}
						break;

					default: break;
				}
			}
		}

		private async UniTask Fall(Entity entity)
		{
			entity.Dead = true;

			if (entity.Mood == Moods.Angry)
			{
				AudioHelpers.PlayOneShot(entity.SoundFall, entity.GridPosition);
				await UniTask.Delay(400); // This delay adds a cartoon effect where the entity floats in the air before falling.
			}
			else
			{
				AudioHelpers.PlayOneShot(entity.SoundBurn, entity.GridPosition);
			}

			entity.Animator.Play("Death");
			await WaitForCurrentAnimation(entity);
		}

		private void NextLevel()
		{
			_game.State.PlayerSaveData.ClearedLevels.Add(_state.CurrentLevelIndex);
			_game.Save.SavePlayerSaveData(_game.State.PlayerSaveData);

			_state.CurrentLevelIndex += 1;

			if (_state.CurrentLevelIndex >= _config.Levels.Length)
			{
				Victory();
				return;
			}

			_state.Running = false;
			_fsm.Fire(GameFSM.Triggers.NextLevel);
		}

		public static bool IsTileWalkable(Tile tile)
		{
			if (tile.colliderType == Tile.ColliderType.None)
				return false;

			return true;
		}
	}
}
