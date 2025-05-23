using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tarabish.UI;
using Tarabish.UI.Menu;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using Unity.Services.Friends;
using Unity.Services.Friends.Exceptions;
using Unity.Services.Friends.Models;
using Unity.Services.Friends.Notifications;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Tarabish.Netcode {
	public class SessionManager : MonoBehaviour
	{
		public LeaderboardEntryUI leaderboardEntryUI;
		public static SessionManager Instance;
	        public Menu_Title menuTitle; // Assign the Menu_Title script in the Inspector
	      //This gameObject reference is only needed to get the IRelationshipUIController component from it.
        [Tooltip("Reference a GameObject that has a component extending from IRelationshipsUIController."), SerializeField]
        GameObject m_RelationshipsViewGameObject;

        IRelationshipsView m_RelationshipsView;

        List<FriendsEntryData> m_FriendsEntryDatas = new List<FriendsEntryData>();
        List<PlayerProfile> m_RequestsEntryDatas = new List<PlayerProfile>();
        List<PlayerProfile> m_BlockEntryDatas = new List<PlayerProfile>();

        ILocalPlayerView m_LocalPlayerView;
        IAddFriendView m_AddFriendView;
        IFriendsListView m_FriendsListView;
        IRequestListView m_RequestListView;
        IBlockedListView m_BlockListView;

        public PlayerProfile m_LoggedPlayerProfile;

		public Button m_LoginBtn;
		public TextMeshProUGUI m_StatusText;

        private FriendsEventConnectionState m_current_state;

		public GameObject m_EndOfMatchPanel;
		public TextMeshProUGUI m_TrophiesEOM, m_XPEOM;
		public Button m_FinishMatch;
		
	    async void Awake()
	    {
			Instance = this;
	        try
	        {
	            await UnityServices.InitializeAsync();
	            Debug.Log(UnityServices.State);
	        }
	        catch (Exception e)
	        {
	            Debug.LogException(e);
	        }
	
	        SetupEvents();
	       // m_SignInGuest.onClick.AddListener(async () => await SignInAnonymouslyAsync());
	    }
	
	    // Setup authentication event handlers if desired
	    void SetupEvents() 
	    {
	        AuthenticationService.Instance.SignedIn += async () => 
	        {
	            // Shows how to get a playerID
	            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
	
	            // Shows how to get an access token
	            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

				await InIt();
	        };
	
	        AuthenticationService.Instance.SignInFailed += (err) => 
	        {
	            Debug.LogError(err);
	        };
	
	        AuthenticationService.Instance.SignedOut += () => 
	        {
	            Debug.Log("Player signed out.");
	        };
	
	        AuthenticationService.Instance.Expired += () =>
	        {
	            Debug.Log("Player session could not be refreshed and expired.");
	        };
	    }

		async Task InIt(){
				RegisterFriendsEventCallbacks(); 
					m_StatusText.text = "Getting Friends..";
				await FriendsService.Instance.InitializeAsync();
				UIInit();
				await LogInAsync();
				RefreshAll();
		}

		async Task LogInAsync()
        {
			m_StatusText.text = "Loading Player Profile..";
            var playerID = AuthenticationService.Instance.PlayerId;
            var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            m_LoggedPlayerProfile = new PlayerProfile(playerName, playerID);

            await SetPresence(Availability.Online, "In Friends Menu");

			    // Load player profile data from Cloud Save
    await LoadPlayerProfile(m_LoggedPlayerProfile);

            m_LocalPlayerView.Refresh(
                m_LoggedPlayerProfile.Name,
                "In Friends Menu",
                Availability.Online);
            RefreshAll();
            Debug.Log($"Logged in as {m_LoggedPlayerProfile}");
        }

		async Task SetPresence(Availability presenceAvailabilityOptions,
            string activityStatus = "")
        {
            var activity = new Activity { Status = activityStatus };
            try
            {
                await FriendsService.Instance.SetPresenceAsync(presenceAvailabilityOptions, activity);
                Debug.Log($"Availability changed to {presenceAvailabilityOptions}.");
            }
            catch (FriendsServiceException e)
            {
                Debug.Log($"Failed to set the presence to {presenceAvailabilityOptions} - {e}");
            }
        }
	
public async Task SignInAnonymouslyAsync()
{

	m_LoginBtn.gameObject.SetActive(false);
	m_StatusText.gameObject.SetActive(true);
	m_StatusText.text = "Signing In...";
    try 
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Sign in anonymously succeeded!");

        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
		
            // Otherwise, switch to Title menu

    } 
    catch (AuthenticationException ex)
    {
        Debug.LogException(ex);
			m_LoginBtn.gameObject.SetActive(true);
    } 
    catch (RequestFailedException ex)
    {
        Debug.LogException(ex);
			m_LoginBtn.gameObject.SetActive(true);
    }
}

public async Task ChangeNameAsync(string newName)
{
    if (string.IsNullOrWhiteSpace(newName))
    {
        Debug.LogError("Name cannot be empty.");
        return;
    }

    try
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
        Debug.Log($"Player name changed to {newName}.");

        // Update the local profile and UI
        //m_LoggedPlayerProfile.Name = newName;
        m_LocalPlayerView.Refresh(newName, "In Friends Menu", Availability.Online);
    }
    catch (RequestFailedException e)
    {
        Debug.LogError($"Failed to change name: {e.Message}");
    }
}

