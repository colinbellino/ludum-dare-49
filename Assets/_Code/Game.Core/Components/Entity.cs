using System;
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
		[SerializeField] public AngerStates AngerState;

		[SerializeField] public bool Trigger;
		[SerializeField] public AngerStates TriggerState;
		[SerializeField] public TriggerActions TriggerAction;

		[SerializeField] public bool MoveTowardsPlayer;
	}

	[Flags] public enum AngerStates { None, Calm, Angry }

	public enum TriggerActions { None, Exit }
}
