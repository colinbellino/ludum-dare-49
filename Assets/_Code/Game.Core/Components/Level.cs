using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	public class Level : MonoBehaviour
	{
		public string Title;
		public Tilemap Ground;
		public Tilemap Entities;
		public bool MoodChanges = true;
		[UnityEngine.Serialization.FormerlySerializedAs("AngerMax")]
		[Range(3, 10)]
		public int MoodMax = 3;
		public Moods DefaultPlayerMood = Moods.Calm;

		[SerializeField] public Texture2D Screenshot;
		// [HideInInspector] [SerializeField] public InputTrace InputTrace;
	}
}
