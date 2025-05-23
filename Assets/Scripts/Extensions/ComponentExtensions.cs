using UnityEngine;

namespace Tarabish.Extensions {
	public static class ComponentExtensions {
		public static bool HasComponent<T>(this Component component) {
			return component.TryGetComponent<T>(out _);
		}
	}
}