using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tarabish.Mechanics {
	public class Hand : CardHolderBase {
		public override void AddCard(Card card, bool faceUp = true) {
			base.AddCard(card, faceUp);
			card.CurrentHolder = this;
			DeckManager.Instance.ActiveCards.TransferCard(card.Info, Definitions.CardDefinitions.CardStatus.ACTIVE);
		}

		public override void RemoveCard(Card card) {
			base.RemoveCard(card);
			card.CurrentHolder = null;
		}

		public void SortHand() {
			Cards.Sort((a, b) => {
				int suitComparison = a.Info.Suit.CompareTo(b.Info.Suit);
				if (suitComparison != 0) return suitComparison;

				int scoreComparison = a.Score.CompareTo(b.Score);
				if (scoreComparison != 0) return scoreComparison;

				return a.Info.Rank - b.Info.Rank;
			});

			List<Card> faceDown = new List<Card>();
			
			for (int i = 0; i < Cards.Count; i++) {
				Card card = Cards[i];
				if (!card.FaceUp) {
					faceDown.Add(card);
					continue;
				}

				card.transform.SetSiblingIndex(i);
			}

			foreach (Card card in faceDown) {
				card.transform.SetAsLastSibling();
			}
		}
	}
}
