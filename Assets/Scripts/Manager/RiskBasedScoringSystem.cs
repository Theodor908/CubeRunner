using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class RiskBasedScoringSystem : MonoBehaviour
{

    [Header("Distance Scoring")]
    [SerializeField] private float pointsPerUnit = 10f;
    [SerializeField] private bool trackContinuousDistance = true;

    [Header("Risk Multipliers")]
    [Tooltip("Multiplier for jumping from high platforms")]
    [SerializeField] private float heightRiskMultiplier = 0.5f;
    [Tooltip("Multiplier for long gap jumps")]
    [SerializeField] private float gapRiskMultiplier = 1f;
    [Tooltip("Multiplier for landing near edge")]
    [SerializeField] private float edgeLandingMultiplier = 2f;
    [Tooltip("Bonus for consecutive risky jumps")]
    [SerializeField] private float comboMultiplier = 1.5f;

    [Header("Risk Thresholds")]
    [SerializeField] private float minRiskyHeight = 3f;
    [SerializeField] private float minRiskyGap = 5f;
    [SerializeField] private float edgeDetectionRadius = 0.5f;

    [Header("Combo System")]
    [SerializeField] private int comboToActivate = 3;
    [SerializeField] private float comboResetTime = 2f;

    [Header("Display")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private ScoreUI scoreUI;

    // Score tracking
    private float totalScore = 0f;
    private float distanceTraveled = 0f;
    private Vector3 lastPosition;
    private Vector3 startPosition;

    // Jump tracking
    private bool isInAir = false;
    private Vector3 jumpStartPosition;
    private Vector3 landingPosition;
    private float jumpStartHeight;
    private float fallDistance;
    private float horizontalGapDistance;

    // Risk tracking
    private int currentCombo = 0;
    private float lastRiskyJumpTime = 0f;
    private List<JumpData> recentJumps = new List<JumpData>();

    // References
    private CustomGravityController gravityController;
    private Transform playerTransform;
    private CubePlayerController playerController;

    private void Awake()
    {
        playerTransform = transform;
        gravityController = GetComponent<CustomGravityController>();
        playerController = GetComponent<CubePlayerController>();

        if (gravityController == null)
        {
            Debug.LogWarning("No CustomGravityController found. Using fallback ground detection.");
        }

        startPosition = playerTransform.position;
        lastPosition = startPosition;
    }

    private void Update()
    {
        if (playerController.IsFrozen() == false)
        {
            if (trackContinuousDistance)
            {
                TrackDistance();
            }

            TrackJumpState();

            CheckComboTimeout();
        }

        if(scoreUI == null)
        {
            // find by tag Canvas

            GameObject gm = GameObject.FindGameObjectWithTag("Canvas");
            if (gm != null)
            {
                scoreUI = gm.GetComponent<ScoreUI>();
                Debug.Log("Found Canvas");
            }

        }
    }

    #region Distance Tracking

    private void TrackDistance()
    {
        float distanceThisFrame = Vector3.Distance(
            new Vector3(lastPosition.x, 0, lastPosition.z),
            new Vector3(playerTransform.position.x, 0, playerTransform.position.z)
        );

        distanceTraveled += distanceThisFrame;

        if (distanceThisFrame > 0.01f)
        {
            float distancePoints = distanceThisFrame * pointsPerUnit;
            AddScore(distancePoints, "Distance");
        }

        lastPosition = playerTransform.position;
    }

    #endregion

    #region Jump & Risk Tracking

    private void TrackJumpState()
    {
        bool wasInAir = isInAir;

        if (gravityController != null)
        {
            isInAir = !gravityController.IsGrounded();
        }
        else
        {
            isInAir = !Physics.Raycast(playerTransform.position, Vector3.down, 0.6f);
        }

        if (isInAir && !wasInAir)
        {
            OnJumpStart();
        }

        if (!isInAir && wasInAir)
        {
            OnLanding();
        }

        if (isInAir)
        {
            fallDistance = jumpStartPosition.y - playerTransform.position.y;
        }
    }

    private void OnJumpStart()
    {
        jumpStartPosition = playerTransform.position;
        jumpStartHeight = jumpStartPosition.y;
        fallDistance = 0f;
    }

    private void OnLanding()
    {
        landingPosition = playerTransform.position;

        float heightDrop = jumpStartHeight - landingPosition.y;
        horizontalGapDistance = Vector3.Distance(
            new Vector3(jumpStartPosition.x, 0, jumpStartPosition.z),
            new Vector3(landingPosition.x, 0, landingPosition.z)
        );

        CalculateJumpScore(heightDrop, horizontalGapDistance);

        StoreJumpData(heightDrop, horizontalGapDistance);
    }

    private void CalculateJumpScore(float heightDrop, float gapDistance)
    {
        float baseJumpPoints = 50f;
        float totalMultiplier = 1f;
        List<string> riskFactors = new List<string>();

        bool isRiskyJump = false;

        if (heightDrop >= minRiskyHeight)
        {
            float heightBonus = heightDrop * heightRiskMultiplier;
            totalMultiplier += heightBonus;
            riskFactors.Add($"High Fall ({heightDrop:F1}m)");
            isRiskyJump = true;
        }

        if (gapDistance >= minRiskyGap)
        {
            float gapBonus = (gapDistance - minRiskyGap) * gapRiskMultiplier;
            totalMultiplier += gapBonus;
            riskFactors.Add($"Long Gap ({gapDistance:F1}m)");
            isRiskyJump = true;
        }

        if (IsEdgeLanding())
        {
            totalMultiplier += edgeLandingMultiplier;
            riskFactors.Add("Edge Landing");
            isRiskyJump = true;
        }

        if (isRiskyJump)
        {
            currentCombo++;
            lastRiskyJumpTime = Time.time;

            if (currentCombo >= comboToActivate)
            {
                float comboBonus = comboMultiplier * (currentCombo - comboToActivate + 1);
                totalMultiplier += comboBonus;
                riskFactors.Add($"Combo x{currentCombo}");
            }
        }
        else
        {
            currentCombo = 0;
        }

        float finalPoints = baseJumpPoints * totalMultiplier;

        string scoreReason = riskFactors.Count > 0
            ? $"Risky Jump: {string.Join(", ", riskFactors)}"
            : "Safe Jump";

        AddScore(finalPoints, scoreReason);

        scoreUI.ShowJumpScore(finalPoints, totalMultiplier, riskFactors);
    }

    private bool IsEdgeLanding()
    {
        Vector3 landingPos = landingPosition + Vector3.up * 0.1f;

        int edgeCount = 0;
        int totalRays = 8;

        for (int i = 0; i < totalRays; i++)
        {
            float angle = (360f / totalRays) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 rayStart = landingPos + direction * edgeDetectionRadius;

            if (!Physics.Raycast(rayStart, Vector3.down, 1f))
            {
                edgeCount++;
            }
        }

        bool isEdge = edgeCount >= 3;

        return isEdge;
    }

    private void CheckComboTimeout()
    {
        if (currentCombo > 0 && Time.time - lastRiskyJumpTime > comboResetTime)
        {
            currentCombo = 0;
        }
    }

    #endregion

    #region Score Management

    private void AddScore(float points, string reason)
    {
        totalScore += points;

        if (scoreUI != null)
        {
            scoreUI.UpdateScore(totalScore);
        }
    }

    public float GetTotalScore()
    {
        return totalScore;
    }

    public float GetDistanceTraveled()
    {
        return distanceTraveled;
    }

    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    public void ResetScore()
    {
        totalScore = 0f;
        distanceTraveled = 0f;
        currentCombo = 0;
        recentJumps.Clear();
        startPosition = playerTransform.position;
        lastPosition = startPosition;

        if (scoreUI != null)
        {
            scoreUI.UpdateScore(0);
        }
    }

    #endregion

    #region Jump Data Storage

    private void StoreJumpData(float height, float distance)
    {
        JumpData jump = new JumpData
        {
            timestamp = Time.time,
            heightDrop = height,
            gapDistance = distance,
            startPosition = jumpStartPosition,
            landingPosition = landingPosition,
            wasRisky = height >= minRiskyHeight || distance >= minRiskyGap
        };

        recentJumps.Add(jump);

        // Keep only last 10 jumps
        if (recentJumps.Count > 10)
        {
            recentJumps.RemoveAt(0);
        }
    }

    public List<JumpData> GetRecentJumps()
    {
        return new List<JumpData>(recentJumps);
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (recentJumps.Count > 0)
        {
            JumpData lastJump = recentJumps[recentJumps.Count - 1];

            Gizmos.color = lastJump.wasRisky ? Color.red : Color.green;
            Gizmos.DrawLine(lastJump.startPosition, lastJump.landingPosition);
            Gizmos.DrawWireSphere(lastJump.startPosition, 0.3f);
            Gizmos.DrawWireSphere(lastJump.landingPosition, 0.3f);
        }

        if (isInAir)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, edgeDetectionRadius);
        }
    }

    #endregion
}

[System.Serializable]
public class JumpData
{
    public float timestamp;
    public float heightDrop;
    public float gapDistance;
    public Vector3 startPosition;
    public Vector3 landingPosition;
    public bool wasRisky;
}