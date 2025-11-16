using TMPro;
using UnityEngine;

public class EndgameUI : MonoBehaviour
{
    [Header("Endgame Display")]
    [SerializeField] private GameObject endgame;
    [SerializeField] private TextMeshProUGUI finalScore;
    [SerializeField] private TextMeshProUGUI top3;

    private RiskBasedScoringSystem scoringSystem;

    private void Awake()
    {
        scoringSystem = FindAnyObjectByType<RiskBasedScoringSystem>();

        if(endgame != null)
        {
            endgame.SetActive(false);
        }
    }

    public void ShowEndgameUI()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (endgame != null)
        {
            endgame.SetActive(true);
        }
        if (finalScore != null && scoringSystem != null)
        {
            finalScore.text = $"Final Score: {scoringSystem.GetTotalScore():F0}";
        }
        if (top3 != null)
        {
            string topScores = PlayerPrefs.GetString("TopScores", "0,0,0");
            string[] scores = topScores.Split(',');
            top3.text = "Top 3 Scores:\n";
            for (int i = 0; i < scores.Length; i++)
            {
                top3.text += $"{i + 1}. {scores[i]}\n";
            }
        }
    }

    public void Restart()
    {
        GameManager.Instance.ResetGame();
    }
}
