using System.Collections.Generic;
using Tarabish.Netcode;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform entryContainer;
    [SerializeField] private LeaderboardEntryUI entryPrefab;
[SerializeField] private Button leaderboardButton;

    public void Start()
    {
        // Bind the button click event.
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardButtonClicked);
        }
    }

	private async void OnLeaderboardButtonClicked()
    {

        // Fetch leaderboard entries.
        List<LeaderboardEntry> scores = await SessionManager.Instance.GetTopScoresAsync();

        // Clear existing entries.
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate leaderboard entries
        foreach (var entry in scores)
        {
            LeaderboardEntryUI newEntry = Instantiate(entryPrefab, entryContainer);
            newEntry.m_Value.text = entry.Score.ToString();
			newEntry.m_Username.text = entry.PlayerName.ToString();
			newEntry.m_Position.text = entry.Rank.ToString();
          //  text.text = $"Rank: {entry.Rank}, Name: {entry.PlayerName}, Wins: {entry.Score}";
        }

    }

    public async void DisplayLeaderboard()
    {
        List<LeaderboardEntry> scores = await SessionManager.Instance.GetTopScoresAsync();

        // Clear existing entries
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate leaderboard entries
        foreach (var entry in scores)
        {
            LeaderboardEntryUI newEntry = Instantiate(entryPrefab, entryContainer);
            newEntry.m_Value.text = entry.Score.ToString();
			newEntry.m_Username.text = entry.PlayerName.ToString();
			newEntry.m_Position.text = entry.Rank.ToString();
          //  text.text = $"Rank: {entry.Rank}, Name: {entry.PlayerName}, Wins: {entry.Score}";
        }
    }
}
