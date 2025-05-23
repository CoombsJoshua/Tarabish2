using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

using Tarabish.Controllers;
using Tarabish.Definitions;
using Tarabish.Extensions;
using Tarabish.GameLoop;
using Tarabish.Mechanics;
using Tarabish.Editor.CustomAttributes;
using Tarabish.Utilities;
using Tarabish.UI;
using Tarabish.UI.Menu;
using Tarabish.UI.Playing;
using Tarabish.Netcode;
public enum Difficulty{ Beginner, Intermediate, Expert}
namespace Tarabish {
	//? Made this its own class for easier access to all the info we need through the game
	[System.Serializable] public class ActiveControllerInfo {
		public string Name;
		public ControllerInfo.ControllerType Type;

		public ControllerBase Controller;
		[HideInInspector] public ControllerInfo Info;

		[HideInInspector] public PlayerController Player;
		[HideInInspector] public AIController AI;

		public void Set(ControllerBase controller) {
			Name = controller.Info.Name;
			Type = controller.Info.Type;

			Controller = controller;
			Info = controller.Info;

			Player = (!Info.IsAI) ? controller.GetComponent<PlayerController>() : null;
			AI = (Info.IsAI) ? controller.GetComponent<AIController>() : null;

			Controller.StartTurn();
		}

		//? Set the next controller as active
public void Next(GameData.GameStatus status) {
    Controller.EndTurn();

    int currentIndex = ControllerManager.Instance.Controllers.IndexOf(Controller);
    int playerCount = ControllerManager.Instance.Controllers.Count;

    for (int i = 1; i < playerCount; i++) {
        int nextIndex = (currentIndex + i) % playerCount;
        var nextController = ControllerManager.Instance.Controllers[nextIndex];

        // Handle different logic based on the game status
        switch (status) {
            case GameData.GameStatus.Bidding:
                // Skip players who have passed during bidding
                if (!GameManager.Instance.GameData.PlayersPassed.Contains(nextController)) {
                    GameManager.Instance.GameData.CurrentBidderIndex = nextIndex;
                    Set(nextController);
                    Debug.Log($"<color=blue>Bidding - Next player: {nextController.Info.Name}</color>");
                    return;
                }
                break;

            case GameData.GameStatus.Playing:
                // Playing logic - No conditions, just rotate to the next player
                Set(nextController);
                Debug.Log($"<color=green>Playing - Next player: {nextController.Info.Name}</color>");
                return;

            default:
                // Default behavior for other game states
                Set(nextController);
                Debug.Log($"<color=gray>Default - Next player: {nextController.Info.Name}</color>");
                return;
        }
    }

    // Handle edge cases like all players passing during bidding
    if (status == GameData.GameStatus.Bidding) {
        Debug.Log($"All players passed. Dealer {GameManager.Instance.GameData.Dealer.Info.Name} must pick a trump suit.");
        GameManager.Instance.GameData.SetTrumpSuit(CardDefinitions.CardSuit.CLUB); // Default fallback
        GameManager.Instance.EndBidding();
    }
}




	}

[System.Serializable]
public class GameData {
    public enum GameStatus {
        Idle,
        Dealing,
        Bidding, //? Each controller is picking a card to put in the pool
        Playing,
        Counting,
        EndRound,
    }
    
    public TeamController BiddingTeam { get; set; }
    public GameStatus Status = default;

public int CurrentBidderIndex { get; set; }
    // Copy the controllers list and store it here to keep the order of players consistent
    public List<ControllerBase> Players { get; private set; } = new List<ControllerBase>();
    public List<TeamController> Teams = new List<TeamController>();
    // Add PlayersPassed property here
    public List<ControllerBase> PlayersPassed { get; private set; } = new List<ControllerBase>();

    public int TargetPlayerCount = 4;
    [ReadOnly] public int PlayerCount = 0;
    public ControllerBase Dealer; 

    // The controller that has the turn
    [ReadOnly] public ActiveControllerInfo ActiveController;

    public CardDefinitions.CardSuit TrumpSuit = CardDefinitions.CardSuit.CLUB;
    public CardDefinitions.CardSuit? LeadSuit { get; set; } // Add LeadSuit here
    public ControllerInfo TrumpController;

    public Difficulty difficulty;

    public void Initialize() {
        // Copy the controllers from the controller manager list to Players
        ControllerManager.Instance.Controllers.ForEach((c) => { Players.Add(c); });
        PlayerCount = Players.Count;

        if (PlayerCount < 4) { // TODO: Make this dynamic instead of hardcoded
            DeckManager.Instance.ExcludedIndexes = 
                CardDefinitions.CardIndexFlags.TWO |
                CardDefinitions.CardIndexFlags.THREE |
                CardDefinitions.CardIndexFlags.FOUR |
                CardDefinitions.CardIndexFlags.FIVE |
                CardDefinitions.CardIndexFlags.SIX |
                CardDefinitions.CardIndexFlags.SEVEN;
            
            UIManager.PlayerScorePanel.Title.text = "You";
        }
        else if (PlayerCount == 4) { // TODO: Make this dynamic instead of hardcoded
            DeckManager.Instance.ExcludedIndexes = CardDefinitions.CardIndexFlags.TWO | CardDefinitions.CardIndexFlags.THREE | CardDefinitions.CardIndexFlags.FOUR | CardDefinitions.CardIndexFlags.FIVE;
            
            Teams.Add(ControllerManager.Instance.AddTeamController(new List<ControllerBase>() {Players[0], Players[2]}));
            Teams.Add(ControllerManager.Instance.AddTeamController(new List<ControllerBase>() {Players[1], Players[3]}));

            UIManager.PlayerScorePanel.Title.text = "Team";
        }
    }

