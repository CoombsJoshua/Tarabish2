using UnityEngine;
using Tarabish.Mechanics;
using UnityEditor;
using Tarabish.Definitions;
using Tarabish.Extensions;
using System.Collections.Generic;
using System.Linq;
using Tarabish.UI.Playing;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Tarabish {
	public class CardManager : Singleton<CardManager> {
		[SerializeField] private bool _updateCards = false;
		[SerializeField] private bool _fetchCards = false;

		[SerializeField] private List<Card> _cardsInScene = new List<Card>();

		protected override void Awake() {
			base.Awake();
		}

		public void UpdateCardsInScene() {
			foreach (Card card in _cardsInScene) {
				card.UpdateCard();
			}
		}
		public void FetchCardsInScene() {
			_cardsInScene = GameObject.FindObjectsByType<Card>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
			this.LogDebug($"Found {_cardsInScene.Count} cards in scene");
		}
		
		#region Runtime
		public Card GenerateCard(CardInfo card) {
			Transform newCard = GameObject.Instantiate(ReferenseManager.Instance.CardPrefab.transform, ReferenseManager.Instance.IdleCardHolder.transform);
			Card cardScript = newCard.GetComponent<Card>();

			cardScript.Info = card;

			_cardsInScene.Add(cardScript);
			card.IsRendered = true;

			cardScript.UpdateCard();
			
			return cardScript;
		}
		public void DestroyCard(Card card) {
			card.Info.IsRendered = false;
			_cardsInScene.Remove(card);
			Destroy(card.gameObject);
		}

		public Card? FindCardByInfo(CardInfo info) {
			Card card = _cardsInScene.Find((c) => c.Info == info);

			if (!card) {
				Debug.LogError($"{CardDefinitions.GetCardLogPrefix(info)} Could not be found in scene");
			}

			return card;
		}

		public void DestroyCardsInScene() {
			FetchCardsInScene();
			foreach (Card card in _cardsInScene) {
				card.Info.IsRendered = false;
				Destroy(card.gameObject);
			}
		}
			
			#region InGame
public bool HandleCardInteraction(Card card) {
    if (card == null || card.Info == null) {
        Debug.LogError("Invalid card: Card is null or destroyed.");
        return false;
    }

    //// Block all card clicks for the player immediately
    //card.Controller.References.Hand.CardInteraction = false;
	
	   // var eventTrigger = card.GetComponent<EventTrigger>();
    //if (eventTrigger != null) {
    //    eventTrigger.enabled = false;
    //}


    List<Card> cardsInPool = ReferenseManager.Instance.Pool.Cards;
    CardDefinitions.CardSuit leadSuit = GameManager.Instance.GameData.LeadSuit ?? 
        (cardsInPool.Count > 0 ? cardsInPool[0].Info.Suit : card.Info.Suit);
    CardDefinitions.CardSuit trumpSuit = GameManager.Instance.GameData.TrumpSuit;

//    Debug.Log($"Handling Card Interaction: LeadSuit = {leadSuit}, TrumpSuit = {trumpSuit}, Card = {card.Info.Suit} {card.Info.Rank}");

    // Rule 1: Follow suit if you have cards in the lead suit.
    var cardsOfLeadSuit = card.Controller.GetCardsBySuit(leadSuit);
    if (cardsOfLeadSuit.Count > 0 && card.Info.Suit != leadSuit) {
        Debug.LogError($"You must follow the lead suit ({leadSuit}) if you have it! Cards in hand: {string.Join(", ", cardsOfLeadSuit.Select(c => c.Info.Suit + " " + c.Info.Rank))}");
        return false; // Invalid move.
    }

    // Rule 2: If a trump card is played, you must beat it if possible.
    bool isTrumpPlayed = cardsInPool.Any(c => c.Info.Suit == trumpSuit);
    if (isTrumpPlayed) {
        Card highestTrump = cardsInPool
            .Where(c => c.Info.Suit == trumpSuit)
            .OrderByDescending(c => c.Info.Rank)
            .FirstOrDefault();

        if (card.Info.Suit == trumpSuit) {
            bool hasHigherTrump = card.Controller.GetCardsBySuit(trumpSuit)
                .Any(c => c.Info.Rank > highestTrump.Info.Rank);

            if (hasHigherTrump && card.Info.Rank <= highestTrump.Info.Rank) {
                Debug.LogError($"You must play a higher trump card if you can! Highest trump: {highestTrump.Info.Suit} {highestTrump.Info.Rank}");
                return false; // Invalid move.
            }
        }
    }

    // Rule 3: Bella eligibility (optional for this logic).
    if (card.Info.Index == CardDefinitions.CardIndex.QUEEN &&
        card.Info.Suit == trumpSuit &&
        card.Controller.GetCardsBySuit(trumpSuit).Any(c => c.Info.Index == CardDefinitions.CardIndex.KING)) {
        Debug.Log($"Player {card.Controller.Info.Name} is eligible to announce Bella!");
    }

    // Rule 4: Process the card transfer.
    if (card.CurrentHolder.Type == CardHolderBase.CardHolderType.HAND) {
        card.CurrentHolder.TransferCard(card, card.Controller.References.PoolSlot);

        Debug.Log($"Card {card.Info.Suit} {card.Info.Rank} transferred to pool.");
        GameManager.Instance.OnCardEnterPool();
        return true; // Valid move.
    }

    Debug.LogError("Card interaction failed: Unhandled case.");
    return false; // Default fallback.
}





			#endregion
		#endregion


		#region InEditor
		private void OnValidate() {
			if (_updateCards) {
				UpdateCardsInScene();
				_updateCards = false;
			}

			if (_fetchCards) {
				FetchCardsInScene();
				_fetchCards = false;
			}
		}
		#endregion
	}
}
