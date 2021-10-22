using Cysharp.Threading.Tasks;
using Game.Core.StateMachines.Game;
using Game.Inputs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Game.Core
{
	public class GameManager : MonoBehaviour
	{
		public static GameSingleton Game { get; private set; }

		private async UniTask Start()
		{
			var config = Resources.Load<GameConfig>("Game Config");
			var camera = Camera.main;
			var cameraRig = FindObjectOfType<CameraRig>();
			var ui = FindObjectOfType<GameUI>();
			var pauseUI = FindObjectOfType<PauseUI>();
			var optionsUI = FindObjectOfType<OptionsUI>();
			var inputRecorder = FindObjectOfType<InputRecorder>();
			var levelWalls = GameObject.Find("Level Walls");

			Assert.IsNotNull(config, "Could not find the Game Config file.");
			Assert.IsNotNull(camera, "Could not find the Camera in the scene.");
			Assert.IsNotNull(cameraRig, "Could not find the CameraRig in the scene.");
			Assert.IsNotNull(ui, "Could not find the GameUI in the scene.");
			Assert.IsNotNull(pauseUI, "Could not find the PauseUI in the scene.");
			Assert.IsNotNull(optionsUI, "Could not find the OptionsUI in the scene.");
			Assert.IsNotNull(inputRecorder, "Could not find the InputRecorder in the scene.");
			Assert.IsNotNull(levelWalls, "Could not find the LevelWalls in the scene.");

			Game = new GameSingleton();
			Game.Config = config;
			Game.Controls = new GameControls();
			Game.CameraRig = cameraRig;
			Game.UI = ui;
			Game.PauseUI = pauseUI;
			Game.OptionsUI = optionsUI;
			Game.State = new GameState();
			Game.GameFSM = new GameFSM(config.DebugFSM, Game);
			Game.Save = new Save();
			Game.InputRecorder = inputRecorder;
			Game.LevelWalls = levelWalls;

			await Game.GameFSM.Start();
		}

		private void Update()
		{
			Time.timeScale = Game.State.TimeScaleCurrent;
			Game?.GameFSM.Tick();
		}
	}
}
