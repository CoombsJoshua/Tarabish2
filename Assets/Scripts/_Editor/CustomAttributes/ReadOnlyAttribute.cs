
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//> Source: https://discussions.unity.com/t/how-to-make-a-readonly-property-in-inspector/75448/5

//! Doesnt work...
//?? for some reason, this is not picked up on by the rest of the workspace...
//?? this does work however if it is outside the Editor directory but that is bad practice :/
//?? I might get back to this later
namespace Tarabish.Editor.CustomAttributes {
	[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ReadOnlyAttribute : PropertyAttribute {

	}

	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
	#endif
}
