using System;
using Tarabish.Definitions;
using Tarabish.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Tarabish.GameLoop {
	public class TrumpPicker : MonoBehaviour {
		public enum TrumpPicks {
			CLUB,
			DIAMOND,
			HEART,
			SPADE,
			PASS
		}

		//TODO Add an awake method that assignes the active color profile to the images
		public void OnButtonClick(string input) {
			GameManager.Instance.PickTrumpSuit(input);
		}
		public void OnButtonClick(TrumpPicks input = TrumpPicks.PASS) { //? For the bots
			GameManager.Instance.PickTrumpSuit(input.ToString());
		}
	}
}
