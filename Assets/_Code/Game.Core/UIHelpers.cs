using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core
{
	public class UIHelpers : MonoBehaviour
	{
		public static void PlayButtonClip()
		{
			FMODUnity.RuntimeManager.PlayOneShot(GameManager.Game.Config.SoundMenuConfirm);
		}

		public void SetSelectedGameObject()
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}
}