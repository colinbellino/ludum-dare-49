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
				_state.Level = Object.Instantiate(_config.AllLevels[_state.CurrentLevelIndex]);

				Assert.AreNotEqual(_state.Level.Ground.origin, Vector3Int.zero,
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
				{
					var bounds = _state.Level.Entities.cellBounds;
					var tiles = _state.Level.Entities.GetTilesBlock(bounds);
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
									_state.Level.Entities.transform
								);
								entity.GridPosition = bounds.min + gridPosition;
								_state.Entities.Add(entity);
								_state.Level.Entities.DeleteCells(gridPosition, new Vector3Int(1, 1, 1));
							}
						}
					}
				}
			}

			_state.Random = new Unity.Mathematics.Random();
			_state.Random.InitState((uint)UnityEngine.Random.Range(0, int.MaxValue));

			if (_config.Music1Clip && _audioPlayer.IsMusicPlaying() == false && _audioPlayer.IsCurrentMusic(_config.Music1Clip) == false)
			{
				_ = _audioPlayer.PlayMusic(_config.Music1Clip, true, 0.5f);
			}

			_ = _ui.FadeOut();

			_state.Running = true;

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;
			_controls.Gameplay.Cancel.started += CancelStarted;
			_controls.Global.Enable();

			while (true)
			{
				await UniTask.NextFrame();

				if (_state.Running == false)
				{
					continue;
				}

				var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
				if (moveInput.magnitude == 0f)
				{
					continue;
				}

				foreach (var entity in _state.Entities)
				{
					if (entity.ControlledByPlayer)
					{
						if (_state.PlayerDidAct)
						{
							continue;
						}

						var destination = entity.GridPosition + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);
						if (MoveTo(entity, destination))
						{
							_state.PlayerDidAct = true;
							await UniTask.Delay(200);
							_state.PlayerDidAct = false;
						}
					}
					else
					{
						var target = _state.Entities[0];
						var path = Pathfinding.FindPath(_state.WalkableGrid, entity.GridPosition, target.GridPosition, Pathfinding.DistanceType.Manhattan);

						var destination = path[0];
						if (MoveTo(entity, destination))
						{
							// _state.PlayerDidAct = true;
							// await UniTask.Delay(200);
							// _state.PlayerDidAct = false;
						}
					}
				}
			}
		}

		public override void Tick()
		{
			base.Tick();

			var cellOffset = new Vector3(0.5f, 0.5f);
			foreach (var entity in _state.Entities)
			{
				// TODO: LERP this, probably
				entity.transform.position = entity.GridPosition + cellOffset;
			}

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

			// var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();

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

			foreach (var entity in _state.Entities)
			{
				GameObject.Destroy(entity.gameObject);
			}
			_state.Entities.Clear();
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
			if (destinationTile == null)
			{
				UnityEngine.Debug.Log($"Can't move there ({destination}).");
				return false;
			}

			if (_config.TileToInfo.TryGetValue(destinationTile, out var destinationTileInfo))
			{
				if (destinationTileInfo.CanWalk == false)
				{
					UnityEngine.Debug.Log($"Can't move there ({destination}).");
					return false;
				}
			}

			// UnityEngine.Debug.Log("Entity moved.");
			entity.GridPosition = destination;
			return true;
		}

		private bool IsTileWalkable(TileBase tile)
		{
			if (tile == null)
			{
				return false;
			}

			var hasInfo = _config.TileToInfo.TryGetValue(tile, out var destinationTileInfo);
			if (hasInfo && destinationTileInfo.CanWalk == true)
			{
				return true;
			}

			return false;
		}
	}
}
