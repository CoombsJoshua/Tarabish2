using System.Collections;
using Tarabish.Extensions;
using UnityEngine;

namespace Tarabish.GameLoop {
	public class GameLoopKit : Singleton<GameLoopKit> {
	    public static Coroutine TurnDelayRoutine;
		
		public static IEnumerator TurnDelay(System.Action callback, float delay = 2) {
			yield return new WaitForSeconds(delay * GameManager.Instance.TestingControls.ControllerTurnDelayMultiplier);
			callback.Invoke();
		}
		public static bool DoTurnDelay(System.Action callback, float? delay = null) {
			delay ??= ControllerManager.Instance.AISettings.DefaultTurnDelay;
			
			if (TurnDelayRoutine != null) {
				Instance.StopCoroutine(TurnDelayRoutine);
				TurnDelayRoutine = null;
			}

			if (delay > 0) {
				TurnDelayRoutine = Instance.StartCoroutine(TurnDelay(callback, (float)delay));
				return true;
			}
			else {
				callback.Invoke();
			}
			
			return false;
		}

		public static IEnumerator WaitForGameStatusChange(System.Action callback) {
			GameData.GameStatus currentStatus = GameManager.Instance.GameData.Status;
			Instance.LogVerbose($"Waiting until {currentStatus} updates");
			yield return new WaitUntil(() => currentStatus != GameManager.Instance.GameData.Status);
			callback.Invoke();
		}
	}
}