void Update(){
	    // Check if key "1" is pressed
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
        AwardXP(50); // Award 50 XP as an example
    }
}

public void FinishMatchUI(){
	m_EndOfMatchPanel.SetActive(false);
	CanvasManager.Instance.SwitchCanvas(MenuType.Title);
	GameManager.Instance.ResetGame();
	ControllerManager.Instance.ResetControllers();

}

	#region  Friends
	

	        void UIInit()
        {
            if (m_RelationshipsViewGameObject == null)
            {
                Debug.LogError($"Missing GameObject in {name}", gameObject);
                return;
            }

            m_RelationshipsView = m_RelationshipsViewGameObject.GetComponent<IRelationshipsView>();
            if (m_RelationshipsView == null)
            {
                Debug.LogError($"No Component extending IRelationshipsView {m_RelationshipsViewGameObject.name}",
                    m_RelationshipsViewGameObject);
                return;
            }

            m_RelationshipsView.Init();
            m_LocalPlayerView = m_RelationshipsView.LocalPlayerView;
            m_AddFriendView = m_RelationshipsView.AddFriendView;

            //Bind Lists
            m_FriendsListView = m_RelationshipsView.FriendsListView;
            m_FriendsListView.BindList(m_FriendsEntryDatas);
            m_RequestListView = m_RelationshipsView.RequestListView;
            m_RequestListView.BindList(m_RequestsEntryDatas);
            m_BlockListView = m_RelationshipsView.BlockListView;
            m_BlockListView.BindList(m_BlockEntryDatas);

            //Bind Friends SDK Callbacks
            m_AddFriendView.onFriendRequestSent += AddFriendAsync;
            m_FriendsListView.onRemove += RemoveFriendAsync;
            m_FriendsListView.onBlock += BlockFriendAsync;
            m_RequestListView.onAccept += AcceptRequestAsync;
            m_RequestListView.onDecline += DeclineRequestAsync;
            m_RequestListView.onBlock += BlockFriendAsync;
            m_BlockListView.onUnblock += UnblockFriendAsync;
            m_LocalPlayerView.onPresenceChanged += SetPresenceAsync;
        }

	    void RegisterFriendsEventCallbacks()
        {
            try
            {
                FriendsService.Instance.RelationshipAdded += e =>
                {
                    RefreshRequests();
                    RefreshFriends();
                    Debug.Log($"create {e.Relationship} EventReceived");
                };
                FriendsService.Instance.MessageReceived += e =>
                {
                    RefreshRequests();
                    Debug.Log("MessageReceived EventReceived");
                };
                FriendsService.Instance.PresenceUpdated += e =>
                {
                    RefreshFriends();
                    Debug.Log("PresenceUpdated EventReceived");
                };
                FriendsService.Instance.RelationshipDeleted += e =>
                {
                    RefreshFriends();
                    Debug.Log($"Delete {e.Relationship} EventReceived");
                };
            }
            catch (FriendsServiceException e)
            {
                Debug.Log(
                    "An error occurred while performing the action. HttpCode: " + e.StatusCode + ", FriendsErrorCode: " + e.ErrorCode +  ", Message: " + e.Message);
            }
        }
		
	    void RefreshAll()
        {
            RefreshFriends();
            RefreshRequests();
            RefreshBlocks();
        }
        async void BlockFriendAsync(string id)
        {
            await BlockFriend(id);
            RefreshAll();
        }

        async void UnblockFriendAsync(string id)
        {
            await UnblockFriend(id);
            RefreshBlocks();
            RefreshFriends();
        }

        async void RemoveFriendAsync(string id)
        {
            await RemoveFriend(id);
            RefreshFriends();
        }

        async void AcceptRequestAsync(string name)
        {
            await AcceptRequest(name);
            RefreshRequests();
            RefreshFriends();
        }

        async void DeclineRequestAsync(string id)
        {
            await DeclineRequest(id);
            RefreshRequests();
        }

        async void SetPresenceAsync((Availability presence, string activity) status)
        {
            await SetPresence(status.presence, status.activity);
            m_LocalPlayerView.Refresh(m_LoggedPlayerProfile.Name, status.activity, status.presence);
        }

        async void AddFriendAsync(string name)
        {
            var success = await SendFriendRequest(name);
            if (success)
            {
                m_AddFriendView.FriendRequestSuccess();
                //If the added friend has also requested friendship, he is already a friend, just refresh the views.
                if (m_RequestsEntryDatas.Find(entry => entry.Name == name) != null)
                    RefreshAll();
            }
            else
            {
                m_AddFriendView.FriendRequestFailed();
            }
        }

        void RefreshFriends()
        {
            m_FriendsEntryDatas.Clear();

            var friends = GetFriends();

            foreach (var friend in friends)
            {
                string activityText;
                if (friend.Presence.Availability == Availability.Offline ||
                    friend.Presence.Availability == Availability.Invisible)
                {
                    activityText = friend.Presence.LastSeen.ToShortDateString() + " " +
                                   friend.Presence.LastSeen.ToLongTimeString();
                }
                else
                {
                    activityText = friend.Presence.GetActivity<Activity>() == null
                        ? ""
                        : friend.Presence.GetActivity<Activity>().Status;
                }

                var info = new FriendsEntryData
                {
                    Name = friend.Profile.Name,
                    Id = friend.Id,
                    Availability = friend.Presence.Availability,
                    Activity = activityText
                };
                m_FriendsEntryDatas.Add(info);
            }

            m_RelationshipsView.RelationshipBarView.Refresh();
        }

        void RefreshRequests()
        {
            m_RequestsEntryDatas.Clear();
            var requests = GetRequests();

            foreach (var request in requests)
                m_RequestsEntryDatas.Add(new PlayerProfile(request.Profile.Name, request.Id));

            m_RelationshipsView.RelationshipBarView.Refresh();
        }

        void RefreshBlocks()
        {
            m_BlockEntryDatas.Clear();

            foreach (var block in FriendsService.Instance.Blocks)
                m_BlockEntryDatas.Add(new PlayerProfile(block.Member.Profile.Name, block.Member.Id));

            m_RelationshipsView.RelationshipBarView.Refresh();
        }
        
        async Task<bool> SendFriendRequest(string playerName)
        {
            try
            {
                //We add the friend by name in this sample but you can also add a friend by ID using AddFriendAsync
                var relationship = await FriendsService.Instance.AddFriendByNameAsync(playerName);
                Debug.Log($"Friend request sent to {playerName}.");
                //If both players send friend request to each other, their relationship is changed to Friend.
                return relationship.Type is RelationshipType.FriendRequest or RelationshipType.Friend;
            }
            catch (FriendsServiceException e)
            {
                Debug.Log($"Failed to Request {playerName} - {e}.");
                return false;
            }
        }

        async Task RemoveFriend(string playerId)
        {
            try
            {
                await FriendsService.Instance.DeleteFriendAsync(playerId);
                Debug.Log($"{playerId} was removed from the friends list.");
            }
            catch (FriendsServiceException e)
            {
                Debug.Log($"Failed to remove {playerId}. - {e}");
            }
        }

        async Task BlockFriend(string playerId)
        {
            try
            {
                await FriendsService.Instance.AddBlockAsync(playerId);
                Debug.Log($"{playerId} was blocked.");
            }
            catch (FriendsServiceException e)
            {
                Debug.Log($"Failed to block {playerId}. - {e}");
            }
        }

        async Task UnblockFriend(string playerId)
        {
            try
            {
                await FriendsService.Instance.DeleteBlockAsync(playerId);
                Debug.Log($"{playerId} was unblocked.");
            }
            catch (FriendsServiceException e)
            {
                Debug.Log($"Failed to unblock {playerId} - {e}.");
            }
        }

        async Task AcceptRequest(string playerName)
        {
            try
            {
                await SendFriendRequest(playerName);
                Debug.Log($"Friend request from {playerName} was accepted.");
            }
            catch (FriendsServiceException e)
            {
                Debug.Log($"Failed to accept request from {playerName}. - {e}");
            }
        }

        async Task DeclineRequest(string playerId)
        {
            try
            {
                await FriendsService.Instance.DeleteIncomingFriendRequestAsync(playerId);
                Debug.Log($"Friend request from {playerId} was declined.");
            }
            catch (FriendsServiceException e)
            {
                Debug.Log($"Failed to decline request from {playerId}. - {e}");
            }
        }

        /// <summary>
        /// Get an amount of friends (including presence data).
        /// </summary>
        /// <returns>List of friends.</returns>
        List<Member> GetFriends()
        {
            return GetNonBlockedMembers(FriendsService.Instance.Friends);
        }

        /// <summary>
        /// Get an amount of Requests. The friends SDK maintains relationships unless explicitly deleted, even those
        /// towards blocked players. We don't want to show blocked players' requests, so we filter them out.
        /// </summary>
        /// <returns>List of players.</returns>
        List<Member> GetRequests()
        {
            return GetNonBlockedMembers(FriendsService.Instance.IncomingFriendRequests);
        }



        /// <summary>
        /// Returns a list of members that are not blocked by the active user.
        /// </summary>
        /// <param name="relationships">The list of relationships to filter.</param>
        /// <returns>Filtered list of members.</returns>
        private List<Member> GetNonBlockedMembers(IReadOnlyList<Relationship> relationships)
        {
            var blocks = FriendsService.Instance.Blocks;
            return relationships
                   .Where(relationship =>
                       !blocks.Any(blockedRelationship => blockedRelationship.Member.Id == relationship.Member.Id))
                   .Select(relationship => relationship.Member)
                   .ToList();
        }

		
	#endregion

