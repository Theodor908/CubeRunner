using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Jump Score Popup")]
    [SerializeField] private GameObject jumpScorePopup;
    [SerializeField] private TextMeshProUGUI jumpScoreText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI riskFactorsText;
    [SerializeField] private float popupDuration = 2f;
    [SerializeField] private AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Combo Display")]
    [SerializeField] private GameObject comboDisplay;
    [SerializeField] private float comboPulseDuration = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color riskyColor = Color.yellow;
    [SerializeField] private Color comboColor = Color.red;

    private RiskBasedScoringSystem scoringSystem;
    private Coroutine popupCoroutine;

    private void Awake()
    {
        scoringSystem = FindAnyObjectByType<RiskBasedScoringSystem>();

        if (jumpScorePopup != null)
        {
            jumpScorePopup.SetActive(false);
        }

        if (comboDisplay != null)
        {
            comboDisplay.SetActive(false);
        }
    }

    private void Update()
    {
        if (scoringSystem == null) return;
        UpdateScore(scoringSystem.GetTotalScore());
        UpdateDistanceDisplay();
        UpdateComboDisplay();
    }

    public void UpdateScore(float score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:F0}";
        }
    }

    private void UpdateDistanceDisplay()
    {
        if (distanceText != null)
        {
            float distance = scoringSystem.GetDistanceTraveled();
            distanceText.text = $"Distance: {distance:F1}m";
        }
    }

    private void UpdateComboDisplay()
    {
        if (comboText == null) return;

        int combo = scoringSystem.GetCurrentCombo();

        if (combo > 0)
        {
            comboText.text = $"Combo: x{combo}";
            comboText.color = combo >= 5 ? comboColor : riskyColor;

            if (comboDisplay != null && !comboDisplay.activeSelf)
            {
                comboDisplay.SetActive(true);
            }
        }
        else
        {
            if (comboDisplay != null && comboDisplay.activeSelf)
            {
                comboDisplay.SetActive(false);
            }
        }
    }

    public void ShowJumpScore(float points, float multiplier, List<string> riskFactors)
    {
        Debug.Log("Attempting to show jump score popup.");
        if (jumpScorePopup == null) return;

        Debug.Log($"Showing Jump Score Popup: +{points:F0} points, x{multiplier:F1} multiplier");
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }

        popupCoroutine = StartCoroutine(ShowJumpScorePopup(points, multiplier, riskFactors));
    }

    private IEnumerator ShowJumpScorePopup(float points, float multiplier, List<string> riskFactors)
    {
        jumpScorePopup.SetActive(true);

        if (jumpScoreText != null)
        {
            jumpScoreText.text = $"+{points:F0}";
            jumpScoreText.color = multiplier > 2f ? comboColor : (multiplier > 1.2f ? riskyColor : normalColor);
        }

        if (multiplierText != null)
        {
            multiplierText.text = multiplier > 1f ? $"x{multiplier:F1}" : "";
        }

        if (riskFactorsText != null)
        {
            riskFactorsText.text = riskFactors.Count > 0 ? string.Join("\n", riskFactors) : "Safe Jump";
        }

        RectTransform popupRect = jumpScorePopup.GetComponent<RectTransform>();
        Vector3 startPos = popupRect.anchoredPosition;
        Vector3 endPos = startPos + Vector3.up * 100f;

        float elapsed = 0f;

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupDuration;

            popupRect.anchoredPosition = Vector3.Lerp(startPos, endPos, popupCurve.Evaluate(t));

            CanvasGroup canvasGroup = jumpScorePopup.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - t;
            }

            yield return null;
        }

        jumpScorePopup.SetActive(false);

        popupRect.anchoredPosition = startPos;

        popupCoroutine = null;
    }
}