    public void ResetScores() {
        // Reset all round scores
        Players.ForEach((p) => {
            p.CurrentScore = 0;
            p.GameScores.Clear();
        });
        Teams.ForEach((t) => t.GameScores.Clear());
    }

    public void SetTrumpSuit(CardDefinitions.CardSuit suit) {
        TrumpSuit = suit;
        // TODO: Add logic to this to also insert the partner if there are 4 players
        TrumpController = ActiveController.Info;

        // Set BiddingTeam
        if (Teams.Count > 0) {
            BiddingTeam = Teams.FirstOrDefault(t => t.Members.Contains(ActiveController.Controller));
            BiddingTeam.PickedTrumpSuit = true;
        } else {
            BiddingTeam = null; // For individual play
        }

        Image display = ReferenseManager.Instance.TrumpSuitDisplay;
        display.sprite = CardDefinitions.GetSuitImageByCardSuit(suit);
        display.color = CardDefinitions.GetColorByCardSuit(suit);
    }
}


	public class GameManager : Singleton<GameManager> {
		public GameData GameData = new();
		public CanvasManager m_CanvasManager;
		public GameTestingControls TestingControls;
       // public List<ControllerBase> PlayersPassed { get; private set; } = new List<ControllerBase>();

		protected override void Awake() {
			base.Awake();
			OnValidate();

			if (!Application.isEditor && ProjectManager.DevBuild) {
				Debug.developerConsoleEnabled = true;
			}
		}

		private void Start() {
			TestingControls.Validate(true);
		}
        public void AnnounceRun()
        {
            var controller = GameData.ActiveController.Controller;

            if (controller.HasAnnouncedRun)
            {
                Debug.Log($"{controller.Info.Name} already announced their run.");
                ShowPopup(controller, "Already announced.");
                return;
            }

            if (controller.AnnouncedRuns == null || controller.AnnouncedRuns.Count == 0)
            {
                Debug.Log($"{controller.Info.Name} has no valid runs to announce.");
                ShowPopup(controller, "No runs found.");
                return;
            }

            controller.AnnounceRun();
            ShowPopup(controller, "I have a run!");
        }



        public void AnnounceBella()
        {
            var controller = GameData.ActiveController.Controller;

            // Must hold both King and Queen of trump suit
            var hasQueen = controller.GetCardsBySuit(GameData.TrumpSuit)
                .Any(c => c.Info.Index == CardDefinitions.CardIndex.QUEEN);
            var hasKing = controller.GetCardsBySuit(GameData.TrumpSuit)
                .Any(c => c.Info.Index == CardDefinitions.CardIndex.KING);

            if (hasQueen && hasKing)
            {
                controller.AnnounceBella();
            }
            else
            {
                Debug.LogWarning($"{controller.Info.Name} tried to announce Bella but doesn't have King and Queen of {GameData.TrumpSuit}.");
                ShowPopup(controller, "No Bella (need K & Q of trump)");
            }
        }


