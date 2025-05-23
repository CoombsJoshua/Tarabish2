using Tarabish.Extensions;
using UnityEngine;

namespace Tarabish.Definitions {
	public class RuleDefinitions : Singleton<RuleDefinitions> {
		#region Static Fields
		public static int MinimumPlayers = default;
		public static int MaximumPlayers = default;

		public static int TargetScore = default;
		public static Difficulty Difficulty = default;
		public static int DealCardAmount = default;
		public static int DealCycles = default;
		#endregion

		#region Inspector
		[SerializeField] private int _minimumPlayers = 2;
		[SerializeField] private int _maximumPlayers = 4;

		[SerializeField] private int _targetScore = 500;
		[SerializeField] private Difficulty _Difficulty = Difficulty.Beginner;


		[Tooltip("The amount of cards to deal per cycle, (e.g. if 3, the cards are dealt in groups of three)")]
		[SerializeField] private int _dealCardAmount = 3;
		[Tooltip("The amount of times to repeat this deal cycle")]
		[SerializeField] private int _dealCycles = 3;
		#endregion

		protected override void Awake() {
			base.Awake();
			OnValidate();
		}

		private void OnValidate() {
			MinimumPlayers = _minimumPlayers;
			MaximumPlayers = _maximumPlayers;

			TargetScore = _targetScore;
			
			DealCardAmount = _dealCardAmount;
			DealCycles = _dealCycles;

			Difficulty = _Difficulty;
		}
	}
}
