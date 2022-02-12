using System;
using FMOD.Studio;
using FMODUnity;
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

		[Header("Authoring")]
		[SerializeField] public bool ClearTileAfterConvert = true;

		[HideInInspector] public Vector3Int GridPosition;
		[HideInInspector] public Vector3Int Direction;

		[Header("Movement")]
		[SerializeField] public float MoveSpeed = 4f;

		[Header("Anger")]
		[SerializeField] public bool AffectedByAnger;
		[SerializeField] public bool SyncToPlayerAnger;
		[UnityEngine.Serialization.FormerlySerializedAs("MoodCurrent")]
		[HideInInspector] public int MoodValue;
		[SerializeField] public int MoodMax = 2;
		[UnityEngine.Serialization.FormerlySerializedAs("AngerState")]
		[SerializeField] public Moods Mood;

		[Header("Triggers")]
		[SerializeField] public bool Trigger;
		[SerializeField] public Moods TriggerState;
		[SerializeField] public TriggerActions TriggerAction;
		[SerializeField] public int IncreaseMood = 2;
		[HideInInspector] public bool PreventMoodChangeThisFrame;

		[Header("Break")]
		[SerializeField] public int BreaksAt = 1;
		[SerializeField] public ParticleSystem BreakParticle;
		[SerializeField] public Vector3 BreakParticleOffset;
		[HideInInspector] public int BreakableProgress;

		[Header("Activation")]
		[SerializeField] public bool HasActiveAnimation;
		[SerializeField] public bool CanBeActivated;
		[SerializeField] public bool ActivatesInSpecificAngerState;
		[SerializeField] public bool ActivatesWhenKeyInLevel;
		[SerializeField] public bool ActivatesWhenLevelStart;
		[HideInInspector] public bool Activated;

		[Header("Audio")]
		[SerializeField] public EventReference SoundCantMoveAudio;
		[SerializeField] public EventReference SoundBurn;
		[SerializeField] public EventReference SoundFall;
		[SerializeField] public EventReference SoundBreakGround;
		[SerializeField] public EventReference SoundTransformation;
		[SerializeField] public EventReference SoundKey;
		[SerializeField] public EventReference SoundExit;
		[SerializeField] public EventReference SoundBreaking;
		[SerializeField] public EventReference[] SoundWalkCalm;
		[SerializeField] public EventReference[] SoundWalkAngry;
		[SerializeField] public EventReference SoundIncreaseMood;

		[HideInInspector] public bool Dead;
		[HideInInspector] public ClipLength AnimationClipLength;
	}

	[Serializable]
	public class ClipLength : SerializableDictionary<string, float> { }

	public enum Moods { None, Calm, Angry }

	public enum TriggerActions { None, Exit, Break, Key, Fall, Burn, ActivateBurn, Push, IncreaseMood }
}