#region  Save System
public async Task SavePlayerProfile(PlayerProfile profile)
{
    var data = new Dictionary<string, object>
    {
        { "playerLevel", profile.Level },
        { "playerXP", profile.XP },
        { "playerGold", profile.Gold },
        { "playerWins", profile.Wins },
        { "playerLosses", profile.Losses },
		{ "playerTrophies", profile.Trophies } // Save Trophies
    };

    await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    Debug.Log("Player profile saved.");
}


public async Task LoadPlayerProfile(PlayerProfile profile)
{
    // The new Player.LoadAllAsync() returns Dictionary<string, Item>
    Dictionary<string, Item> data = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

    // Extract values using GetAsString() 
    string xpStr = data.ContainsKey("playerXP") ? data["playerXP"].Value.GetAsString() : "0";
    string goldStr = data.ContainsKey("playerGold") ? data["playerGold"].Value.GetAsString() : "0";
    string levelStr = data.ContainsKey("playerLevel") ? data["playerLevel"].Value.GetAsString() : "1";
    string trophiesStr = data.ContainsKey("playerTrophies") ? data["playerTrophies"].Value.GetAsString() : "0";

    string winsStr = data.ContainsKey("playerWins") ? data["playerWins"].Value.GetAsString() : "1";
    string lossesStr = data.ContainsKey("playerLosses") ? data["playerLosses"].Value.GetAsString() : "0";
    // Parse integers from the retrieved strings
    int xp = int.TryParse(xpStr, out int xpValue) ? xpValue : 0;
    int gold = int.TryParse(goldStr, out int goldValue) ? goldValue : 0;
    int loadedLevel = int.TryParse(levelStr, out int levelValue) ? levelValue : 1;
    int trophies = int.TryParse(trophiesStr, out int trophiesValue) ? trophiesValue : 0;

	int wins = int.TryParse(winsStr, out int winsValue) ? winsValue : 0;
	int losses = int.TryParse(lossesStr, out int lossesValue) ? lossesValue : 0;

    profile.AddXP(xp);
    profile.EarnGold(gold);
    for (int i = profile.Level; i < loadedLevel; i++)
    {
        profile.AddXP(profile.Level * 100);
    }

	profile.AddWins(winsValue);
	profile.AddLosses(lossesValue);
    // Directly set trophies by adjusting difference
    int trophyDifference = trophies - profile.Trophies;
    if (trophyDifference > 0)
        profile.AddTrophies(trophyDifference);
    else if (trophyDifference < 0)
        profile.RemoveTrophies(Mathf.Abs(trophyDifference));

    Debug.Log($"Loaded Player Profile: {profile}");

    CanvasManager.Instance.SwitchCanvas(MenuType.Title);
}




