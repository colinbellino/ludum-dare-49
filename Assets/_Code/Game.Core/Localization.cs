using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedObjects;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

namespace Game.Core
{
	public static class Localization
	{
		public static string TableReference = "AllText";

		public static string GetLocalizedString(string key)
		{
			var localized = new LocalizedString(TableReference, key);
			return localized.GetLocalizedString();
		}

		public static void SetTMPTextKey(GameObject gameObject, string key)
		{
			var localizer = gameObject.GetComponentInChildren<GameObjectLocalizer>();
			var trackedText = localizer.GetTrackedObject<TrackedUGuiGraphic>(gameObject.GetComponentInChildren<TMP_Text>());
			var textVariant = trackedText.GetTrackedProperty<LocalizedStringProperty>("m_text");
			textVariant.LocalizedString.SetReference(TableReference, key);
		}
	}

}
