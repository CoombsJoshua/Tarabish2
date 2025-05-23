using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tarabish.UI.Playing {
	public class GameLogChunk : MonoBehaviour {
		public List<string> Lines = new List<string>();

		public TextMeshProUGUI Text => this.GetComponent<TextMeshProUGUI>();

		// private void Awake() {
		// 	Text = GetComponent<TextMeshProUGUI>();
		// } 

		public void AddLine(string line) {
			// Text.text += Lines.Count > 0 ? "\n" : "";
			// Text.text += line;
			// Text.text += String.Format("{0}{1}", Lines.Count > 0 ? "\n" : "", line);
			// Text.text += "";
			// Text.text += "HELLO WORLD";
			Text.text += ((Lines.Count > 0) ? "\n" : "") + line;
			Lines.Add(line);
		}
	}
}