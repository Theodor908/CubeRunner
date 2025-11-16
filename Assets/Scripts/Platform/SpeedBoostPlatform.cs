using System.Collections;
using UnityEngine;

public class SpeedBoostPlatform : BasePlatform
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float boostDuration = 3f;

    [Header("Collider Setup")]
    [Tooltip("The trigger collider (Is Trigger = checked)")]

    private bool hasBeenTriggered = false;

    private void Awake()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;

        if (other.CompareTag("Player"))
        {
            CubePlayerController player = other.GetComponent<CubePlayerController>();
            if (player != null)
            {
                ApplySpeedBoost(player);
                return;
            }
        }
    }

    private void ApplySpeedBoost(CubePlayerController player)
    {
        hasBeenTriggered = true;

        float currentSpeed = player.CurrentSpeed;
        float boostedSpeed = currentSpeed * speedMultiplier;
        player.AddSpeed(boostedSpeed - currentSpeed); 
    }
}