        public void AddBellaPoints(ControllerBase controller) {
    // Bella is always 20 points
    int bellaPoints = 20;

    // Add Bella points to the current score
    controller.CurrentScore += bellaPoints;

    this.Log(new LogLineContent() {
        Level = ProjectManager.LogLevels.ANNOUNCEMENT,
        Text = $"{controller.Info.Name} scores <color=green>{bellaPoints}</color> points for Bella!"
    });

    // Update the UI to reflect the new scores
    UIManager.UpdateScores();
}


public void InitializeStart() {
    SettingsManager.Instance.Initialize();
    UIManager.UpdateGameLogVisibility();
    m_CanvasManager.SwitchCanvas(MenuType.Playing);
    ControllerManager.Instance.CreateClientController();

    HashSet<string> usedNames = new HashSet<string>();
    foreach (var player in GameData.Players) {
        usedNames.Add(player.Info.Name);
    }

    while (GameData.PlayerCount < GameData.TargetPlayerCount) {
        string aiName;
        do {
            aiName = $"{CreditDefinitions.GetAIPlayer().GetName()} (AI)";
        } while (usedNames.Contains(aiName));

        usedNames.Add(aiName);

        ControllerManager.Instance.AddController(new ControllerInfo(
            aiName,
            ControllerInfo.ControllerType.OPONENT
        ) { IsAI = true });

        GameData.PlayerCount++;
    }

    GameData.Initialize();

    // Assign PoolSlot names
    for (int i = 0; i < ControllerManager.Instance.Controllers.Count; i++) {
        var controller = ControllerManager.Instance.Controllers[i];
        var poolSlot = ReferenseManager.Instance.OtherReferences[i].PoolSlot; // Get the PoolSlot reference.
        if (poolSlot != null) {
            poolSlot.Controller = controller; // Ensure the controller reference is set.
            poolSlot.UpdateControllerName();  // Update the UI with the controller's name.
        }
    }

    StartGame();
}





		
		public void StartGame() {
			//ReferenseManager.Instance.MenuTitle.Buttons.Start.gameObject.SetActive(false);
		
			if (GameData.PlayerCount < RuleDefinitions.MinimumPlayers) {
				Debug.LogError($"Game can not start with less then {RuleDefinitions.MinimumPlayers} players");
				return;
			}
			else if (GameData.PlayerCount > RuleDefinitions.MaximumPlayers) {
				Debug.LogError($"Game can not start with more then {RuleDefinitions.MaximumPlayers} players");
				return;
			}
			else if (GameData.PlayerCount == 3) {
				// this.LogNotify($"This version does not support a 3 player game yet. Please start a game with 2 or 4 players.");
				this.Log(new LogLineContent("This version does not support a 3 player game yet. Please start a game with 2 or 4 players.", ProjectManager.LogLevels.NOTIFY));
				return;
			}

			//? When the game restarts with the same players, this resets all scores
			GameData.ResetScores();
			
			StartRound();
		}

private void StartRound() {
    // Reset scores for the round
    GameData.Players.ForEach((p) => p.CurrentScore = 0);
    UIManager.UpdateScores();
    // Clear passed players for a fresh start
    GameData.PlayersPassed.Clear();
// If no dealer is set yet (e.g., first game), select a random player as the dealer
if (GameData.Dealer == null) {
    int randomIndex = UnityEngine.Random.Range(0, GameData.Players.Count);
    GameData.Dealer = GameData.Players[randomIndex];
}
        // Highlight the dealer
    HighlightDealer(GameData.Dealer);
    
    ShowPopup(GameData.Dealer, "I am the dealer");

    //* Set up the game
    DeckManager.Instance.CreateDeck();
    DeckManager.Instance.Suffle(DeckManager.Instance.ActiveCards.Deck);
    DeckManager.Instance.Deal();
    CardManager.Instance.FetchCardsInScene();
    CardManager.Instance.UpdateCardsInScene();

    foreach (ControllerBase controller in ControllerManager.Instance.Controllers) {
        controller.References.Hand.SortHand();
    }

    // Collect and compare runs after players have had a chance to announce
    CollectAndCompareRuns();

    // Find the dealer's index
    int dealerIndex = GameData.Players.IndexOf(GameData.Dealer);
    GameData.CurrentBidderIndex = (dealerIndex + 1) % GameData.Players.Count;

    // Player to the left of the dealer goes first
    int firstLeaderIndex = (dealerIndex + 1) % GameData.Players.Count;

    // Set the active controller to the first leader
    GameData.ActiveController.Set(GameData.Players[firstLeaderIndex]);

    // Log who is leading the first trick
    this.Log(new LogLineContent() {
        Level = ProjectManager.LogLevels.INFO,
        Text = $"<b>{GameData.Players[firstLeaderIndex].Info.Name}</b> will lead the first trick."
    });

    // Start the bidding sequence
    BiddingSequence();
}


private void BiddingSequence() {
    GameData.Status = GameData.GameStatus.Bidding;

    // Use the current bidder index
    Debug.Log($"Starting bidding sequence with {GameData.Players[GameData.CurrentBidderIndex].Info.Name}");
    GameData.ActiveController.Set(GameData.Players[GameData.CurrentBidderIndex]);

    ReferenseManager.Instance.TrumpPickerObject.gameObject.SetActive(
        GameData.ActiveController.Type == ControllerInfo.ControllerType.CLIENT
    );
}

public void EndBidding() {
    ReferenseManager.Instance.TrumpPickerObject.gameObject.SetActive(false);
    RevealKitty();

    // Set game status to playing
    GameData.Status = GameData.GameStatus.Playing;

    // Determine the player to the left of the dealer
    int dealerIndex = GameData.Players.IndexOf(GameData.Dealer);
    int firstPlayerIndex = (dealerIndex + 1) % GameData.Players.Count;

    // Set the first player as the active controller
    GameData.ActiveController.Set(GameData.Players[firstPlayerIndex]);

    this.Log(new LogLineContent() {
        Level = ProjectManager.LogLevels.INFO,
        Text = $"<b>{GameData.Players[firstPlayerIndex].Info.Name}</b> starts the play."
    });

    // Trigger the first turn for the active controller
    GameData.ActiveController.Controller.StartTurn();
}