public void AwardXP(int amount)
{
    if (m_LoggedPlayerProfile == null)
    {
        Debug.LogError("Player profile is not initialized.");
        return;
    }

    m_LoggedPlayerProfile.AddXP(amount);
           // Update UI
            menuTitle.UpdateXPUI();
    // Save the updated profile to Cloud Save
    SavePlayerProfile(m_LoggedPlayerProfile);

    Debug.Log($"Awarded {amount} XP. Current Level: {m_LoggedPlayerProfile.Level}, XP: {m_LoggedPlayerProfile.XP}");
}

public void OnMatchEnd(bool isWin)
{
    m_EndOfMatchPanel.SetActive(true);
    
    int trophyChange = 0;
    int xpChange = 0;

    if (isWin)
    {
        m_LoggedPlayerProfile.IncrementWins();
        xpChange = 50;
        trophyChange = 6; // Player gains 6 trophies on win
        m_LoggedPlayerProfile.AddTrophies(trophyChange);
    }
    else
    {
        m_LoggedPlayerProfile.IncrementLosses();
        xpChange = 25;
        trophyChange = -2; // Player loses 2 trophies on loss, but can't go below 0
        m_LoggedPlayerProfile.RemoveTrophies(Mathf.Abs(trophyChange));
    }

    // Award XP after win/loss adjustments
    AwardXP(xpChange);
  // Update Leaderboard with new trophy count
    UpdateTrophiesLeaderboardAsync(m_LoggedPlayerProfile.Trophies);
            // Update UI based on actual trophy and XP changes
            menuTitle.UpdateXPUI();
            menuTitle.UpdateTrophyUI();
    // Update UI based on actual trophy and XP changes
    // If loss: trophyChange is negative, show "-2"
    // If win: trophyChange is positive, show "+6"
    m_TrophiesEOM.text = (trophyChange > 0) ? $"+{trophyChange}" : $"{trophyChange}";
    m_XPEOM.text = $"+{xpChange}";

    // Save updated profile
    SavePlayerProfile(m_LoggedPlayerProfile);
}

