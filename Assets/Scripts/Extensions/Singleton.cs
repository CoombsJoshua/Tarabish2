//* Script source: https://chatgpt.com/share/77413061-1c97-45e4-b55d-b4b5ac23032e

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tarabish.Extensions {
	public class Singleton<T> : MonoBehaviour where T : Component {
		private static T instance;
		public static T Instance {
			get {
				if (instance == null) {
					instance = FindFirstObjectByType<T>();
					if (instance == null) {
						// Ensure this works in both runtime and editor
						#if UNITY_EDITOR
						instance = CreateAndRegisterInstance();
						#else
						GameObject obj = new GameObject();
						obj.name = typeof(T).Name;
						instance = obj.AddComponent<T>();
						#endif
					}
				}
				return instance;
			}
		}
	
		protected virtual void Awake() {
			if (instance == null) {
				instance = this as T;
				DontDestroyOnLoad(gameObject);
			}
			else if (instance != this) {
				Destroy(gameObject);
			}
		}

		#if UNITY_EDITOR
		private static T CreateAndRegisterInstance() {
			// Check for existing instance in the scene
			T existingInstance = FindFirstObjectByType<T>();
			if (existingInstance != null) {
				return existingInstance;
			}

			// Create a new GameObject and register it as a new instance
			GameObject obj = new GameObject(typeof(T).Name);
			T newInstance = obj.AddComponent<T>();

			if (!Application.isPlaying) {
				EditorApplication.delayCall += () => DestroyImmediate(obj);
			}

			return newInstance;
		}
		#endif
	}
	
}