		private void RevealKitty() {
			foreach (ControllerBase controller in ControllerManager.Instance.Controllers) {
				controller.References.Hand.SetFaceUp(true);
				controller.References.Hand.SortHand();
			}
		}
public void PickTrumpSuit(string input) {
    if (input == "PASS") {
        if (GameData.ActiveController.Controller == GameData.Dealer) {
            // If dealer passes, enforce them to pick a trump suit
            Debug.Log($"Dealer {GameData.Dealer.Info.Name} must pick a trump suit.");
            GameData.SetTrumpSuit(CardDefinitions.CardSuit.CLUB); // Default fallback suit
            EndBidding();
            return;
        }

        // Log the pass
        Debug.Log($"{GameData.ActiveController.Controller.Info.Name} has passed.");
        // this.Log(new LogLineContent() {
        //     Level = ProjectManager.LogLevels.INFO,
        //     Text = $"TrumpSuit PASS",
        //     Controller = GameData.ActiveController.Controller
        // });

        // Show a popup indicating the player passed
        GameManager.Instance.ShowPopup(GameData.ActiveController.Controller, "I'll Pass");

        // Add the current controller to the list of passed players
        GameData.PlayersPassed.Add(GameData.ActiveController.Controller);

        // Move to the next player
        GameData.ActiveController.Next(GameData.GameStatus.Bidding);


        // If the new active controller is the dealer and all others have passed
        if (GameData.ActiveController.Controller == GameData.Dealer) {
            Debug.Log($"Dealer {GameData.Dealer.Info.Name} must pick a trump suit.");
            GameData.SetTrumpSuit(CardDefinitions.CardSuit.CLUB); // Default fallback suit
            EndBidding();
        } else {
            // Continue the bidding sequence with the next player
            Debug.Log($"Continuing bidding sequence with {GameData.ActiveController.Controller.Info.Name}");
            BiddingSequence();
        }
    } else {
        // Player picks a trump suit
        Debug.Log($"{GameData.ActiveController.Controller.Info.Name} picked {input} as the trump suit.");
        GameData.SetTrumpSuit((CardDefinitions.CardSuit)Enum.Parse(typeof(CardDefinitions.CardSuit), input));

        // Log the trump suit choice
        this.Log(new LogLineContent() {
            Level = ProjectManager.LogLevels.INFO,
            Text = $"Picked TrumpSuit <b>{GameData.TrumpSuit}</b>",
            Controller = GameData.ActiveController.Controller
        });

        // Show a popup indicating the chosen trump suit
        GameManager.Instance.ShowPopup(GameData.ActiveController.Controller, $"I pick {GameData.TrumpSuit}");

        // End the bidding sequence
        EndBidding();
    }
}



public void ShowPopup(ControllerBase controller, string message) {
    var anchor = controller.References.PopupAnchor; // Get the anchor for this controller.
    if (anchor == null) {
        Debug.LogWarning($"No PopupAnchor found for {controller.Info.Name}");
        return;
    }

    // Instantiate the popup bubble.
    var popupPrefab = Resources.Load<GameObject>("PopupBubble"); // Assumes the prefab is in a Resources folder.
    var popupInstance = Instantiate(popupPrefab, anchor.position, Quaternion.identity, anchor);
    var popupController = popupInstance.GetComponent<PopupBubbleController>();
    popupController.Setup(message);
}

public bool IsCardValidEntry(Card card) {
    List<Card> cardsInPool = ReferenseManager.Instance.Pool.Cards;

    // Determine the lead suit
    CardDefinitions.CardSuit leadSuit = cardsInPool.Count > 0
        ? cardsInPool[0].Info.Suit // Use the suit of the first card in the pool
        : card.Info.Suit;          // Otherwise, set lead suit to the suit of the first card being played

    Debug.Log($"Trump Suit: {GameData.TrumpSuit}, Lead Suit: {leadSuit}, Player Card: {card.Info.Suit} {card.Info.Rank}");

    // If this is the first card in the trick, any card is valid.
    if (cardsInPool.Count == 0) {
        Debug.Log($"First card played: {card.Info.Suit} {card.Info.Rank}. Setting lead suit to {leadSuit}.");
        return true;
    }

    // Step 1: If the player has at least one card of the lead suit, they must follow suit.
    if (card.Controller.GetCardsBySuit(leadSuit).Count > 0) {
        if (card.Info.Suit != leadSuit) {
            Debug.LogError($"Invalid Card Entry: You must follow the lead suit ({leadSuit}) if you have it!");
            return false;
        }
        Debug.Log($"Valid Card Entry: Player followed the lead suit ({leadSuit}).");
        return true;
    }

    // Step 2: If no lead suit cards are available but the player has trump cards.
    if (card.Controller.GetCardsBySuit(GameData.TrumpSuit).Count > 0) {
        if (card.Info.Suit != GameData.TrumpSuit) {
            Debug.LogError($"Invalid Card Entry: You must play trump ({GameData.TrumpSuit}) if you don't have the lead suit!");
            return false;
        }

        // If a trump card is played, check if it must beat the current highest trump in the pool.
        Card highestTrumpInPool = cardsInPool
            .Where(c => c.Info.Suit == GameData.TrumpSuit)
            .OrderByDescending(c => c.Info.Rank)
            .FirstOrDefault();

        if (highestTrumpInPool != null) {
            bool hasHigherTrump = card.Controller.GetCardsBySuit(GameData.TrumpSuit)
                .Any(c => c.Info.Rank > highestTrumpInPool.Info.Rank);

            if (hasHigherTrump && card.Info.Rank <= highestTrumpInPool.Info.Rank) {
                Debug.LogError($"Invalid Card Entry: You must play a higher trump card than {highestTrumpInPool.Info.Rank} if you can!");
                return false;
            }
        }

        Debug.Log($"Valid Card Entry: Player played trump ({GameData.TrumpSuit}).");
        return true;
    }

    // Step 3: If the player has neither the lead suit nor trump, any card is allowed.
    Debug.Log($"Valid Card Entry: Player has no lead suit or trump; any card is valid.");
    return true;
}







public void OnCardEnterPool() {
    var pool = ReferenseManager.Instance.Pool.Cards;

    if (pool.Count == GameData.PlayerCount) {
        GameData.Status = GameData.GameStatus.Counting;
        GameLoopKit.DoTurnDelay(() => { OnEndRun(); }, 2);
    } else {
        if (pool.Count == 1) { // Only set the lead suit after the first card
            GameData.LeadSuit = pool[0].Info.Suit;
            Debug.Log($"Lead suit set to: {GameData.LeadSuit}");
        }
GameData.ActiveController.Next(GameData.GameStatus.Playing);

    }
}


private int GetCardScore(Card card, CardDefinitions.CardSuit trumpSuit) {
    bool isTrump = (card.Info.Suit == trumpSuit);
    CardDefinitions.CardIndex index = card.Info.Index;

    if (isTrump) {
        // Trump scoring
        switch (index) {
            case CardDefinitions.CardIndex.JACK: return 20;
            case CardDefinitions.CardIndex.NINE: return 14;
            case CardDefinitions.CardIndex.ACE: return 11;
            case CardDefinitions.CardIndex.TEN: return 10;
            case CardDefinitions.CardIndex.KING: return 4;
            case CardDefinitions.CardIndex.QUEEN: return 3;
            // EIGHT, SEVEN, SIX, and any other excluded ranks are worth 0 as trump
            default: return 0;
        }
    } else {
        // Non-trump scoring
        switch (index) {
            case CardDefinitions.CardIndex.ACE: return 11;
            case CardDefinitions.CardIndex.TEN: return 10;
            case CardDefinitions.CardIndex.KING: return 4;
            case CardDefinitions.CardIndex.QUEEN: return 3;
            case CardDefinitions.CardIndex.JACK: return 2;
            // NINE, EIGHT, SEVEN, SIX, etc. are 0 in non-trump
            default: return 0;
        }
    }
}
private void OnEndRun() {
    List<Card> cardsInPool = ReferenseManager.Instance.Pool.Cards;
    CardDefinitions.CardSuit leadSuit = cardsInPool[0].Info.Suit;

    // Separate cards into trump and lead suit groups.
    List<Card> trumpCards = cardsInPool.Where(c => c.Info.Suit == GameData.TrumpSuit).ToList();
    List<Card> leadSuitCards = cardsInPool.Where(c => c.Info.Suit == leadSuit).ToList();

    // Determine the winning card:
    Card winningCard;
    if (trumpCards.Count > 0) {
        // If trump cards are present, use the custom trump order
        winningCard = trumpCards.OrderByDescending(c => GetTrumpRank(c)).First();
    } else {
        // Otherwise, the highest-ranked lead suit card wins
        winningCard = leadSuitCards.OrderByDescending(c => c.Info.Rank).First();
    }

    // The player who won the trick leads the next trick.
    ControllerBase winningController = winningCard.Controller;
    GameData.ActiveController.Set(winningController);

    // Sum points from all cards played in this trick.
    int totalScore = cardsInPool.Sum(c => GetCardScore(c, GameData.TrumpSuit));
    winningController.CurrentScore += totalScore;

    // Award points for the last card (assuming it's worth extra points, e.g., 10).
    if (IsLastTrick()) {
        totalScore += 10; // Adjust value if "last card" bonus differs.
        Debug.Log($"Last card bonus added. Total score for this trick: {totalScore}");
    }

    // Display a popup for the winning player
    string popupMessage = $"I Won the Trick! {totalScore} Points";
    GameManager.Instance.ShowPopup(winningController, popupMessage);

    // Log the result
    this.Log(new LogLineContent() {
        Level = ProjectManager.LogLevels.ANNOUNCEMENT,
        Text = $"<b>{winningController.Info.Name}</b> won the trick and receives <color=#00BB00>{totalScore}</color> points."
    });

    GameData.LeadSuit = null;

    // Update scores and clear the pool
    UIManager.UpdateScores();
    ReferenseManager.Instance.Pool.Clear();

    // Check if the round or game ends
    CheckForVictory();
    CheckForEndOfRound();
}

private int GetTrumpRank(Card card) {
    if (card.Info.Suit != GameData.TrumpSuit) {
        return -1; // Non-trump cards have the lowest priority
    }

    return TrumpOrder.TryGetValue(card.Info.Index, out int rank) ? rank : -1;
}

private void HighlightDealer(ControllerBase dealer) {
    foreach (var controller in GameData.Players) {
        var poolSlot = controller.References.PoolSlot;
        if (poolSlot == null) continue;

        // Highlight the dealer's pool slot
        if (controller == dealer) {
            poolSlot.Highlight(true);
        } else {
            poolSlot.Highlight(false);
        }
    }
}

public void ResetGame()
{
    // Reset GameData
    GameData = new GameData();
    
    // Clear Controllers
    var controllerManager = ControllerManager.Instance;
    controllerManager.Controllers.Clear();

    // Reset Deck, Pool, and related managers
    DeckManager.Instance.ResetDeck();
    ReferenseManager.Instance.Pool.Clear();

    // Reset UI to initial menu state (if needed)
    UIManager.Instance.ResetUI();
}



