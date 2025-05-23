using System.Collections.Generic;
using UnityEngine;

using Tarabish.Definitions;
using Tarabish.Mechanics;
using Tarabish.Extensions;
using Tarabish.Controllers;

namespace Tarabish {
	[System.Serializable] public class ActiveCardClass {
		[Tooltip("Should cards be returned to the deck?")]
		public bool RecycleCards = false;

		public List<CardInfo> Deck; //? Before the dealer has delt
		public List<CardInfo> Active; //? In Some Controllers hand
		public List<CardInfo> Pool; //? The the middle of the table
		public List<CardInfo> Used; //? After the card has been played

		// //? Game rules specific lists
		//?? we probs dont need this
		// public List<CardInfo> Kitty;
		
		public void TransferCard(CardInfo info, CardDefinitions.CardStatus toStatus) {
			DeckManager.Instance.LogVerbose($"{CardDefinitions.GetCardLogPrefix(info)} Transfering from {info.Status} to {toStatus}");
			
			if (info.IsRendered && !(toStatus == CardDefinitions.CardStatus.ACTIVE || toStatus == CardDefinitions.CardStatus.POOL)) {
				DeckManager.Instance.LogVerbose($"{CardDefinitions.GetCardLogPrefix(info)} Returning\nIsRendered: {info.IsRendered} | {toStatus}");
				ReturnCard(info);
				return;
			}

			List<CardInfo> from = GetListFromStatus(info.Status);
			List<CardInfo> to = GetListFromStatus(toStatus);

			if (from == null || to == null) {
				Debug.LogError($"{CardDefinitions.GetCardLogPrefix(info)} Could not transfer from {info.Status} to {toStatus}");
			}

			to.Add(info);
			from.Remove(info);

			info.Status = toStatus;
		}

		public void ReturnCard(Card card) {
			if (RecycleCards) {
				card.Info.Status = CardDefinitions.CardStatus.DECK;
				Deck.Add(card.Info);
			}
			else {
				card.Info.Status = CardDefinitions.CardStatus.USED;
				Used.Add(card.Info);
			}

			if (Active.Contains(card.Info)) {
				Active.Remove(card.Info);
			}
			else if (Pool.Contains(card.Info)) {
				Pool.Remove(card.Info);
			}
			else {
				Debug.LogError($"{CardDefinitions.GetCardLogPrefix(card.Info)} Could not be returned because card is not active/in pool");
			}

			CardManager.Instance.DestroyCard(card);
		}
		public void ReturnCard(CardInfo info) {
			Card card = CardManager.Instance.FindCardByInfo(info);
			
			if (!card) {
				Debug.LogError($"{CardDefinitions.GetCardLogPrefix(card.Info)} Unable to return non-existing card");
				return;
			}

			ReturnCard(card);
		}

		public void Clear() {
			Deck.Clear();
			Active.Clear();
			Used.Clear();
		}


		private List<CardInfo>? GetListFromStatus(CardDefinitions.CardStatus status) {
			switch (status) {
				case CardDefinitions.CardStatus.DECK: return Deck;
				case CardDefinitions.CardStatus.ACTIVE: return Active;
				case CardDefinitions.CardStatus.POOL: return Pool;
				case CardDefinitions.CardStatus.USED: return Used;
				// case CardDefinitions.CardStatus.KITTY: return Kitty;
				default: return null;
			}
		}
	}

	public class DeckManager : Singleton<DeckManager> {
		public CardDefinitions.CardSuitFlags ExcludedSuits;
		public CardDefinitions.CardIndexFlags ExcludedIndexes;
		public ActiveCardClass ActiveCards = new ActiveCardClass();

		[SerializeField] private bool _createDeck = false;
		[SerializeField] private bool _previewDeck = false;
		
    public void ResetDeck() {
        // Clear out all existing cards
        if (ActiveCards.Deck != null) ActiveCards.Deck.Clear();
        if (ActiveCards.Active != null) ActiveCards.Active.Clear();
        if (ActiveCards.Pool != null) ActiveCards.Pool.Clear();
        if (ActiveCards.Used != null) ActiveCards.Used.Clear();

        // Optionally, reset any excluded suits or indexes if they've changed.
        // ExcludedSuits = ...; (if needed)
        // ExcludedIndexes = ...; (if needed)

        // If cards were previously instantiated in the scene, ensure they are removed.
        CardManager.Instance.DestroyCardsInScene();
    }
		public void CreateDeck() {
			if (Application.isPlaying) CardManager.Instance.DestroyCardsInScene();
			ActiveCards.Clear();
			
			foreach (string suit in System.Enum.GetNames(typeof(CardDefinitions.CardSuit))) {
				if (ExcludedSuits.ToString().Contains(suit)) { continue; }

				Hand cardHolder = default;
				if (_previewDeck) {
					cardHolder = Instantiate(ReferenseManager.Instance.CardHolderPrefab, ReferenseManager.Instance.PreviewArea).GetComponent<Hand>();
				}

				foreach (string index in System.Enum.GetNames(typeof(CardDefinitions.CardIndex))) {
					if (ExcludedIndexes.ToString().Contains(index)) { continue; }

					CardInfo newCard = new CardInfo(
						(CardDefinitions.CardSuit)System.Enum.Parse(typeof(CardDefinitions.CardSuit), suit), 
						(CardDefinitions.CardIndex)System.Enum.Parse(typeof(CardDefinitions.CardIndex), index)
					);

					ActiveCards.Deck.Add(newCard);

					if (_previewDeck && cardHolder != null) {
						Card cardScript = CardManager.Instance.GenerateCard(newCard);
						cardHolder.AddCard(cardScript);
						cardScript.FaceUp = true;
					}
				}

				if (_previewDeck) {
					cardHolder.SetFaceUp(true);
				}
			}

			if (_previewDeck) {
				CardManager.Instance.FetchCardsInScene();
				CardManager.Instance.UpdateCardsInScene();
				_previewDeck = false;
			}
		}

		public void Suffle(List<CardInfo> cards) {
			//* References source: https://www.grepper.com/answers//?ucard=1
			System.Random rng = new System.Random();
			for (int i = cards.Count - 1; i > 0; i--) {
				int swapIndex = rng.Next(i + 1);
				CardInfo tmpCard = cards[i];
				cards[i] = cards[swapIndex];
				cards[swapIndex] = tmpCard;
			}
		}

		public void Deal() {
			//TODO Make this dynamic and configurable
			for (int i = 0; i < RuleDefinitions.DealCycles; i++) {
				bool faceUp = (i != RuleDefinitions.DealCycles - 1); //? Keep the face down for the kitty

				foreach (ControllerBase controller in ControllerManager.Instance.Controllers) {
					for (int j = 0; j < RuleDefinitions.DealCardAmount; j++) {
						this.LogVerbose($"{CardDefinitions.GetCardLogPrefix(ActiveCards.Deck[0])} Being dealt to {controller.Info.Name}");
						controller.References.Hand.AddCard(ActiveCards.Deck[0], faceUp);
					}
				}
			}
		}

		private void OnValidate() {
			//? We dont want to randomly display all cards to everyone when we are playing
			if (_previewDeck && Application.isPlaying) _previewDeck = false;

			if (_createDeck) {
				CreateDeck();
				_createDeck = false;
			}
		}
	}
}
