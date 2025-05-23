using Tarabish.Controllers;
using UnityEngine;

namespace Tarabish.Editor {
	public class Prototyping : MonoBehaviour {
		[SerializeField] private bool _startTest = false;

		private void OnValidate() {
			if (_startTest) {
				_startTest = false;

				ControllerManager.Instance.RemoveController(new ControllerInfo("Test", ControllerInfo.ControllerType.PARTNER));
			}
		}
	}
}