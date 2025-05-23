using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Tarabish.Netcode;

namespace Tarabish.UI.Menu {
	public class Menu_Splash : Menu_
	{
		public override MenuType MenuType => MenuType.Splash;
	
		public TextMeshProUGUI m_UsernameText;
		public Button m_GuestBtn;
	
		void Start(){
			m_GuestBtn.onClick.AddListener(async () => await SessionManager.Instance.SignInAnonymouslyAsync());
		}
	}
}

