using Cysharp.Threading.Tasks;
using NesScripts.Controls.PathFind;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Game.Core.StateMachines.Game
{
	public class GameGameplayState : BaseGameState
	{
		private bool _confirmWasPressedThisFrame;
		private bool _cancelWasPressedThisFrame;
		private bool _resetWasPressedThisFrame;

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
					UnityEngine.Debug.Log("Last level reached!");
					_fsm.Fire(GameFSM.Triggers.Won);
					return;
				}

				_state.Level = Object.Instantiate(_config.AllLevels[_state.CurrentLevelIndex]);
				UnityEngine.Debug.Log("Loaded level: " + _state.Level.name);

				Assert.AreEqual(_state.Level.Ground.origin, Vector3Int.zero,
					"Make sure the tilemap's origin is at (0,0,0).");

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

			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)UnityEngine.Random.Range(0, int.MaxValue));

			{
				var player = _state.Entities.Find((entity) => entity.ControlledByPlayer);
				var moodClip = player.AngerState == AngerStates.Calm ? _config.MusicCalmClip : _config.MusicAngryClip;
				if (_audioPlayer.IsMusicPlaying() == false && _audioPlayer.IsCurrentMusic(moodClip) == false)
				{
					_ = _audioPlayer.PlayMusic(moodClip, true, 0.5f);
				}
				else
				{
					if (_audioPlayer.IsCurrentMusic(moodClip) == false)
					{
						SetMusic(player);
					}
				}
			}

			_ = _ui.FadeOut();

			_state.Running = true;

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;
			_controls.Gameplay.Cancel.started += CancelStarted;
			_controls.Gameplay.Reset.started += ResetStarted;
			_controls.Global.Enable();

			while (true)
			{
				await UniTask.NextFrame();

				if (_state.Running == false)
				{
					continue;
				}

				if (_state.PlayerDidAct)
				{
					continue;
				}

				var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
				if (moveInput.magnitude == 0f)
				{
					continue;
				}

				var player = _state.Entities.Find((entity) => entity.ControlledByPlayer);

				var destination = player.GridPosition + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);
				var playerDidMove = MoveTo(player, destination);
				if (playerDidMove)
				{
					_state.PlayerDidAct = true;

					await UniTask.Delay(200);

					foreach (var entity in _state.Entities)
					{
						if (entity.MoveTowardsPlayer)
						{
							var path = Pathfinding.FindPath(
								_state.WalkableGrid,
								entity.GridPosition, player.GridPosition,
								Pathfinding.DistanceType.Manhattan
							);
							MoveTo(entity, path[0]);
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
								entityAtPosition.Animator.Play("Fall");
								entityAtPosition.Moving = false;

								await UniTask.Delay(500);

								if (entityAtPosition.ControlledByPlayer)
								{
									_state.TriggerRetryAt = Time.time + 0.5f;
								}
							}
						}
					}

					_state.PlayerDidAct = false;
				}
			}
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
						_state.Entities.Add(entity);
						tilemap.SetTile(entity.GridPosition, null);
					}
				}
			}
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

			var cellOffset = new Vector3(0.5f, 0.5f);
			foreach (var entity in _state.Entities)
			{
				if (entity.Placed == false)
				{
					entity.transform.position = entity.GridPosition + cellOffset;
					entity.Placed = true;
				}

				if (entity.Moving)
				{
					if (entity.MoveT == 0)
					{
						entity.Animator.Play("Walk");
					}

					entity.MoveT += Time.deltaTime * entity.MoveSpeed;
					entity.transform.position = Vector3.Lerp(entity.transform.position, entity.GridPosition + cellOffset, entity.MoveT);

					if (entity.MoveT >= 1)
					{
						entity.Animator.Play("Idle");
						entity.MoveT = 0;
						entity.Moving = false;
					}
				}

				if (entity.AffectedByAnger)
				{
					entity.SpriteRenderer.color = (entity.AngerState == AngerStates.Calm) ? new Color(0.5f, 0.5f, 1, 1) : Color.white;
				}

				if (entity.BreakableProgress > 0)
				{
					entity.Animator.Play("Damaged");
				}
			}

			if (_resetWasPressedThisFrame)
			{
				_state.TriggerRetryAt = Time.time;
			}

			if (_state.TriggerExitAt > 0 && Time.time >= _state.TriggerExitAt)
			{
				_state.TriggerExitAt = 0;
				_state.CurrentLevelIndex += 1;
				_state.Running = false;
				_fsm.Fire(GameFSM.Triggers.NextLevel);
			}

			if (_state.TriggerRetryAt > 0 && Time.time >= _state.TriggerRetryAt)
			{
				_state.TriggerRetryAt = 0;
				_state.Running = false;
				// _ = _audioPlayer.StopMusic();
				_fsm.Fire(GameFSM.Triggers.Retry);
			}

			var player = _state.Entities.Find((entity) => entity.ControlledByPlayer);

			if (Utils.IsDevBuild())
			{
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
					Defeat();
				}
			}

			_confirmWasPressedThisFrame = false;
			_cancelWasPressedThisFrame = false;
			_resetWasPressedThisFrame = false;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_state.Running = false;

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.started -= ConfirmStarted;
			_controls.Gameplay.Cancel.started -= CancelStarted;
			_controls.Gameplay.Reset.started -= ResetStarted;
			_controls.Global.Disable();

			await _ui.FadeIn(Color.white);

			_ui.HideGameplay();

			foreach (var entity in _state.Entities)
			{
				Object.Destroy(entity.gameObject);
			}
			_state.Entities.Clear();
			_state.WalkableGrid = null;
			_state.TriggerExitAt = 0;
			_state.TriggerRetryAt = 0;
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

		private void Victory()
		{
			_ = _audioPlayer.StopMusic();
			_fsm.Fire(GameFSM.Triggers.Won);
		}

		private async void Defeat()
		{
			if (_config.PlayerDeathClip)
			{
				_ = _audioPlayer.PlaySoundEffect(_config.PlayerDeathClip);
			}

			_state.Running = false;

			await UniTask.Delay(1000);

			_state.Running = true;

			_fsm.Fire(GameFSM.Triggers.Lost);
		}

		private bool MoveTo(Entity entity, Vector3Int destination)
		{
			var destinationTile = _state.Level.Ground.GetTile(destination);
			var entityAtDestination = _state.Entities.Find(entity => entity.GridPosition == destination);

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

				switch (entityAtDestination.TriggerAction)
				{
					case TriggerActions.Exit:
						{
							if (entity.ControlledByPlayer)
							{
								if (entityAtDestination.ExitAudioClip)
								{
									_audioPlayer.PlaySoundEffect(entityAtDestination.ExitAudioClip);
								}

								_state.TriggerExitAt = Time.time + 1f;
							}
						}
						break;
					case TriggerActions.Break:
						{
							entityAtDestination.BreakableProgress += 1;

							if (entityAtDestination.BreakableProgress >= entityAtDestination.BreaksAt)
							{
								entityAtDestination.Breaking = true;
							}
						}
						break;
					default: break;
				}
			}


			// UnityEngine.Debug.Log("Entity moved.");
			entity.Direction = destination - entity.GridPosition;
			entity.GridPosition = destination;
			entity.MoveStartTimestamp = Time.time;
			entity.MoveT = 0;
			entity.Moving = true;

			if (_state.TriggerExitAt == 0)
			{
				if (entity.AffectedByAnger)
				{
					entity.AngerProgress += 1;
					if (entity.AngerProgress >= 3)
					{
						entity.AngerProgress = 0;
						entity.AngerState = (entity.AngerState == AngerStates.Calm) ? AngerStates.Angry : AngerStates.Calm;

						if (entity.ControlledByPlayer)
						{
							SetMusic(entity);
						}
					}
				}
			}

			if (entity.Animator)
			{
				entity.Animator.SetFloat("DirectionX", entity.Direction.x);
				entity.Animator.SetFloat("DirectionY", entity.Direction.y);
			}

			return true;
		}

		private void SetMusic(Entity entity)
		{
			UnityEngine.Debug.Log("Toggle audio");
			var calmId = _config.MusicCalmClip.GetInstanceID();
			var angryId = _config.MusicAngryClip.GetInstanceID();

			if (entity.AngerState == AngerStates.Calm)
			{
				_audioPlayer.MusicTimes[calmId] = _audioPlayer.MusicTimes.ContainsKey(angryId) ? _audioPlayer.MusicTimes[angryId] : 0;
				_ = _audioPlayer.PlayMusic(_config.MusicCalmClip, false, 0);
			}
			else
			{
				_audioPlayer.MusicTimes[angryId] = _audioPlayer.MusicTimes.ContainsKey(calmId) ? _audioPlayer.MusicTimes[calmId] : 0;
				_ = _audioPlayer.PlayMusic(_config.MusicAngryClip, false, 0);
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
	}
}
