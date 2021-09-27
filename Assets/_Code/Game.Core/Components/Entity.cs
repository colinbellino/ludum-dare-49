using Prime31;
using UnityEngine;

namespace Game.Core
{
	public class Entity : MonoBehaviour
	{
		[SerializeField] public Rigidbody2D Rigidbody;
		[SerializeField] public CharacterController2D Controller;
		[SerializeField] public Animator Animator;
		[SerializeField] public SpriteRenderer SpriteRenderer;
		[SerializeField] public AudioSource AudioSource;
	}
}
