#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Core.Editor
{
	public class DebugMenus
	{
		[MenuItem("Alteration/Associate screenshots to levels")]
		static void AssociateScreenshots()
		{
			// var path = Path.Combine(Application.persistentDataPath);
			// EditorUtility.RevealInFinder(path);

			UnityEngine.Debug.Log("Associating screenshots to levels.");

			var config = Resources.Load<GameConfig>("Game Config");

			for (int i = 0; i < config.AllLevels.Length; i++)
			{
				var levelAsset = config.AllLevels[i];
				var levelPath = AssetDatabase.GetAssetPath(levelAsset);

				var screenshotPath = levelPath.Replace($"{levelAsset.name}.prefab", $"{i + 1:D2}.png");
				var originalScreenshot = AssetDatabase.LoadAssetAtPath<Texture2D>(screenshotPath);
				if (originalScreenshot)
				{
					levelAsset.Screenshot = originalScreenshot;
				}

				PrefabUtility.SavePrefabAsset(levelAsset.gameObject);
			}

			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
		}

		[MenuItem("Alteration/Associate screenshots to levels", true)]
		static bool ValidateAssociateScreenshots()
		{
			return Application.isPlaying == false;
		}
	}
}
#endif
