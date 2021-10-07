using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Core.Editor
{
	public class DebugMenus
	{
		[MenuItem("Alteration/Merge level assets")]
		static void DoSomething()
		{
			// var path = Path.Combine(Application.persistentDataPath);
			// EditorUtility.RevealInFinder(path);

			UnityEngine.Debug.Log("Merging level assets.");

			var config = Resources.Load<GameConfig>("Game Config");

			for (int i = 0; i < config.AllLevels.Length; i++)
			{
				var level = config.AllLevels[i];

				var originalScreenshot = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Levels/{i + 1:D2}.png");
				var screenshot = new Texture2D(originalScreenshot.width, originalScreenshot.height, originalScreenshot.format, true);
				Graphics.CopyTexture(originalScreenshot, screenshot);
				screenshot.name = "Screenshot";
				if (level.Screenshot != null)
				{
					level.Screenshot = screenshot;
				}
				else
				{
					AssetDatabase.AddObjectToAsset(screenshot, level);
					level.Screenshot = screenshot;
					AssetDatabase.DeleteAsset($"Assets/Resources/Levels/{i + 1:D2}.png");
				}

				// var originalTrace = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Resources/Levels/{i + 1:D2}.inputtrace");
				// UnityEngine.Debug.Log(originalTrace + " " + $"Assets/Resources/Levels/{i + 1:D2}.inputtrace");
				// var trace = new TextAsset();
				// var stream = new MemoryStream(originalTrace.bytes);
				// var reader = new StreamReader(stream);
				// // trace.text = reader.ReadToEnd();

				// // File.WriteAllText(UnityEditor.AssetDatabase.GetAssetPath(trace), originalTrace.text);
				// // trace.bytes = originalTrace.bytes;
				// trace.name = "Input Trace";
				// if (level.InputTrace)
				// {
				// 	level.InputTrace = trace;
				// }
				// else
				// {
				// 	AssetDatabase.AddObjectToAsset(trace, level);
				// 	level.InputTrace = trace;
				// 	// UnityEditor.AssetDatabase.DeleteAsset("Assets/Resources/Levels/01.png");
				// }
			}

			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
		}
	}
}
