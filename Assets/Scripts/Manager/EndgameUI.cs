using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndgameUI : MonoBehaviour
{
    [Header("Endgame Display")]
    [SerializeField] private GameObject endgamePanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardText;

    [Header("Loading")]
    [SerializeField] private string loadingMessage = "Loading leaderboard...";

    [Header("References")]
    [SerializeField] private RiskBasedScoringSystem scoringSystem;
    [SerializeField] private LeaderboardManager leaderboardManager;

    [Header("Settings")]
    [SerializeField] private int topScoreCount = 10;

    private bool isShowing = false;

    private void Awake()
    {

        if (scoringSystem == null)
        {
            scoringSystem = FindAnyObjectByType<RiskBasedScoringSystem>();
        }

        if (leaderboardManager == null)
        {
            leaderboardManager = FindAnyObjectByType<LeaderboardManager>();
        }

        if (endgamePanel != null)
        {
            endgamePanel.SetActive(false);
        }
    }

    public void ShowEndgameUI()
    {
        if (isShowing) return; 
        isShowing = true;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show panel
        if (endgamePanel != null)
        {
            endgamePanel.SetActive(true);
        }

        ShowFinalScore();

        // Show loading message
        if (leaderboardText != null)
        {
            leaderboardText.text = loadingMessage;
        }

        StartCoroutine(SubmitAndFetchLeaderboard());
    }

    private void ShowFinalScore()
    {
        if (finalScoreText == null || scoringSystem == null)
        {
            return;
        }

        int finalScore = Mathf.RoundToInt(scoringSystem.GetTotalScore());
        finalScoreText.text = $"Final Score: {finalScore:N0}";
    }

    private IEnumerator SubmitAndFetchLeaderboard()
    {
        if (leaderboardManager == null)
        {
            ShowError("Leaderboard unavailable");
            yield break;
        }

        leaderboardManager.OnGameOver();

        yield return new WaitForSeconds(0.5f);

        bool fetchStarted = false;
        bool fetchCompleted = false;
        string errorMessage = null;

        StartCoroutine(LeaderboardAPI.Instance.GetLeaderboard(
            topScoreCount,
            (entries) => {
                fetchCompleted = true;
                DisplayLeaderboard(entries);
            },
            (error) => {
                fetchCompleted = true;
                errorMessage = error;
            }
        ));

        fetchStarted = true;

        float timeout = 10f;
        float elapsed = 0f;

        while (!fetchCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }


        if (!fetchCompleted)
        {
            ShowError("Leaderboard timed out");
        }
        else if (errorMessage != null)
        {
            ShowError($"Failed to load leaderboard\n{errorMessage}");
        }
    }

    private void DisplayLeaderboard(LeaderboardEntry[] entries)
    {
        if (leaderboardText == null)
        {
            return;
        }

        if (entries == null || entries.Length == 0)
        {
            leaderboardText.text = "No scores yet!\nBe the first!";
            return;
        }

        string leaderboard = $"<b>Top {entries.Length} Scores:</b>\n\n";

        string currentPlayerName = leaderboardManager.GetPlayerName();

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];

            bool isCurrentPlayer = entry.player_name == currentPlayerName;

            if (isCurrentPlayer)
            {
                leaderboard += $"<color=yellow>#{entry.rank}. {entry.player_name}: {entry.score:N0}</color>\n";
            }
            else
            {
                leaderboard += $"#{entry.rank}. {entry.player_name}: {entry.score:N0}\n";
            }
        }

        leaderboardText.text = leaderboard;
    }


    private void ShowError(string message)
    {
        if (leaderboardText != null)
        {
            leaderboardText.text = $"<color=red>{message}</color>";
        }
    }

    public void HideEndgameUI()
    {
        if (endgamePanel != null)
        {
            endgamePanel.SetActive(false);
        }

        isShowing = false;

    }

    public void OnRestartClicked()
    {

        HideEndgameUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }
        else
        {
            Debug.LogError("[EndgameUI] GameManager not found!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    public void OnQuitClicked()
    {
        Debug.Log("[EndgameUI] Quit button clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}