	private bool IsLastTrick() {
    // Return true if all players have no cards left after this trick
    return GameData.Players.All(player => player.References.Hand.Cards.Count == 0);
    }

		private void CheckForEndOfRound() {
			foreach (ControllerBase controller in ControllerManager.Instance.Controllers) {
				if (controller.References.Hand.Cards.Count > 0) {
					GameData.Status = GameData.GameStatus.Playing;
					return;
				}
			}

			GameData.Status = GameData.GameStatus.EndRound;
			OnEndRound();
		}

		//? Once everyone is out of cards and the total sum needs to be counted

internal void OnEndRound() {
    // Sum up the scores for each team
    int biddingTeamScore = 0;
    int opposingTeamScore = 0;
    // Clear passed players for a fresh start
    GameData.PlayersPassed.Clear();
    // Determine the bidding team and opposing team
    TeamController biddingTeam = GameData.BiddingTeam;
    TeamController opposingTeam = GameData.Teams.FirstOrDefault(t => t != biddingTeam);

    // Add last trick bonus if applicable
    if (IsLastTrick()) {
        // Add 10 points for the last trick to the winning team
        ControllerBase lastTrickWinner = GameData.ActiveController.Controller;
        if (biddingTeam.Members.Contains(lastTrickWinner)) {
            biddingTeamScore += 10;
            Debug.Log("Last trick bonus added to bidding team.");
        } else if (opposingTeam.Members.Contains(lastTrickWinner)) {
            opposingTeamScore += 10;
            Debug.Log("Last trick bonus added to opposing team.");
        }
    }

    // Sum up the scores for teams
    if (GameData.Teams.Count > 0) {
        biddingTeamScore += biddingTeam.Members.Sum(p => p.CurrentScore);
        opposingTeamScore += opposingTeam.Members.Sum(p => p.CurrentScore);
    } else {
        // Individual play logic
        ControllerBase biddingPlayer = ControllerManager.Instance.Controllers.FirstOrDefault(c => c.Info == GameData.TrumpController);
        biddingTeamScore += biddingPlayer.CurrentScore;
        opposingTeamScore += ControllerManager.Instance.Controllers.Where(c => c != biddingPlayer).Sum(c => c.CurrentScore);
    }

    // Debugging output
    int totalPoints = biddingTeamScore + opposingTeamScore;
    this.Log(new LogLineContent() {
        Level = ProjectManager.LogLevels.DEBUG,
        Text = $"Debug: Bidding Team Score: {biddingTeamScore}, Opposing Team Score: {opposingTeamScore}, Total Points: {totalPoints}"
    });

    // Ensure the total points sum up to 162
    if (totalPoints != 162) {
        this.Log(new LogLineContent() {
            Level = ProjectManager.LogLevels.VERBOSE,
            Text = $"Error: Total points for the hand is {totalPoints}, which is not equal to 162."
        });
        // Handle the discrepancy (optional adjustment)
    }

    // Check if the bidding team met the minimum required score (82 points)
    if (biddingTeamScore < 82) {
        // Bidding team is bate
        biddingTeam.GameScores.Add(0);
        opposingTeam.GameScores.Add(162);

        // Update individual players' GameScores
        foreach (ControllerBase player in biddingTeam.Members) {
            player.GameScores.Add(0);
        }
        foreach (ControllerBase player in opposingTeam.Members) {
            player.GameScores.Add(162 / opposingTeam.Members.Count);
        }

        this.Log(new LogLineContent() {
            Level = ProjectManager.LogLevels.ANNOUNCEMENT,
            Text = $"Bidding team is bate. All 162 points awarded to the opposing team."
        });
    } else {
        // Normal scoring
        biddingTeam.GameScores.Add(biddingTeamScore);
        opposingTeam.GameScores.Add(opposingTeamScore);

        // Update individual players' GameScores
        foreach (ControllerBase player in biddingTeam.Members) {
            player.GameScores.Add(player.CurrentScore);
        }
        foreach (ControllerBase player in opposingTeam.Members) {
            player.GameScores.Add(player.CurrentScore);
        }

        this.Log(new LogLineContent() {
            Level = ProjectManager.LogLevels.ANNOUNCEMENT,
            Text = $"Bidding team scored {biddingTeamScore} points. Opposing team scored {opposingTeamScore} points."
        });
    }

    // Reset CurrentScores and other properties for the next round
    foreach (ControllerBase player in GameData.Players) {
        player.CurrentScore = 0;
        player.HasAnnouncedRun = false;
        player.HasShownRun = false;
        player.AnnouncedRuns.Clear();
    }

    UIManager.UpdateScores();
    // Call victory check after scores are updated.
    CheckForVictory();
    // Check for game winner
    List<TeamController> gameWinningTeams = GameData.Teams.Where(t => t.TotalGameScore >= RuleDefinitions.TargetScore).ToList();
    int dealerIndex = GameData.Players.IndexOf(GameData.Dealer);
    dealerIndex = (dealerIndex + 1) % GameData.Players.Count;
    GameData.Dealer = GameData.Players[dealerIndex];

    // Highlight the new dealer
HighlightDealer(GameData.Dealer);

    if (gameWinningTeams.Count > 0) {
        // Announce the winning team
        TeamController winningTeam = gameWinningTeams.OrderByDescending(t => t.TotalGameScore).First();
        AnnounceGameWinner(winningTeam);
    } else {
        // No team has reached the target score, start the next round
        StartRound();
    }
}


private void AnnounceGameWinner(TeamController winningTeam) {
    string playerNames = string.Join(" and ", winningTeam.Members.Select(m => $"<b>{m.Info.Name}</b>"));

    this.Log(new LogLineContent() {
        Level = ProjectManager.LogLevels.ANNOUNCEMENT,
        Text = $"{playerNames} won the game with <color=#00BB00><b>{winningTeam.TotalGameScore}</b></color> points!",
    });

    // Determine if local player is part of the winning team
    var localPlayer = ReferenseManager.Instance.LocalPlayer;
    bool localPlayerWon = (winningTeam.Members.Contains(localPlayer));

    // Now that the match is definitively over, call OnMatchEnd
    SessionManager.Instance.OnMatchEnd(localPlayerWon);

    EndGame($"Winners: {playerNames}");
}


private bool CheckForVictory() {
    var localPlayer = ReferenseManager.Instance.LocalPlayer;
    if (localPlayer == null) {
        Debug.LogWarning("Local player not found.");
        return false;
    }

    // Check if the local player's team has reached the target score.
    if (GameManager.Instance.GameData.Teams.Count > 0) {
        TeamController localPlayerTeam = GameManager.Instance.GameData.Teams
            .FirstOrDefault(team => team.Members.Contains(localPlayer));

        if (localPlayerTeam != null) {
	if (localPlayerTeam.TotalGameScore >= RuleDefinitions.TargetScore) {
		Debug.Log($"Congratulations! Your team has won with a total score of {localPlayerTeam.TotalGameScore}!");
		GameManager.Instance.AnnounceGameWinner(localPlayerTeam);
		return true;
	} else {
		Debug.Log($"Your team has not yet won. Current Score: {localPlayerTeam.TotalGameScore}");
		return false;
	}

        }
    }

    // Individual play: check the player's score directly.
    if (localPlayer.TotalGameScore >= RuleDefinitions.TargetScore) {
        Debug.Log($"Congratulations! {localPlayer.Info.Name} (you) have won the game!");
        GameManager.Instance.AnnounceGameWinner(null); // Pass `null` for individual wins.
        return true;
    } else {
        Debug.Log($"You have not yet won. Current Score: {localPlayer.TotalGameScore}");
        return false;
    }
}



