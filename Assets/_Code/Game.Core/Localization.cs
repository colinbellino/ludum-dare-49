using UnityEngine.Localization;

namespace Game.Core
{
	public static class Localization
	{
		private static string _tableReference = "AllText";

		public static string GetLocalizedString(string key)
		{
			var localized = new LocalizedString(_tableReference, key);
			return localized.GetLocalizedString();
		}
	}
}
