using Prime31;
using UnityEngine;

namespace Game.Core
{
	public class Entity : MonoBehaviour
	{
		[SerializeField] public Animator Animator;
		[SerializeField] public SpriteRenderer SpriteRenderer;
		[SerializeField] public AudioSource AudioSource;

		[SerializeField] public bool ControlledByPlayer;

		[HideInInspector] public Vector3Int GridPosition;

		[SerializeField] public bool AffectedByAnger;
		[HideInInspector] public int AngerProgress;
		[HideInInspector] public AngerStates AngerState;
	}

	public enum AngerStates { Calm, Angry }
}
