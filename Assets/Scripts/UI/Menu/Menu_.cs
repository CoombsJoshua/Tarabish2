using UnityEngine;

namespace Tarabish.UI.Menu {
	public enum MenuType {
		None = 0,
	
		Splash,
		Title,
		CreateAccount,
		Playing,
		Profile,
		Settings,
		Credits,
		EndOfGame
	}
	public abstract class Menu_ : MonoBehaviour {
		public abstract MenuType MenuType {get;}
	}
}