		public void EndGame(string reason) {
			// this.LogMessage($"Game End\nReason: {reason}");
			this.Log(new LogLineContent() {
				Level = ProjectManager.LogLevels.MESSAGE,
				Text = $"Game End\nReason: {reason}", 
			});
			this.LogDebug("GameManager.EndGame() - not implimented yet\n<color=#ff8800>Impliment reward system payouts here");
		}

		
		private void OnValidate() {
			TestingControls.Validate();
		}

			private void CollectAndCompareRuns() {
		Dictionary<ControllerBase, List<List<Card>>> playerRuns = new Dictionary<ControllerBase, List<List<Card>>>();

		// Collect runs from all players who have announced runs
		foreach (ControllerBase player in GameData.Players) {
			if (player.HasAnnouncedRun && player.AnnouncedRuns.Count > 0) {
				playerRuns.Add(player, player.AnnouncedRuns);
			}
		}

		// Compare runs and determine the best run
		ControllerBase bestPlayer = DetermineBestRun(playerRuns);

		if (bestPlayer != null) {
			// Calculate total points for all runs the best player has
			int totalPoints = 0;
			foreach (var run in playerRuns[bestPlayer]) {
				totalPoints += CalculateRunPoints(run);
			}

			// Award points to the best player
			bestPlayer.CurrentScore += totalPoints;

			// Log and update UI
			this.Log(new LogLineContent() {
				Level = ProjectManager.LogLevels.ANNOUNCEMENT,
				Text = $"<b>{bestPlayer.Info.Name}</b> has the best run(s) and gains <color=green>{totalPoints}</color> points."
			});
			UIManager.UpdateScores();
		} else {
			// Runs cancel each other out
			this.Log(new LogLineContent() {
				Level = ProjectManager.LogLevels.INFO,
				Text = $"Runs cancel each other out. No runs are counted this hand."
			});
		}
	}

