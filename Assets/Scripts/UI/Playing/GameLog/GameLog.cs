using System.Collections.Generic;
using Tarabish.Controllers;
using Tarabish.Definitions;
using Tarabish.Mechanics;
using UnityEngine;
using UnityEngine.UI;

namespace Tarabish.UI.Playing {
	[System.Serializable] public class LogLineContent {
		public string Text;
		public ProjectManager.LogLevels Level = ProjectManager.LogLevels.INFO;

		public CardInfo? CardInfo = null;
		public ControllerBase? Controller = null;

		public SuitInfo? SuitInfo => (CardInfo != null) ? CardDefinitions.GetSuitInfo(this.CardInfo.Suit) : null;

		public LogLineContent(string? text = "", ProjectManager.LogLevels level = ProjectManager.LogLevels.INFO, ControllerBase? controller = null, CardInfo? cardInfo = null) {
			Text = text;
			Level = level;
			Controller = controller;
			CardInfo = cardInfo;
		}
	}
	public class GameLog : MonoBehaviour {
		[System.Serializable] public class LogLevelColor {
			public string Name;
			public Color Color;

			public LogLevelColor(string name) {
				Name = name;
			}
		}

		public List<LogLineContent> Logs = new List<LogLineContent>();
		public List<LogLevelColor> LogLevelColors = new List<LogLevelColor>();

		private RectTransform _scrollContentRect;

		private void Awake() {
			if (_scrollContentRect == null) {
				_scrollContentRect = this.GetComponent<ScrollRect>().content;
			}
		}

		public void AddLog(LogLineContent content) {
			GameLogLine logLine = Instantiate(ReferenseManager.Instance.GameLogLine, _scrollContentRect).GetComponent<GameLogLine>();
			logLine.SetInfo(content);
		}

		private void OnValidate() {
			string[] logLevels = System.Enum.GetNames(typeof(ProjectManager.LogLevels));
			if (LogLevelColors.Count < logLevels.Length) {
				for (int i = 0; i < logLevels.Length; i++) {
					string level = logLevels[i];
					if (LogLevelColors.Find((llc) => llc.Name == level) != null) continue;
					LogLevelColors.Insert(i, new LogLevelColor(level));
				}
			}
		}

				/// <summary>
		/// Clears all log entries from the UI and the Logs list.
		/// </summary>
		public void ClearLogs() {
			// Clear the Logs list
			Logs.Clear();

			// Destroy all children (log lines) under the scroll content
			for (int i = _scrollContentRect.childCount - 1; i >= 0; i--) {
				Transform child = _scrollContentRect.GetChild(i);
				Destroy(child.gameObject);
			}
		}
	}
}