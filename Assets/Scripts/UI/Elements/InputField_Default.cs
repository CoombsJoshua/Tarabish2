using TMPro;
using UnityEngine;

namespace Tarabish.UI.Elements {
	public class InputField_Default : MonoBehaviour {
		public TextMeshProUGUI Label;
		public TMP_InputField InputField;

		public string Value => InputField.text;
	}
}