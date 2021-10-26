using System;
using Game.Core;

namespace UnityEngine.Localization.Settings
{
	/// <summary>
	/// Uses PlayerSettings.LocaleCode to keep track of the last used locale.
	/// Whenever the locale is changed, the new Locale is recorded in PlayerSettings.LocaleCode.
	/// </summary>
	[Serializable]
	public class PlayerSettingsLocaleSelector : IStartupLocaleSelector, IInitialize
	{
		public void PostInitialization(LocalizationSettings settings)
		{
			if (Application.isPlaying)
			{
				var selectedLocale = settings.GetSelectedLocale();
				if (selectedLocale != null)
					GameManager.Game.State.PlayerSettings.LocaleCode = selectedLocale.Identifier.Code;
			}
		}

		public Locale GetStartupLocale(ILocalesProvider availableLocales)
		{
			var code = GameManager.Game.State.PlayerSettings.LocaleCode;
			if (string.IsNullOrEmpty(code) == false)
			{
				return availableLocales.GetLocale(code);
			}

			return null;
		}
	}
}
