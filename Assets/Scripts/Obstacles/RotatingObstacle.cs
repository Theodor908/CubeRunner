using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CubePlayerController playerController = other.GetComponent<CubePlayerController>();
            if (playerController != null)
            {
                GameManager.Instance.PlayerLost();
            }
        }
    }
}
