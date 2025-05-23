using System.Collections.Generic;
using System.Linq;
using Tarabish.Definitions;
using Tarabish.Editor.CustomAttributes;
using Tarabish.Mechanics;
using UnityEngine;

namespace Tarabish.Controllers {
	public class TeamController : MonoBehaviour {
		public List<ControllerBase> Members = new List<ControllerBase>();

		public int CurrentScore {
			get {
				int score = 0;

				foreach (ControllerBase member in Members) {
					score += member.CurrentScore;
				}

				_currentScore = score;
				return score;
			}
		}
		[SerializeField, ReadOnly] private int _currentScore = 0;

		public int PoolValue {
			get {
				int value = 0;

				foreach (ControllerBase member in Members) {
					if (member.References.PoolSlot.Card != null) {
						value += member.References.PoolSlot.Card.Info.Score;
					}
				}

				_poolValue = value;
				return value;
			}
		}
		[SerializeField, ReadOnly] private int _poolValue = 0;
		
		public Card? BestCard {
			get {
				Card? card = Members[0].References.PoolSlot.Card;

				foreach (ControllerBase member in Members) {
					if (member.References.PoolSlot.Card != null) {
						if (card == null || CardDefinitions.IsBetterCard(member.References.PoolSlot.Card, card)) {
							card = member.References.PoolSlot.Card; 
						}
					}
				}

				return card;
			}
		}

		public int TrumpCards {
			get {
				int count = 0;
				foreach (ControllerBase member in Members) {
					count += (
						member.References.PoolSlot.Card && 
						member.References.PoolSlot.Card.Info.Suit == GameManager.Instance.GameData.TrumpSuit
					) ? 1 : 0;
				}

				_currentTrumpCards = count;
				return count;
			}
		}
		[SerializeField, ReadOnly] private int _currentTrumpCards = 0;
		public bool IncludesTrumpCard { get { return (TrumpCards > 0); } }
		public bool PickedTrumpSuit {
			get {
				return (Members.Find((c) => c.Info == GameManager.Instance.GameData.TrumpController) != null);
			} set{
				
			}
		}

		public List<int> GameScores {
			get {
				List<int> scores = new List<int>();

				for (int i = 0; i < Members.First().GameScores.Count; i++) {
					int score = 0;

					foreach (ControllerBase member in Members) {
						score += member.GameScores[i];
					}

					scores.Add(score);
				}

				_gameScores = scores;
				return scores;
			}
		}
		[SerializeField, ReadOnly] private List<int> _gameScores = new List<int>();

		public int TotalGameScore {
			get {
				_totalGameScore = GameScores.Sum();
				return GameScores.Sum();
			}
		}
		[SerializeField, ReadOnly] private int _totalGameScore = 0;


		//? Get all the values once so that they get updated in the inspector
		public void UpdateValues() {
			if (ProjectManager.RELEASE) return;
			_ = CurrentScore;
			_ = PoolValue;
			_ = BestCard;
			_ = TrumpCards;
			_ = IncludesTrumpCard;
			_ = PickedTrumpSuit;
			_ = GameScores;
			_ = TotalGameScore;
		}
	}
}