		private ControllerBase DetermineBestRun(Dictionary<ControllerBase, List<List<Card>>> playerRuns) {
		if (playerRuns.Count == 0) {
			return null;
		}

		// For each player, get their best run
		Dictionary<ControllerBase, List<Card>> bestRuns = new Dictionary<ControllerBase, List<Card>>();
		foreach (var pair in playerRuns) {
			List<Card> bestRun = pair.Value.OrderByDescending(run => run.Count)
				.ThenByDescending(run => run.Last().Info.Rank)
				.First();
			bestRuns.Add(pair.Key, bestRun);
		}

		// Find the highest run length
		int highestRunLength = bestRuns.Values.Max(run => run.Count);

		// Players with the highest run length
		var playersWithHighestRun = bestRuns.Where(pair => pair.Value.Count == highestRunLength).ToList();

		if (playersWithHighestRun.Count == 1) {
			// Only one player has the highest run length
			return playersWithHighestRun[0].Key;
		} else {
			// Compare the highest cards in the runs
			int highestRank = playersWithHighestRun.Max(pair => (int)pair.Value.Last().Info.Rank);

			var playersWithHighestRank = playersWithHighestRun
				.Where(pair => (int)pair.Value.Last().Info.Rank == highestRank).ToList();

			if (playersWithHighestRank.Count == 1) {
				return playersWithHighestRank[0].Key;
			} else {
				// Check if any of the runs is in trumps
				var trumps = playersWithHighestRank
					.Where(pair => pair.Value[0].Info.Suit == GameData.TrumpSuit).ToList();
				if (trumps.Count == 1) {
					return trumps[0].Key;
				} else {
					// Runs cancel each other out
					return null;
				}
			}
		}
	}

