using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeaderboardManager leaderboardManager;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI topScoresText;

    [Header("Leaderboard")]
    [SerializeField] private bool updateLeaderboard = true;
    [SerializeField] private int topScoreCount = 5;

    private void Start()
    {
        if (playerNameText != null)
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");
            playerNameText.text = $"Player: {playerName}";
        }

        leaderboardManager = FindAnyObjectByType<LeaderboardManager>();
        leaderboardManager.SetTopScoreCount(topScoreCount);
    }


    private void FixedUpdate()
    {

        if(updateLeaderboard)
        {
            leaderboardManager.FetchLeaderboard();
            updateLeaderboard = false;
            topScoresText.text = "Updating leaderboard...";
        }

        if (leaderboardManager == null || leaderboardManager.IsLeaderboardUpdating())
            return;
        if (topScoresText != null)
        {
            List<string> topScores = leaderboardManager.GetTopScoresList();
            topScoresText.text = $"Top {leaderboardManager.GetTopScoreCount()} Scores:\n";
            foreach (string scoreEntry in topScores)
            {
                topScoresText.text += scoreEntry + '\n';
            }
        }
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
