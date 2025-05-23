using Tarabish.UI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Tarabish.UI.Menu {
	[System.Serializable] public class ElementReferences {
		public InputField_Default ScoreToWin;
	}

	public class Menu_Settings : Menu_ {
		public override MenuType MenuType => MenuType.Settings;

		public ElementReferences Elements;
	}
}
