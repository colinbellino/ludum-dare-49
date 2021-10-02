using System;
using UnityEngine;

namespace Game.Core
{
	public class Entity : MonoBehaviour
	{
		[SerializeField] public Animator Animator;
		[SerializeField] public SpriteRenderer SpriteRenderer;
		[SerializeField] public AudioSource AudioSource;

		[Header("AI")]
		[SerializeField] public bool ControlledByPlayer;
		[SerializeField] public bool MoveTowardsPlayer;

		[HideInInspector] public bool Placed;
		[HideInInspector] public Vector3Int GridPosition;

		[Header("Movement")]
		[HideInInspector] public bool Moving;
		[SerializeField] public float MoveSpeed = 4f;
		[HideInInspector] public float MoveT;
		[HideInInspector] public float MoveStartTimestamp;

		[Header("Anger")]
		[SerializeField] public bool AffectedByAnger;
		[HideInInspector] public int AngerProgress;
		[SerializeField] public AngerStates AngerState;

		[Header("Triggers")]
		[SerializeField] public bool Trigger;
		[SerializeField] public AngerStates TriggerState;
		[SerializeField] public TriggerActions TriggerAction;
	}

	[Flags] public enum AngerStates { None, Calm, Angry }

	public enum TriggerActions { None, Exit }
}
