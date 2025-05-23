using System.Collections.Generic;
using System.Linq;
using Tarabish.Definitions;
using Tarabish.Editor.CustomAttributes;
using Tarabish.Extensions;
using Tarabish.Mechanics;
using Tarabish.UI.Playing;
using UnityEngine;


namespace Tarabish.Controllers {
	public class ControllerBase : MonoBehaviour {
		public ControllerInfo Info;
		public ReferenseManager.ControllerObjectReferences References;
		[HideInInspector] public TeamController? Team = null;

		private bool _isFirstTurn = true;

		public bool HasAnnouncedRun { get;  set; } = false;
    	public bool HasShownRun { get;  set; } = false;
    	public List<List<Card>> AnnouncedRuns { get; private set; } = new List<List<Card>>();
		public bool BellaAnnounced { get; private set; } = false;

		[Header("Scores")]
		public int CurrentScore = 0;
		public int TotalGameScore {
			get {
				_totalGameScore = GameScores.Sum();
				return GameScores.Sum();
			}
		}
		[SerializeField, ReadOnly] private int _totalGameScore = 0;
		public List<int> GameScores = new List<int>();

public virtual void StartTurn() {
    if (_isFirstTurn && GameManager.Instance.GameData.Status == GameData.GameStatus.Playing) {
        _isFirstTurn = false;
        CheckCardSequence();
        if (AnnouncedRuns.Count > 0) {
            // Prompt player to announce run
            AnnounceRun();
        }
    }

    if (GameManager.Instance.GameData.Status != GameData.GameStatus.Bidding) {
        References.Hand.CardInteraction = true;
    }
}

        public void AnnounceRun()
        {
            if (HasAnnouncedRun) return;

            if (AnnouncedRuns != null && AnnouncedRuns.Count > 0)
            {
                HasAnnouncedRun = true;

                this.Log(new LogLineContent()
                {
                    Level = ProjectManager.LogLevels.INFO,
                    Controller = this,
                    Text = $"{Info.Name} announces a run."
                });
            }
        }

        public void AnnounceBella() {
    if (!BellaAnnounced) {
        BellaAnnounced = true;
        GameManager.Instance.AddBellaPoints(this);

        this.Log(new LogLineContent() {
            Level = ProjectManager.LogLevels.INFO,
            Text = $"{Info.Name} announces Bella!",
            Controller = this
        });

        GameManager.Instance.ShowPopup(this, "Bella!");
    }
}


public void ShowRun() {
    if (!HasAnnouncedRun || HasShownRun) return;

    // Logic to show the run (e.g., reveal the cards)
    HasShownRun = true;
    this.Log(new LogLineContent() {
        Level = ProjectManager.LogLevels.INFO,
        Controller = this,
        Text = $"{Info.Name} shows their run."
    });
}


		public virtual void EndTurn() {
			References.Hand.CardInteraction = false;
		}

public List<Card> GetCardsBySuit(CardDefinitions.CardSuit suit) {


    var matchingCards = References.Hand.Cards.FindAll((c) => { return c.Info.Suit == suit; });
   // Debug.Log($"GetCardsBySuit called for Suit: {suit}. Matching Cards: {string.Join(", ", matchingCards.Select(c => $"{c.Info.Suit} {c.Info.Rank}"))}");
    return matchingCards;
}


		public List<Card> GetCardsByIndex(CardDefinitions.CardIndex index) {
			return References.Hand.Cards.FindAll((c) => { return c.Info.Index == index; });
		}

		//? This function determines if a randomly generated number between 0 and 100
		//? is less than or equal to the given percentage, simulating a chance mechanism.
		//? It returns true if the condition is met, false otherwise.
		//?? Maybe this can also be used by the player controller if there is a timeout or something
		protected virtual bool Decide(float percent) {
			int randomNumber = Random.Range(0, 101);
			bool result = randomNumber <= percent;
			this.LogVerbose($"{ControllerManager.Instance.GetLogPrefix(GameManager.Instance.GameData.ActiveController.Info)} Random number {randomNumber} <= chance: {percent}%\nResult: {result}");
			return result;
		}

		/// <summary>
		/// Get a random Suit
		/// </summary>
		/// <param name="loops">The amount of times to try until it returns null [0 = infinate]</param>
		/// <param name="chanceMultiplier">Influence the chance: 100 / suits + -(chanceMultiplier)</param>
		/// <returns></returns>
		protected virtual CardDefinitions.CardSuit? PickRandomCardSuit(int loops = 0, float chanceMultiplier = 0) {
			if (loops > 0) {
				for (int i = 0; i < loops; i++) {
					CardDefinitions.CardSuit? result = GetRandomCardSuit(chanceMultiplier);
					if (result != null) {
						return result;
					}
				}
			}
			else {
				CardDefinitions.CardSuit? result = null;

				while (result == null) { //?? is this stupid?
					result = GetRandomCardSuit(chanceMultiplier);
				}

				return result;
			}

			return null;
		}

		private CardDefinitions.CardSuit? GetRandomCardSuit(float chanceMultiplier = 0) {
			string[] suitArray = System.Enum.GetNames(typeof(CardDefinitions.CardSuit));

			foreach (string suit in suitArray) {
				if (Decide(100 / Mathf.Clamp(suitArray.Length + -(chanceMultiplier), 2, 100))) {
					return (CardDefinitions.CardSuit)System.Enum.Parse(typeof(CardDefinitions.CardSuit), suit);
				}
			}

			return null;
		}

		

private void CheckCardSequence() {
    List<List<Card>> sequenceGroups = new List<List<Card>>();
    List<Card> sortedCards = this.References.Hand.Cards
        .OrderBy(c => c.Info.Suit)
        .ThenBy(c => c.Info.Rank)
        .ToList();

    foreach (CardDefinitions.CardSuit suit in System.Enum.GetValues(typeof(CardDefinitions.CardSuit))) {
        List<Card> cardsOfSuit = sortedCards.Where(c => c.Info.Suit == suit).ToList();
        List<Card> currentSequence = new List<Card>();

        for (int i = 0; i < cardsOfSuit.Count; i++) {
            if (currentSequence.Count == 0) {
                currentSequence.Add(cardsOfSuit[i]);
            } else {
                Card prevCard = currentSequence.Last();
                if ((int)cardsOfSuit[i].Info.Rank == (int)prevCard.Info.Rank + 1) {
                    currentSequence.Add(cardsOfSuit[i]);
                } else {
                    if (currentSequence.Count >= 3) {
                        sequenceGroups.Add(new List<Card>(currentSequence));
                    }
                    currentSequence.Clear();
                    currentSequence.Add(cardsOfSuit[i]);
                }
            }
        }
        if (currentSequence.Count >= 3) {
            sequenceGroups.Add(new List<Card>(currentSequence));
        }
    }

    // Store the runs in AnnouncedRuns without awarding points
    AnnouncedRuns = sequenceGroups;
	
}
	}
}
