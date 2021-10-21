using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Core
{
	public class Save
	{
		private string _playerSettingsPath = Application.persistentDataPath + "/Settings.bin";
		private string _playerSaveDataPath = Application.persistentDataPath + "/Save0.bin";

		public PlayerSettings LoadPlayerSettings()
		{
			if (File.Exists(_playerSettingsPath))
			{
				UnityEngine.Debug.Log("Loading player settings: " + _playerSettingsPath);
				return BinaryFileSerializer.Deserialize<PlayerSettings>(_playerSettingsPath);
			}

			UnityEngine.Debug.Log("Loading player settings: DEFAULT");
			return new PlayerSettings
			{
				GameVolume = 1,
				SoundVolume = 1,
				MusicVolume = 1,
				FullScreen = Screen.fullScreen,
				ResolutionWidth = Screen.currentResolution.width,
				ResolutionHeight = Screen.currentResolution.height,
				ResolutionRefreshRate = Screen.currentResolution.refreshRate,
			};
		}

		public void SavePlayerSettings(PlayerSettings data)
		{
			BinaryFileSerializer.Serialize(data, _playerSettingsPath);
			UnityEngine.Debug.Log("Saving player settings: " + _playerSettingsPath);
		}

		public PlayerSaveData LoadPlayerSaveData()
		{
			if (File.Exists(_playerSaveDataPath))
			{
				UnityEngine.Debug.Log("Loading player data: " + _playerSaveDataPath);
				return BinaryFileSerializer.Deserialize<PlayerSaveData>(_playerSaveDataPath);
			}

			UnityEngine.Debug.Log("Loading player data: DEFAULT");
			return new PlayerSaveData
			{
				ClearedLevels = new HashSet<int>(),
			};
		}

		public void SavePlayerSaveData(PlayerSaveData data)
		{
			BinaryFileSerializer.Serialize(data, _playerSaveDataPath);
			UnityEngine.Debug.Log("Saving player data: " + _playerSaveDataPath);
		}
	}
}
