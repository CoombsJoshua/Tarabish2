using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tarabish.UI {
	[System.Serializable] public class PanelElementReferences {
		public class TextValuePair {
			public TextMeshProUGUI Text;
			public int Value = 0;

			public TextValuePair(TextMeshProUGUI text, int value) {
				Text = text;
				Value = value;
			}
		}

		public TextMeshProUGUI Title;
		public TextMeshProUGUI Total;
		public TextMeshProUGUI Scores;
		public TextMeshProUGUI Points;
		public TextMeshProUGUI Tricks;

		public int TotalValue = 0;
		public List<int> ScoresValue = new List<int>();
		public int PointsValue = 0;
		public int TricksValue = 0;

		public PanelElementReferences(RectTransform parent) {
			Title = parent.Find("Title/Text").GetComponent<TextMeshProUGUI>();
			Total = parent.Find("Total/Text").GetComponent<TextMeshProUGUI>();
			Scores = parent.Find("Scroll View/Viewport/Content").GetComponent<TextMeshProUGUI>();
			Points = parent.Find("Points/Value").GetComponent<TextMeshProUGUI>();
			Tricks = parent.Find("Tricks/Value").GetComponent<TextMeshProUGUI>();
		}

		public void UpdateValues(int total, List<int> scores, int points, int tricks) {
			Total.text = total.ToString();
			Scores.text = string.Join("\n", scores);
			Points.text = points.ToString();
			Tricks.text = tricks.ToString();

			TotalValue = total;
			ScoresValue = scores;
			PointsValue = points;
			TricksValue = tricks;
		}

		public List<TextValuePair> AsPairs() {
			return new List<TextValuePair>() {
				new TextValuePair(Total, TotalValue),
				new TextValuePair(Points, PointsValue),
				new TextValuePair(Tricks, TricksValue),
			};
		}

	}

	public class GameScorePanel : MonoBehaviour {
		public List<PanelElementReferences> Panels = new List<PanelElementReferences>();
		public PanelElementReferences PlayerPanel;
		public PanelElementReferences OpponentPanel;

		[Header("Colors")]
		[SerializeField, Range(0, 100)] private int _aproxRange = 10;
		[SerializeField] private Color _less;
		[SerializeField] private Color _aprox;
		[SerializeField] private Color _more;

		private void Awake() {
			foreach (RectTransform child in this.transform) {
				Panels.Add(new PanelElementReferences(child));
			}

			//TODO Make this dynamic
			if (Panels.Count > 1) {
				PlayerPanel = Panels[0];
				OpponentPanel = Panels[1];
			}
		}

		public void UpdateText() {
			List<PanelElementReferences.TextValuePair> playerPairs = PlayerPanel.AsPairs();
			List<PanelElementReferences.TextValuePair> opponentPairs = OpponentPanel.AsPairs();

			for (int i = 0; i < playerPairs.Count; i++) {
				Color[] colors = GetScoreColors(playerPairs[i].Value, opponentPairs[i].Value);
				playerPairs[i].Text.color = colors[0];
				opponentPairs[i].Text.color = colors[1];
			}

			List<string> pScores = new List<string>();
			List<string> oScores = new List<string>();
			for (int i = 0; i < PlayerPanel.ScoresValue.Count; i++) {
				int playerScore = PlayerPanel.ScoresValue[i];
				int opponentScore = OpponentPanel.ScoresValue[i];

				Color[] colors = GetScoreColors(playerScore, opponentScore);
				pScores.Add($"<color=#{ColorUtility.ToHtmlStringRGB(colors[0])}>{((playerScore == 0) ? "xxx" : playerScore)}</color>");
				oScores.Add($"<color=#{ColorUtility.ToHtmlStringRGB(colors[1])}>{((opponentScore == 0) ? "xxx" : opponentScore)}</color>");
			}
			PlayerPanel.Scores.text = string.Join("\n", pScores);
			OpponentPanel.Scores.text = string.Join("\n", oScores);
		}

		private Color[] GetScoreColors(int a, int b) {
			Color[] colors = new Color[2] { _aprox, _aprox };

			if (a > b) {
				colors[0] = _more;
				colors[1] = ((a - b) <= _aproxRange) ? _aprox : _less;
			} else if (a < b) {
				colors[1] = _more;
				colors[0] = ((b - a) <= _aproxRange) ? _aprox : _less;
			}

			return colors;
		}

		private void OnValidate() {
			if (Panels.Count == 0) return;
			UpdateText();
		}
	}
}
