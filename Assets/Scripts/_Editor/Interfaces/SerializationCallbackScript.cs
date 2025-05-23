#if UNITY_EDITOR

using UnityEngine;

namespace Tarabish.Editor.Interfaces {
	public class SerializationCallbackScript : ISerializationCallbackReceiver {
		public void OnAfterDeserialize() {
			// CardManager.Instance.UpdateCardsInScene();
			throw new System.NotImplementedException();
		}

		public void OnBeforeSerialize() {
			throw new System.NotImplementedException();
		}
	}
}

#endif