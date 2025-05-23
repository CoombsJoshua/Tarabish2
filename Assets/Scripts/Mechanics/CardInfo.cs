using System.Linq;
using System.Collections.Generic;
using Tarabish.Definitions;
using UnityEngine;

namespace Tarabish.Mechanics {
	[System.Serializable] public class CardInfo {
		public CardDefinitions.CardSuit Suit = CardDefinitions.CardSuit.SPADE;
		public CardDefinitions.CardIndex Index = CardDefinitions.CardIndex.TWO;

		[Header("Stats")]
		public CardDefinitions.CardStatus Status = CardDefinitions.CardStatus.DECK;
		public bool IsRendered = false;

		public int Score { get { return CardDefinitions.GetCardValue(this); } }
		public int Rank {
			get {
				return System.Array.IndexOf(System.Enum.GetNames(typeof(CardDefinitions.CardIndex)), Index.ToString());
			}
		}

		public CardInfo(CardDefinitions.CardSuit suit, CardDefinitions.CardIndex index) {
			Suit = suit;
			Index = index;
		}
	}
}
