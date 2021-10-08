using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	public class Level : MonoBehaviour
	{
		public string Title;
		public Tilemap Ground;
		public Tilemap Entities;

		[SerializeField] public Texture2D Screenshot;
		// [HideInInspector] [SerializeField] public InputTrace InputTrace;
	}
}
