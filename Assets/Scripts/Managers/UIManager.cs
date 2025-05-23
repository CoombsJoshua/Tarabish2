using System.Collections.Generic;
using UnityEngine;

using Tarabish.Controllers;
using Tarabish.Extensions;
using Tarabish.UI;
using Tarabish.UI.Playing;
using System.Runtime.CompilerServices;
using Tarabish.Definitions;

namespace Tarabish {
	public class UIManager : Singleton<UIManager> {
		public static PanelElementReferences PlayerScorePanel => ReferenseManager.Instance.GameScorePanel.PlayerPanel;
		public static PanelElementReferences OpponentScorePanel => ReferenseManager.Instance.GameScorePanel.OpponentPanel;

		private static GameLog _gameLog => ReferenseManager.Instance.GameLog;

		public static void UpdateScores() {
			if (GameManager.Instance.GameData.Teams == null || GameManager.Instance.GameData.Teams.Count == 0) {
    Debug.LogWarning("Teams are not initialized yet, skipping UpdateScores()");
    return;
}
			ControllerBase client = ControllerManager.Instance.Controllers.Find((c) => c.Info.Type == ControllerInfo.ControllerType.CLIENT);
			List<ControllerBase> opponents = ControllerManager.Instance.Controllers.FindAll((c) => c.Info.Type == ControllerInfo.ControllerType.OPONENT);


			if (client.Team == null) {
				PlayerScorePanel.UpdateValues(client.TotalGameScore, client.GameScores, client.CurrentScore, 0);
			} else {
				PlayerScorePanel.UpdateValues(client.Team.TotalGameScore, client.Team.GameScores, client.Team.CurrentScore, 0);
			}

			int oppTotal = 0;
			List<int> oppScores = new List<int>();
			int oppPoints = 0;
			int oppTricks = 0;
			for (int i = 0; i < opponents.Count; i++) {
				ControllerBase opponent = opponents[i];
				
				oppTotal += opponent.TotalGameScore;
				oppPoints += opponent.CurrentScore;
				oppTricks += 0; //TODO

				for (int j = 0; j < opponent.GameScores.Count; j++) {
					if (i == 0) {
						oppScores.Add(opponent.GameScores[j]);
					} else {
						oppScores[j] += opponent.GameScores[j];
					}
				}
			}

			OpponentScorePanel.UpdateValues(oppTotal, oppScores, oppPoints, oppTricks);

			ReferenseManager.Instance.GameScorePanel.UpdateText();
		}
		
        public static void UpdateGameLogVisibility()
        {

			_gameLog.gameObject.SetActive(false);
            // Disable or enable the _gameLog UI object based on the difficulty level
            // if (RuleDefinitions.Difficulty > Difficulty.Beginner)
            // {
            //     _gameLog.gameObject.SetActive(false);
            //     Debug.Log("Game log UI disabled for non-Beginner difficulty.");
            // }
            // else
            // {
            //     _gameLog.gameObject.SetActive(true);
            //     Debug.Log("Game log UI enabled for Beginner difficulty.");
            // }
        }
		
		//TODO Add more params to be able to parse through the info
		public static void AddGameLog(LogLineContent logLine) {
			_gameLog.AddLog(logLine);
		}

		    // New method to reset the UI state
    public void ResetUI() {
        // Reset player score panel to initial values
        PlayerScorePanel.UpdateValues(0, new List<int>(), 0, 0);
        
        // Reset opponent score panel to initial values
        OpponentScorePanel.UpdateValues(0, new List<int>(), 0, 0);

        // Clear or reset the game log if needed (assuming there's a method to clear)
        _gameLog.ClearLogs(); // Make sure a ClearLogs() method exists in GameLog

        // Update UI text to reflect the reset
        ReferenseManager.Instance.GameScorePanel.UpdateText();

        // Optionally set the game log visibility based on current difficulty
        UpdateGameLogVisibility();
    }
	}
}