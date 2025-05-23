using Unity.Services.Authentication;
using UnityEngine;

namespace Tarabish.Controllers {
	[System.Serializable] public class ControllerInfo {
		public enum ControllerType {
			CLIENT, 	//? The Local Client
			PARTNER, 	//? The Controller that is the partner of the client
			OPONENT, 	//? The Controller that is an oponent of the client
		}

		public string Name = "Placeholder";
		public ControllerType Type;
		public bool IsAI = false;

		public ControllerInfo(string name, ControllerType type) {
			Name = name;
			Type = type;
		}
	}
}