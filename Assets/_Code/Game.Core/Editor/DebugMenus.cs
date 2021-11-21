#if UNITY_EDITOR
using System.Linq;
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

			for (int i = 0; i < config.Levels.Length; i++)
			{
				var levelAsset = config.Levels[i];
				var levelPath = AssetDatabase.GetAssetPath(levelAsset);

				var screenshotPath = levelPath.Replace($"{levelAsset.name}.prefab", $"{Utils.GetLevelIndex(i)}.png");
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

		[MenuItem("Alteration/Force reserialize levels")]
		static void ForceReserializeLevels()
		{
			// var paths = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Resources/Levels" });
			var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/Resources/Levels") && p.EndsWith(".prefab"));
			foreach (var item in paths)
				UnityEngine.Debug.Log(item);

			AssetDatabase.ForceReserializeAssets(paths);
		}
	}
}
#endif
