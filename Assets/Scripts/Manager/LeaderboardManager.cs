using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{

    public static LeaderboardManager instance;

    [Header("Player Settings")]
    [SerializeField] private string playerNameKey = "PlayerName";

    [Header("References")]
    [SerializeField] private RiskBasedScoringSystem scoringSystem;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MainMenuUI mainMenuUI;

    [Header("Leaderboard Settings")]
    [SerializeField] private int topScoresToFetch = 5;
    [SerializeField] private List<string> topScoresList;

    private bool leaderboardUpdating = true;

    private string currentPlayerName;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        // Get or generate player name from PlayerPrefs
        currentPlayerName = GetOrCreatePlayerName();
        Debug.Log($"[Leaderboard] Playing as: {currentPlayerName}");
    }
    private void Update()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
        if (scoringSystem == null)
        {
            scoringSystem = FindAnyObjectByType<RiskBasedScoringSystem>();
        }
    }

    private string GetOrCreatePlayerName()
    {
        if (PlayerPrefs.HasKey(playerNameKey))
        {
            return PlayerPrefs.GetString(playerNameKey);
        }
        else
        {
            // Generate unique player name
            string newName = $"Player_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            PlayerPrefs.SetString(playerNameKey, newName);
            PlayerPrefs.Save();
            return newName;
        }
    }

    public void OnGameOver()
    {
        leaderboardUpdating = true;
        if (scoringSystem == null)
        {
            Debug.LogError("[Leaderboard] ScoringSystem not assigned!");
            return;
        }

        // Get final score
        int finalScore = Mathf.RoundToInt(scoringSystem.GetTotalScore());

        Debug.Log($"[Leaderboard] Game Over! Score: {finalScore}");

        // Submit score to leaderboard
        StartCoroutine(LeaderboardAPI.Instance.SubmitScore(
            currentPlayerName,
            finalScore,
            OnScoreSubmitted,
            OnError
        ));

        leaderboardUpdating = false;
    }

    private void OnScoreSubmitted()
    {
        FetchLeaderboard();
    }

    public int GetTopScoreCount()
    {
        return topScoresToFetch;
    }

    public void SetTopScoreCount(int count)
    {
        topScoresToFetch = count;
    }

    public List<string> GetTopScoresList()
    {
        if(topScoresList == null)
        {
            topScoresList = new List<string>();
            FetchLeaderboard();
        }
        return topScoresList;
    }

    public void FetchLeaderboard()
    {
        leaderboardUpdating = true;
        StartCoroutine(LeaderboardAPI.Instance.GetLeaderboard(
            topScoresToFetch, 
            OnLeaderboardReceived,
            OnError
        ));
    }

    private void OnLeaderboardReceived(LeaderboardEntry[] entries)
    {
        Debug.Log($"[Leaderboard] Received {entries.Length} entries:");

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            topScoresList.Add($"{i + 1}. {entry.player_name} - Score: {entry.score} (Games: {entry.games_played})");

            // Highlight if it's the current player
            if (entry.player_name == currentPlayerName)
            {
                topScoresList[i] += " <- You!";
            }


        }

        leaderboardUpdating = false;
    }

    public bool IsLeaderboardUpdating()
    {
        return leaderboardUpdating;
    }

    public void FetchPlayerStats()
    {
        StartCoroutine(LeaderboardAPI.Instance.GetPlayerStats(
            currentPlayerName,
            OnPlayerStatsReceived,
            OnError
        ));
    }

    private void OnPlayerStatsReceived(PlayerStats stats)
    {
        Debug.Log($"[Leaderboard] Your Stats:");
        Debug.Log($"  Best Score: {stats.best_score}");
        Debug.Log($"  Games Played: {stats.games_played}");
        Debug.Log($"  Average Score: {stats.average_score}");
    }

    public void FetchPlayerRank()
    {
        StartCoroutine(LeaderboardAPI.Instance.GetPlayerRank(
            currentPlayerName,
            OnPlayerRankReceived,
            OnError
        ));
    }

    private void OnPlayerRankReceived(PlayerRank rank)
    {
        if (rank.rank > 0)
        {
            Debug.Log($"[Leaderboard] Your Rank: #{rank.rank}");
        }
        else
        {
            Debug.Log($"[Leaderboard] Not ranked yet - play more games!");
        }
    }

    private void OnError(string error)
    {
        Debug.LogError($"[Leaderboard] Error: {error}");
        // Optionally show error UI to player
    }

    public string GetPlayerName()
    {
        return currentPlayerName;
    }

    public void ChangePlayerName(string newName)
    {
        currentPlayerName = newName;
        PlayerPrefs.SetString(playerNameKey, newName);
        PlayerPrefs.Save();
        Debug.Log($"[Leaderboard] Name changed to: {newName}");
    }
}