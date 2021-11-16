using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	public class Level : MonoBehaviour
	{
		public string Title;
		public Tilemap Ground;
		public Tilemap Entities;
		public int AngerMax = 3;

		[SerializeField] public Texture2D Screenshot;
		// [HideInInspector] [SerializeField] public InputTrace InputTrace;
	}
}