	private int CalculateRunPoints(List<Card> run) {
		if (run.Count >= 4) {
			return 50; // A "fifty" run
		} else if (run.Count == 3) {
			return 20; // A "twenty" run
		}
		return 0;
	}

    private static readonly Dictionary<CardDefinitions.CardIndex, int> TrumpOrder = new Dictionary<CardDefinitions.CardIndex, int>() {
    { CardDefinitions.CardIndex.JACK, 8 },
    { CardDefinitions.CardIndex.NINE, 7 },
    { CardDefinitions.CardIndex.ACE, 6 },
    { CardDefinitions.CardIndex.TEN, 5 },
    { CardDefinitions.CardIndex.KING, 4 },
    { CardDefinitions.CardIndex.QUEEN, 3 },
    { CardDefinitions.CardIndex.EIGHT, 2 },
    { CardDefinitions.CardIndex.SEVEN, 1 },
    { CardDefinitions.CardIndex.SIX, 0 }
};



	}
    

	[System.Serializable] public class GameTestingControls {
		[SerializeField] private bool _initStartGame = false;
		[SerializeField] private bool _startGame = false;
		[SerializeField] private bool _onEndRound = false;

		[Header("Turn Settings")]
		[Tooltip("The client always takes the first turn after the trump is selected")]
		public bool ClientStartTurn = false;
		[Range(0, 10)] public float ControllerTurnDelayMultiplier = 1;

		[Header("Set Active Controller")]
		[SerializeField] private int _controllerIndex = 0;
		[SerializeField] private bool _setController = false;

		//? has the game manager already called start?
		//- if not, ignore the upcomming validate to prevent initializing too soon
		private bool _gameManagerCallesStart = false;

		internal void Validate(bool onStart = false) {
			if (ProjectManager.RELEASE) {
				_initStartGame = false;
				_startGame = false;
				_onEndRound = false;
				ClientStartTurn = false;
				ControllerTurnDelayMultiplier = 1;
				_setController = false;
				return;
			}

			if (!_gameManagerCallesStart) _gameManagerCallesStart = onStart;

			if (Application.isPlaying && _gameManagerCallesStart) {
				if (_initStartGame) {
					_initStartGame = false;
					_startGame = false; //? Becuase initStart already calls start game
					GameManager.Instance.InitializeStart();
				}
				if (_startGame) {
					_startGame = false;
					GameManager.Instance.StartGame();
				}
				if (_onEndRound) {
					_onEndRound = false;
					GameManager.Instance.OnEndRound();
				}
			}


			if (_setController) {
				_setController = false;
				_controllerIndex = Math.Clamp(_controllerIndex, 0, ControllerManager.Instance.Controllers.Count - 1);
				GameManager.Instance.GameData.ActiveController.Set(ControllerManager.Instance.Controllers[_controllerIndex]);
			}
		}
	}
}