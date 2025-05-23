using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tarabish.Definitions;
using Tarabish.Mechanics;
using Tarabish.GameLoop;
using Tarabish.Extensions;
using System.Linq;

namespace Tarabish.Controllers {
	public class AIController : ControllerBase {
		public override void StartTurn() {
			base.StartTurn();
			StartTurnSequence();
		}

		private void StartTurnSequence() {
			this.LogVerbose($"{ControllerManager.Instance.GetLogPrefix(this.Info)} Start Turn Sequence");
			switch (GameManager.Instance.GameData.Status) {
				case GameData.GameStatus.Bidding: {
					DoPickTrumpSuit(ControllerManager.Instance.AISettings.GetTurnDelay(GameData.GameStatus.Playing));
				} break;
				case GameData.GameStatus.Playing: {
					DoCardPickingSequence(ControllerManager.Instance.AISettings.GetTurnDelay(GameData.GameStatus.Bidding));
				} break;

				default: StartCoroutine(GameLoopKit.WaitForGameStatusChange(StartTurnSequence)); break;
			}
		}

private void DoPickTrumpSuit(float turnDelay) {
    GameLoopKit.DoTurnDelay(() => {
        CardDefinitions.CardSuit? result = PickRandomCardSuit(1, -10);

        // If no trump is picked, make the dealer auto-pick.
        if (result == null && GameManager.Instance.GameData.ActiveController.Controller == GameManager.Instance.GameData.Dealer) {
            result = CardDefinitions.CardSuit.SPADE; // Default to a suit (e.g., SPADES).
        }

        GameManager.Instance.PickTrumpSuit((result == null) ? "PASS" : result.ToString());
    }, turnDelay);
}


private void DoCardPickingSequence(float turnDelay) {
    GameLoopKit.DoTurnDelay(() => {
        Card pick = null;
        List<Card> cardsInPool = ReferenseManager.Instance.Pool.Cards;

        // Determine the lead suit if cards are in the pool
        CardDefinitions.CardSuit? leadSuit = cardsInPool.Count > 0 ? cardsInPool[0].Info.Suit : (CardDefinitions.CardSuit?)null;
        CardDefinitions.CardSuit trumpSuit = GameManager.Instance.GameData.TrumpSuit;

        // If there is no lead suit (first card of the trick), pick any card at random
        if (leadSuit == null) {
            pick = PickRandomCard();
        } else {
            // Rule 1: Follow the lead suit if possible
            List<Card> leadSuitCards = GetCardsBySuit(leadSuit.Value);
            if (leadSuitCards.Count > 0) {
                pick = PickValidCard(leadSuitCards);
            } else {
                // Rule 2: If no lead suit, play a trump card if possible
                List<Card> trumpCards = GetCardsBySuit(trumpSuit);
                if (trumpCards.Count > 0) {
                    pick = PickValidCard(trumpCards, isTrump: true);
                } else {
                    // Rule 3: Play any card if no lead suit or trump available
                    pick = PickValidCard(References.Hand.Cards);
                }
            }
        }

        // Ensure a valid card is picked
        if (pick == null) {
            Debug.LogWarning($"{this.Info.Name} could not find a valid card to play!");
            pick = FallbackCard();
        }

        // Attempt to play the card
        if (!CardManager.Instance.HandleCardInteraction(pick)) {
            Debug.LogWarning($"{this.Info.Name} attempted an invalid card. Retrying...");
            RetryWithFallbackCards(pick);
        }
    }, turnDelay);
}


// Retry mechanism
private void RetryWithFallbackCards(Card invalidPick) {
    List<Card> fallbackCards = References.Hand.Cards.Except(new List<Card> { invalidPick }).ToList();
    foreach (Card fallbackPick in fallbackCards) {
        if (CardManager.Instance.HandleCardInteraction(fallbackPick)) {
            return;
        }
    }
    Debug.LogError($"{this.Info.Name} could not find any valid card to play!");
}

private Card FallbackCard() {
    Debug.LogWarning($"{this.Info.Name} is falling back to the first card in their hand.");
    return References.Hand.Cards.First();
}


private Card PickValidCard(List<Card> cards, bool isTrump = false) {
    cards = cards.OrderBy(c => c.Info.Rank).ToList();

    if (isTrump) {
        List<Card> cardsInPool = ReferenseManager.Instance.Pool.Cards;
        Card highestTrump = cardsInPool
            .Where(c => c.Info.Suit == GameManager.Instance.GameData.TrumpSuit)
            .OrderByDescending(c => c.Info.Rank)
            .FirstOrDefault();

        if (highestTrump != null) {
            // Try to pick a higher trump card if available
            Card higherTrump = cards.FirstOrDefault(c => c.Info.Rank > highestTrump.Info.Rank);
            if (higherTrump != null) {
                return higherTrump;
            }
        }
    }

    return cards.FirstOrDefault(); // Fallback to the lowest card
}


private Card PickRandomCard() {
    if (References.Hand.Cards.Count == 0) {
        Debug.LogError("AI has no cards to play!");
        return null;
    }

    int randomIndex = Random.Range(0, References.Hand.Cards.Count);
    Card randomCard = References.Hand.Cards[randomIndex];

    Debug.Log($"{this.Info.Name} is randomly playing: {randomCard.Info.Suit} {randomCard.Info.Rank}");
    return randomCard;
}



	}
}