#endregion

#region Leaderboards
        private const string LeaderboardId = "wins-leaderboard"; // Update with your leaderboard ID
        private int LeaderboardOffset = 0;
        private int LeaderboardLimit = 10;
        /// <summary>
        /// Updates the player's score on the Wins leaderboard.
        /// </summary>
        public async Task UpdateWinsLeaderboardAsync(int wins)
        {
            try
            {
                var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, wins);
                Debug.Log($"Leaderboard updated: {JsonConvert.SerializeObject(scoreResponse)}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to update leaderboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches and logs the top scores from the Wins leaderboard.
        /// </summary>
        public async Task<List<LeaderboardEntry>> GetTopScoresAsync()
        {
            try
            {
                var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId,
                    new GetScoresOptions { Offset = LeaderboardOffset, Limit = LeaderboardLimit });

                Debug.Log($"Leaderboard scores: {JsonConvert.SerializeObject(scoresResponse.Results)}");
                return scoresResponse.Results;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to fetch leaderboard scores: {ex.Message}");
                return new List<LeaderboardEntry>();
            }
        }

        /// <summary>
        /// Fetches and logs the local player's score from the Wins leaderboard.
        /// </summary>
        public async Task<LeaderboardEntry> GetPlayerScoreAsync()
        {
            try
            {
                var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardId);
                Debug.Log($"Player score: {JsonConvert.SerializeObject(scoreResponse)}");
                return scoreResponse;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to fetch player score: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Logs the leaderboard scores for a specific version ID.
        /// </summary>
        public async Task GetVersionScoresAsync(string versionId)
        {
            try
            {
                var versionScoresResponse = await LeaderboardsService.Instance.GetVersionScoresAsync(LeaderboardId, versionId);
                Debug.Log($"Version scores: {JsonConvert.SerializeObject(versionScoresResponse.Results)}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to fetch version scores: {ex.Message}");
            }
        }

#endregion
#region Leaderboards - Trophies
private const string TrophiesLeaderboardId = "trophies-leaderboard"; // Update with your leaderboard ID
public Transform leaderboardPanel;
public void OnLeaderboardButtonClicked()
{
    StartCoroutine(DisplayLeaderboardCoroutine());
}

private IEnumerator DisplayLeaderboardCoroutine()
{
    // Ensure the UI is ready before updating
    yield return new WaitForEndOfFrame();

    UpdateTrophiesLeaderboardUI(leaderboardPanel);
}


public async Task UpdateTrophiesLeaderboardAsync(int trophies)
{
    try
    {
        var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(TrophiesLeaderboardId, trophies);
        Debug.Log($"Trophies leaderboard updated: {JsonConvert.SerializeObject(scoreResponse)}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to update trophies leaderboard: {ex.Message}");
    }
}

public async Task<List<LeaderboardEntry>> GetTopTrophiesAsync()
{
    try
    {
        var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(TrophiesLeaderboardId,
            new GetScoresOptions { Offset = 0, Limit = 10 }); // Adjust limit as needed

        Debug.Log($"Trophies leaderboard scores: {JsonConvert.SerializeObject(scoresResponse.Results)}");
        return scoresResponse.Results;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to fetch trophies leaderboard scores: {ex.Message}");
        return new List<LeaderboardEntry>();
    }
}

public async Task<LeaderboardEntry> GetPlayerTrophiesAsync()
{
    try
    {
        var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(TrophiesLeaderboardId);
        Debug.Log($"Player trophies score: {JsonConvert.SerializeObject(scoreResponse)}");
        return scoreResponse;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to fetch player trophies score: {ex.Message}");
        return null;
    }
}

public async Task UpdateTrophiesLeaderboardUI(Transform leaderboardContent)
{
    // Clear existing entries
    foreach (Transform child in leaderboardContent)
    {
        Destroy(child.gameObject);
    }

    // Fetch leaderboard data
    List<LeaderboardEntry> topScores = await GetTopTrophiesAsync();

    // Populate UI
    foreach (var entry in topScores)
    {
        LeaderboardEntryUI entryObject = Instantiate(leaderboardEntryUI, leaderboardContent);


        entryObject.m_Position.text = $"{entry.Rank + 1}";
        entryObject.m_Username.text = entry.PlayerName;
        entryObject.m_Value.text = $"{entry.Score}";
    }

    // Show the leaderboard panel
   // leaderboardPanel.SetActive(true);
}

#endregion

}
}
