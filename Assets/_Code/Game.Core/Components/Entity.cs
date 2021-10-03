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

		[HideInInspector] public Vector3Int GridPosition;
		[HideInInspector] public Vector3Int Direction;

		[Header("Movement")]
		[SerializeField] public float MoveSpeed = 4f;

		[Header("Anger")]
		[SerializeField] public bool AffectedByAnger;
		[HideInInspector] public int AngerProgress;
		[SerializeField] public AngerStates AngerState;

		[Header("Triggers")]
		[SerializeField] public bool Trigger;
		[SerializeField] public AngerStates TriggerState;
		[SerializeField] public TriggerActions TriggerAction;

		[Header("Break")]
		[SerializeField] public int BreaksAt = 1;
		[SerializeField] public ParticleSystem BreakParticle;
		[SerializeField] public Vector3 BreakParticleOffset;
		[HideInInspector] public int BreakableProgress;

		[Header("Activation")]
		[SerializeField] public bool CanBeActivated;
		[SerializeField] public bool ActivatesWhenKeyInLevel;
		[SerializeField] public int ActivatesWithKeys;
		[HideInInspector] public bool Activated;

		[Header("Audio")]
		[SerializeField] public AudioClip CantMoveAudioClip;
		[SerializeField] public AudioClip FallAudioClip;
		[SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("WalkAudioClips")] public AudioClip[] BreakGroundAudioClips;
		[SerializeField] public AudioClip TransformationAudioClip;
		[SerializeField] public AudioClip KeyAudioClip;
		[SerializeField] public AudioClip ExitAudioClip;
		[SerializeField] public AudioClip BreakingAudioClip;
		[SerializeField] public AudioClip[] WalkCalmAudioClips;
		[SerializeField] public AudioClip[] WalkAngryAudioClips;

		[HideInInspector] public bool Dead;

		[HideInInspector] public ClipLength AnimationClipLength;
	}

	[Serializable]
	public class ClipLength : SerializableDictionary<string, float> { }

	public enum AngerStates { None, Calm, Angry }

	public enum TriggerActions { None, Exit, Break, Key, Fall }
}
