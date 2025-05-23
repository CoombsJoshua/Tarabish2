using System.Collections.Generic;
using Tarabish.Controllers;
using Tarabish.Definitions;
using Tarabish.Extensions;
using Tarabish.Mechanics;
using Tarabish.Utilities;
using Unity.Services.Authentication;
using UnityEngine;


namespace Tarabish {
	[System.Serializable] public class AIControllerSettings {
		public float DefaultTurnDelay = 1;

		//TODO Add a turn delay field for each GameStatus entry
		public float GetTurnDelay(GameData.GameStatus status) {
			//> Return the delay accosiated with the entry from the todo above here
			return DefaultTurnDelay;
		}
	}

	public class ControllerManager : Singleton<ControllerManager> {
		public List<ControllerBase> Controllers = new();

		public AIControllerSettings AISettings;

		private GameObject _controllerObject;

		[Header("Debug")]
		[SerializeField] private bool _reset = false;
		[SerializeField] private bool _allowInEditor = false;
		[SerializeField] private bool _addPlayer = false;
		[SerializeField] private bool _addAI = false;

		protected override void Awake() {
			base.Awake();
		}

		private void Start() {
			//? The player controller is never added as it already exists
			//? so set the things that are normally only set in the add function here
			// Controllers[0].References.Hand.Controller = Controllers[0];
			// Controllers[0].References.PoolSlot.Controller = Controllers[0];
		}

		public ControllerBase CreateClientController() {
			ControllerBase client = AddController(new ControllerInfo(AuthenticationService.Instance.PlayerName, ControllerInfo.ControllerType.CLIENT));
			ReferenseManager.Instance.LocalPlayer = (PlayerController)client;
			return client;

		}
public void ResetControllers() {
    Debug.Log("Resetting controllers...");
    foreach (var controller in Controllers) {
        if (controller != null) {
            Destroy(controller.gameObject);
        }
    }

    Controllers.Clear();

    // Reset GameData to ensure a clean state
    GameManager.Instance.GameData.Players.Clear();
    GameManager.Instance.GameData.PlayerCount = 0;
    GameManager.Instance.GameData.Teams.Clear();

    Debug.Log("Controllers and GameData reset complete.");
}



		public ControllerBase? AddController(ControllerInfo info) {
			if (Controllers.Count >= RuleDefinitions.MaximumPlayers) {
				Debug.LogError($"Player {info.Name} can not be added because the player count is already reached");
				return null;
			}
			
			if (!_controllerObject) _controllerObject = ReferenseManager.Instance.Controllers.gameObject;

			ControllerBase newController;

			if (info.IsAI) {
				newController = _controllerObject.AddComponent<AIController>();
			}
			else {
				newController = _controllerObject.AddComponent<PlayerController>();
			}

			newController.Info = info;
			newController.References = ReferenseManager.Instance.OtherReferences[Controllers.Count];
			newController.References.SetController(newController);
			Controllers.Add(newController);

			return newController;
		}

		public TeamController AddTeamController(List<ControllerBase> members) {
			TeamController team = _controllerObject.AddComponent<TeamController>();
			team.Members = members;

			members.ForEach((m) => { m.Team = team; });

			if (team.Members.Find((m) => m.Info.Type == ControllerInfo.ControllerType.CLIENT) != null) {
				team.Members.ForEach((m) => {
					if (m.Info.Type == ControllerInfo.ControllerType.OPONENT) {
						m.Info.Type = ControllerInfo.ControllerType.PARTNER;
					}
				});
			}

			return team;
		}

		

		public void RemoveController(ControllerBase controller) {
			if (!Controllers.Contains(controller)) {
				Debug.LogError($"Controller \"{controller.Info.Name}\" is not present in the Controllers list");
			}
			else {
				Controllers.Remove(controller);
			}
			
			if (Controllers.Count < RuleDefinitions.MinimumPlayers) {
				GameManager.Instance.EndGame("Droped below minimum players");
			}

			Destroy(controller);
		}
		public void RemoveController(ControllerInfo info) {
			ControllerBase controller = Controllers.Find((c) => c.Info == info);
			if (!controller) {
				Debug.LogError($"Controller \"{info.Name}\" could not be found");
				return;
			}
			RemoveController(controller);
		}


		public string GetLogPrefix(ControllerInfo info, string suffix = "\n") {
			return GeneralUtilities.FormatLogPrefix(new object[] {info.Type, info.Name}, suffix);
		}
		public string GetLogPrefix(ControllerBase controller, string suffix = null) {
			return GetLogPrefix(controller.Info, suffix);
		}

		public ControllerBase GetWinningController(List<ControllerBase> controllers = null, bool totalGameScore = false) {
			controllers ??= Controllers;

			ControllerBase winningController = controllers[0];
			int highscore = (totalGameScore) ? winningController.TotalGameScore : winningController.CurrentScore;

			foreach (ControllerBase controller in controllers) {
				int score = (totalGameScore) ? controller.TotalGameScore : controller.CurrentScore;

				if (controller.Team == null || controller.Team == winningController.Team) {
					if (score > highscore) {
						winningController = controller;
						highscore = score;
					}
				}
				else {
					if (
						(totalGameScore && controller.Team.TotalGameScore > winningController.Team.TotalGameScore) ||
						(!totalGameScore && controller.Team.CurrentScore > winningController.Team.CurrentScore)
					) {
						winningController = controller;
					}
				}
			}

			return winningController;
		}

		private void OnValidate() {
			if (!Application.isPlaying && !_allowInEditor) {
				_reset = false;
				_addPlayer = false;
				_addAI = false;
			}

			if (_reset) {
				_reset = false;
				for (int i = 0; i < Controllers.Count; i++) {
					if (Controllers[i].Info.Type != ControllerInfo.ControllerType.CLIENT) {
						RemoveController(Controllers[i]);
					}
				}
			}

			if (_addPlayer) {
				_addPlayer = false;
				ControllerInfo info = new ControllerInfo($"Player {Controllers.Count}", ControllerInfo.ControllerType.OPONENT);
				AddController(info);
			}
			if (_addAI) {
				_addAI = false;
				ControllerInfo info = new ControllerInfo($"AI {Controllers.Count}", ControllerInfo.ControllerType.OPONENT) {
					IsAI = true
				};
				AddController(info);
			}
		}
	}
}
