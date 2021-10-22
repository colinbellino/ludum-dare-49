using Game.Core.StateMachines.Game;
using Game.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
	public class GameSingleton
	{
		public GameConfig Config;
		public GameUI UI;
		public PauseUI PauseUI;
		public OptionsUI OptionsUI;
		public CameraRig CameraRig;
		public GameControls Controls;
		public GameState State;
		public GameFSM GameFSM;
		public Save Save;
		public InputRecorder InputRecorder;
		public GameObject LevelWalls;
	}
}
