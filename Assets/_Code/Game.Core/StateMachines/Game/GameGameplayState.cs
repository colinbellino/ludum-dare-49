using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NesScripts.Controls.PathFind;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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

		public GameGameplayState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.SetDebugText(@"[DEBUG]
- F1: trigger victory
- F2: trigger defeat
- ESCAPE: pause/options");

			// Load current level
			{
				if (_state.CurrentLevelIndex > _config.AllLevels.Length - 1)
				{
					Victory();
					return;
				}

				_state.Level = Object.Instantiate(_config.AllLevels[_state.CurrentLevelIndex]);
				UnityEngine.Debug.Log("Loaded level: " + _state.Level.name);

				// Generate grid for walkable tiles
				var tilemap = _state.Level.Ground;
				var walkableTiles = new bool[tilemap.size.x, tilemap.size.y];
				for (var x = 0; x < tilemap.size.x - 0; x++)
				{
					for (var y = 0; y < tilemap.size.y - 0; y++)
					{
						var position = new Vector3Int(x, y, 0);
						walkableTiles[x, y] = IsTileWalkable(tilemap.GetTile(position));
					}
				}
				_state.WalkableGrid = new GridData(walkableTiles);

				// Spawn entities
				SpawnEntitiesFromTilemap(_state.Level.Ground);
				SpawnEntitiesFromTilemap(_state.Level.Entities);
			}

			// Initialize entities
			foreach (var entity in _state.Entities)
			{
				entity.transform.position = entity.GridPosition + cellOffset;

				if (entity.CanBeActivated)
				{
					if (entity.ActivatesWhenKeyInLevel)
					{
						var keys = _state.Entities.FindAll(e => e.TriggerAction == TriggerActions.Key);
						if (keys.Count == 0)
						{
							entity.Activated = true;
						}
					}
				}
			}

			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)UnityEngine.Random.Range(0, int.MaxValue));

			// Start or continue music where we left off
			{
				var player = _state.Entities.Find((entity) => entity.ControlledByPlayer);
				var moodClip = player.AngerState == AngerStates.Calm ? _config.MusicCalmClip : _config.MusicAngryClip;
				if (_audioPlayer.IsMusicPlaying() == false)
				{
					_ = _audioPlayer.PlayMusic(moodClip, true, 0.5f);
				}
				else
				{
					ToggleMusic(player);
				}
			}

			_state.Running = true;

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;
			_controls.Gameplay.Cancel.started += CancelStarted;
			_controls.Gameplay.Reset.started += ResetStarted;
			_controls.Global.Enable();

			_ = _ui.FadeOut();

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

				var player = _state.Entities.Find((entity) => entity.ControlledByPlayer);

				if (player.Dead)
				{
					continue;
				}

				var destination = player.GridPosition + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);
				var playerDidMove = await Turn(player, destination);
				if (playerDidMove)
				{
					foreach (var entity in _state.Entities)
					{
						if (entity.MoveTowardsPlayer)
						{
							var path = Pathfinding.FindPath(
								_state.WalkableGrid,
								entity.GridPosition, player.GridPosition,
								Pathfinding.DistanceType.Manhattan
							);
							await Turn(entity, path[0]);
						}

						if (entity.Breaking)
						{
							entity.Animator.Play("Breaking");
							entity.Trigger = false;
							entity.Breaking = false;
							entity.BreakableProgress = 0;

							var entityAtPosition = _state.Entities.Find(e => e.GridPosition == entity.GridPosition && e != entity);
							if (entityAtPosition)
							{
								if (entityAtPosition.FallAudioClip)
								{
									_ = _audioPlayer.PlaySoundEffect(entityAtPosition.FallAudioClip);
								}
								entityAtPosition.Animator.Play("Fall");
								await CurrentAnimation(entityAtPosition);
								await UniTask.Delay(500);

								entityAtPosition.Dead = true;

								if (entityAtPosition.ControlledByPlayer)
								{
									_state.Running = false;

									_ = _audioPlayer.StopMusic(2);
									await _ui.FadeIn(Color.black);
									await UniTask.Delay(1000);
									_fsm.Fire(GameFSM.Triggers.Retry);
									return;
								}
							}
						}

						if (entity.CanBeActivated && entity.Activated == false && _state.Keys >= entity.ActivatesWithKeys)
						{
							entity.Activated = true;
						}

						if (entity.Dead)
						{
							entity.Animator.Play("Dead");
						}
						else
						{
							if (entity.AffectedByAnger)
							{
								entity.AngerProgress += 1;

								if (entity.AngerProgress >= 3)
								{
									entity.AngerProgress = 0;
									entity.AngerState = (entity.AngerState == AngerStates.Calm) ? AngerStates.Angry : AngerStates.Calm;
									entity.Transforming = true;
									entity.Animator.SetFloat("AngerState", (entity.AngerState == AngerStates.Calm) ? 0 : 1);

									entity.Animator.Play("Transform");
									await CurrentAnimation(entity);

									if (entity.ControlledByPlayer)
									{
										ToggleMusic(entity);
									}
								}
							}
						}
					}
				}
			}
		}

		private UniTask CurrentAnimation(Entity entity)
		{
			return UniTask.Delay(System.TimeSpan.FromSeconds(entity.AnimationClipLength[entity.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name]));
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

				if (_resetWasPressedThisFrame)
				{
					_fsm.Fire(GameFSM.Triggers.Retry);
				}

				if (Utils.IsDevBuild())
				{
					var player = _state.Entities.Find((entity) => entity.ControlledByPlayer);
					_ui.GameplayText.text = @$"Progress: {player.AngerProgress}
	State: {player.AngerState}
	Calm Track Timestamp: {(_audioPlayer.MusicTimes.ContainsKey(_config.MusicCalmClip.GetInstanceID()) ? _audioPlayer.MusicTimes[_config.MusicCalmClip.GetInstanceID()] : 0)}
	Angry Track Timestamp: {(_audioPlayer.MusicTimes.ContainsKey(_config.MusicAngryClip.GetInstanceID()) ? _audioPlayer.MusicTimes[_config.MusicAngryClip.GetInstanceID()] : 0)}";
				}

				if (Utils.IsDevBuild())
				{
					if (Keyboard.current.f1Key.wasPressedThisFrame)
					{
						Victory();
					}

					if (Keyboard.current.f2Key.wasPressedThisFrame)
					{
						_fsm.Fire(GameFSM.Triggers.Retry);
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

			await _ui.FadeIn(Color.black);

			_ui.HideGameplay();

			foreach (var entity in _state.Entities)
			{
				Object.Destroy(entity.gameObject);
			}
			_state.Entities.Clear();
			_state.WalkableGrid = null;
			_state.TriggerExitAt = 0;
			_state.TriggerRetry = false;
			_state.PlayerDidAct = false;
			_audioPlayer.MusicTimes.Clear();
			if (_state.Level)
			{
				Object.Destroy(_state.Level.gameObject);
				_state.Level = null;
			}
		}

		private void ConfirmStarted(InputAction.CallbackContext context) => _confirmWasPressedThisFrame = true;

		private void CancelStarted(InputAction.CallbackContext context) => _cancelWasPressedThisFrame = true;

		private void ResetStarted(InputAction.CallbackContext context) => _resetWasPressedThisFrame = true;

		private void Victory()
		{
			_ = _audioPlayer.StopMusic();
			_fsm.Fire(GameFSM.Triggers.Won);
		}

		private async UniTask<bool> Turn(Entity entity, Vector3Int destination)
		{
			var destinationTile = _state.Level.Ground.GetTile(destination);
			var entitiesAtDestination = _state.Entities.FindAll(entity => entity.GridPosition == destination);
			if (entitiesAtDestination.Count > 1)
			{
				UnityEngine.Debug.LogError("We don't handle multiple entities in the same position right now. See Resources/Levels/README.md for more informations.");
			}

			var entityAtDestination = entitiesAtDestination.Count > 0 ? entitiesAtDestination[0] : null;

			if (destinationTile == null && entityAtDestination == null)
			{
				UnityEngine.Debug.Log($"Can't move to {destination} (tile is null).");
				return false;
			}

			if (destinationTile && _config.TileToInfo.TryGetValue(destinationTile, out var destinationTileInfo))
			{
				if (destinationTileInfo.Walkable == false)
				{
					UnityEngine.Debug.Log($"Can't move to {destination} (not walkable).");
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

				if (entityAtDestination.TriggerState != AngerStates.None && entity.AngerState.HasFlag(entityAtDestination.TriggerState) == false)
				{
					UnityEngine.Debug.Log($"Can't move to {destination} (wrong state).");
					return false;
				}

				if (entityAtDestination.CanBeActivated && entityAtDestination.Activated == false)
				{
					UnityEngine.Debug.Log($"Can't move to {destination} (entity not activated).");
					return false;
				}
			}

			entity.Direction = destination - entity.GridPosition;
			entity.Animator.SetFloat("DirectionX", entity.Direction.x);
			entity.Animator.SetFloat("DirectionY", entity.Direction.y);
			entity.GridPosition = destination;
			entity.Animator.Play("Walk");
			await DOTween.To(() => entity.transform.position, x => entity.transform.position = x, entity.GridPosition + cellOffset, 1 / entity.MoveSpeed);
			entity.Animator.Play("Idle");

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

							_state.CurrentLevelIndex += 1;
							_state.Running = false;

							_ = _audioPlayer.StopMusic(2);
							if (entityAtDestination.ExitAudioClip)
							{
								_ = _audioPlayer.PlaySoundEffect(entityAtDestination.ExitAudioClip);
							}
							await _ui.FadeIn(Color.black);
							await UniTask.Delay(4000);
							_fsm.Fire(GameFSM.Triggers.NextLevel);

							return true;
						}
						break;
					case TriggerActions.Break:
						{
							if (entity.AngerState != AngerStates.Angry)
							{
								break;
							}

							entityAtDestination.BreakableProgress += 1;

							if (entityAtDestination.BreakableProgress >= entityAtDestination.BreaksAt)
							{
								entityAtDestination.Breaking = true;
							}
						}
						break;
					case TriggerActions.Key:
						{
							if (entity.ControlledByPlayer)
							{
								entityAtDestination.Dead = true;
								_state.Keys += 1;
							}
						}
						break;
					default: break;
				}
			}

			return true;
		}

		private void ToggleMusic(Entity entity)
		{
			// UnityEngine.Debug.Log("Toggle audio");
			var calmId = _config.MusicCalmClip.GetInstanceID();
			var angryId = _config.MusicAngryClip.GetInstanceID();

			if (entity.AngerState == AngerStates.Calm)
			{
				_audioPlayer.MusicTimes[calmId] = _audioPlayer.MusicTimes.ContainsKey(angryId) ? _audioPlayer.MusicTimes[angryId] : 0;
				_ = _audioPlayer.PlayMusic(_config.MusicCalmClip, false, 1f);
			}
			else
			{
				_audioPlayer.MusicTimes[angryId] = _audioPlayer.MusicTimes.ContainsKey(calmId) ? _audioPlayer.MusicTimes[calmId] : 0;
				_ = _audioPlayer.PlayMusic(_config.MusicAngryClip, false, 1f);
			}
		}

		private bool IsTileWalkable(TileBase tile)
		{
			if (tile == null)
			{
				return false;
			}

			var hasInfo = _config.TileToInfo.TryGetValue(tile, out var destinationTileInfo);
			if (hasInfo && destinationTileInfo.Walkable == true)
			{
				return true;
			}

			return false;
		}

		private void SpawnEntitiesFromTilemap(Tilemap tilemap)
		{
			var bounds = tilemap.cellBounds;
			var tiles = tilemap.GetTilesBlock(bounds);
			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					var tile = tiles[x + y * bounds.size.x];
					var gridPosition = new Vector3Int(x, y, 0);

					if (tile == null)
					{
						continue;
					}

					if (_config.TileToEntity.ContainsKey(tile))
					{
						var entity = Object.Instantiate(
							_config.TileToEntity[tile],
							tilemap.transform
						);
						entity.GridPosition = bounds.min + gridPosition;
						entity.Direction = Vector3Int.down;
						_state.Entities.Add(entity);
						tilemap.SetTile(entity.GridPosition, null);

						var clips = entity.Animator.runtimeAnimatorController.animationClips;
						entity.AnimationClipLength = new ClipLength();
						foreach (var clip in clips)
						{
							if (entity.AnimationClipLength.ContainsKey(clip.name))
							{
								continue;
							}
							entity.AnimationClipLength.Add(clip.name, clip.length);
						}
					}
					else
					{
						UnityEngine.Debug.LogWarning("Missing entity for tile: " + tile.name);
					}
				}
			}
		}
	}
}
