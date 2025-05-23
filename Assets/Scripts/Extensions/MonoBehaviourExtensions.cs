using Tarabish.Definitions;
using Tarabish.UI.Playing;
using Tarabish.Utilities;
using UnityEngine;

namespace Tarabish.Extensions {
	public static class MonoBehaviourExtensions {
		internal static void Log(this MonoBehaviour self, LogLineContent content) {
			//this.LogInfo($"{ControllerManager.Instance.GetLogPrefix(card.Controller.Info, null)} {CardDefinitions.GetCardLogPrefix(card.Info)} Transfered card to Pool");

			string cardInfo = (content.CardInfo != null) ? CardDefinitions.GetCardLogPrefix(content.CardInfo, null) : null;
			string controllerInfo = (content.Controller != null) ? ControllerManager.Instance.GetLogPrefix(content.Controller, null) : null;

			switch (content.Level) {
				case ProjectManager.LogLevels.INFO: 		self.LogInfo($"{controllerInfo}{cardInfo}\n{content.Text}"); 			break;
				case ProjectManager.LogLevels.MESSAGE: 		self.LogMessage($"{controllerInfo}{cardInfo}\n{content.Text}"); 		break;
				case ProjectManager.LogLevels.NOTIFY: 		self.LogNotify($"{controllerInfo}{cardInfo}\n{content.Text}"); 		break;
				case ProjectManager.LogLevels.ANNOUNCEMENT: self.LogAnnouncement($"{controllerInfo}{cardInfo}\n{content.Text}"); 	break;
				case ProjectManager.LogLevels.DEBUG: 		self.LogDebug($"{controllerInfo}{cardInfo}\n{content.Text}"); 		break;
				case ProjectManager.LogLevels.VERBOSE: 		self.LogVerbose($"{controllerInfo}{cardInfo}\n{content.Text}"); 		break;
				default: break;
			}

			if (content.Level <= ProjectManager.LogLevels.ANNOUNCEMENT) {
				ReferenseManager.Instance.GameLog.AddLog(content);
			}
		}
		
		internal static void LogInfo(this MonoBehaviour self, string message, Object context = null) {
			SendLogMessage(self, ProjectManager.LogLevels.INFO, message, context);
		}
		internal static void LogMessage(this MonoBehaviour self, string message, Object context = null) {
			SendLogMessage(self, ProjectManager.LogLevels.MESSAGE, message, context);
		}
		internal static void LogNotify(this MonoBehaviour self, string message, Object context = null) {
			SendLogMessage(self, ProjectManager.LogLevels.NOTIFY, message, context);
		}
		internal static void LogAnnouncement(this MonoBehaviour self, string message, Object context = null) {
			SendLogMessage(self, ProjectManager.LogLevels.ANNOUNCEMENT, message, context);
		}
		internal static void LogDebug(this MonoBehaviour self, string message, Object context = null) {
			SendLogMessage(self, ProjectManager.LogLevels.DEBUG, message, context);
		}
		internal static void LogVerbose(this MonoBehaviour self, string message, Object context = null) {
			SendLogMessage(self, ProjectManager.LogLevels.VERBOSE, message, context);
		}

		private static void SendLogMessage(MonoBehaviour self, ProjectManager.LogLevels level, string message, Object context = null) {
			if (!ProjectManager.LogLevel.HasFlag(level)) return;

			string colorHex = "FFFFFF";
			switch (level) {
				case ProjectManager.LogLevels.INFO: colorHex = "AAE7FF"; break;
				case ProjectManager.LogLevels.MESSAGE: colorHex = "FFFFFF"; break;
				case ProjectManager.LogLevels.NOTIFY: colorHex = "F8E16D"; break;
				case ProjectManager.LogLevels.ANNOUNCEMENT: colorHex = "FB7846"; break;
				case ProjectManager.LogLevels.DEBUG: colorHex = "52EBFF"; break;
				case ProjectManager.LogLevels.VERBOSE: colorHex = "6DD6FF"; break;
				default: break;
			}

			string msg = $"{GeneralUtilities.FormatLogPrefix($"<color=#{colorHex}>{level}</color>", null)}{((message.Contains("\n")) ? " " : "\n")}{message}";
			Debug.Log(msg, (context != null) ? context : self);
		}
	}
}