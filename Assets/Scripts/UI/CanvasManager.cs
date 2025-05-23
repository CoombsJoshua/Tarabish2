using System.Collections.Generic;
using Tarabish.Extensions;
using UnityEngine;

namespace Tarabish.UI {
	public class CanvasManager : Singleton<CanvasManager>
	{
		// public static CanvasManager Instance;

		// private void Awake(){
		// 	Instance = this;
		// }
		
		[SerializeField] Menu.MenuType m_ActiveCanvasType = Menu.MenuType.None;
		//CanvasController lastActiveCanvas;

		List<Menu.Menu_> canvasControllerList = new List<Menu.Menu_>();

		private void OnValidate()
		{
			GetComponentsInChildren(true, canvasControllerList);

			foreach (Menu.Menu_ cc in canvasControllerList)
				cc.gameObject.SetActive(cc.MenuType == m_ActiveCanvasType);
		}

		private void Start()
		{
			GetComponentsInChildren<Menu.Menu_>(true, canvasControllerList);
			// canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
		}

		public void SwitchCanvas(Menu.MenuType _type)
		{

			m_ActiveCanvasType = _type;

			foreach (Menu.Menu_ canvasController in canvasControllerList)
				canvasController.gameObject.SetActive(canvasController.MenuType == _type);
		}

		//? For ui interactions to call to
		public void SwitchCanvas(string _type) {
			SwitchCanvas((Menu.MenuType)System.Enum.Parse(typeof(Menu.MenuType), _type));
		}

		public Menu.MenuType activeCanvasType { get { return m_ActiveCanvasType; } }
	}
}
