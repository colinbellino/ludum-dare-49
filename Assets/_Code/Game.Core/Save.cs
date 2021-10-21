using System.IO;
using UnityEngine;

namespace Game.Core
{
	public class Save
	{
		private string _playerSettingsPath = Application.persistentDataPath + "/PlayerSettings.bin";

		public void SavePlayerSettings(PlayerSettings data)
		{
			BinaryFileSerializer.Serialize(data, _playerSettingsPath);
			UnityEngine.Debug.Log("Saving player settings: " + _playerSettingsPath);
		}

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
	}
}
