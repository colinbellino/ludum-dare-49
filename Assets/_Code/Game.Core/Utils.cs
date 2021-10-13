using System;
using Cysharp.Threading.Tasks;
using Game.Inputs;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	public static class Utils
	{
		public static bool IsDevBuild()
		{
#if UNITY_EDITOR
			return true;
#endif

			if (Debug.isDebugBuild)
			{
				return true;
			}

#pragma warning disable 162
			return false;
#pragma warning restore 162
		}

		public static float GetMusicVolume(GameState state)
		{
			if (state.MusicMuted)
			{
				return 0;
			}
			return state.MusicVolume;
		}
	}
}
