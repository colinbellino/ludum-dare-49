using Cysharp.Threading.Tasks;
using NesScripts.Controls.PathFind;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core.StateMachines.Game
{
	public class GameLoadLevelState : BaseGameState
	{
		public GameLoadLevelState(GameFSM fsm, GameSingleton game) : base(fsm, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			// Load current level
			if (_state.CurrentLevelIndex <= _state.AllLevels.Length - 1)
			{
				_state.Level = Object.Instantiate(_state.AllLevels[_state.CurrentLevelIndex]);
				_ = _ui.ShowLevelName($"{_state.CurrentLevelIndex + 1:D2} - {_state.Level.Title}");

				// Generate grid for walkable tiles
				var tilemap = _state.Level.Ground;
				var walkableTiles = new bool[tilemap.size.x, tilemap.size.y];
				for (var x = 0; x < tilemap.size.x - 0; x++)
				{
					for (var y = 0; y < tilemap.size.y - 0; y++)
					{
						var position = new Vector3Int(x, y, 0);
						var tile = tilemap.GetTile(position) as Tile;
						walkableTiles[x, y] = tile && GameGameplayState.IsTileWalkable(tile);
					}
				}
				_state.WalkableGrid = new GridData(walkableTiles);

				// Spawn entities
				SpawnEntitiesFromTilemap(_state.Level.Ground);
				SpawnEntitiesFromTilemap(_state.Level.Entities);
			}

			await UniTask.NextFrame();

			_fsm.Fire(GameFSM.Triggers.Done);
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

						if (entity.ClearTileAfterConvert)
						{
							tilemap.SetTile(entity.GridPosition, null);
						}

						entity.AnimationClipLength = new ClipLength();
						var clips = entity.Animator.runtimeAnimatorController.animationClips;
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
						// UnityEngine.Debug.LogWarning("Missing entity for tile: " + tile.name);
					}
				}
			}
		}
	}
}
