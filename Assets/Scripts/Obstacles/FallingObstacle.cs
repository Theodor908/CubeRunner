using UnityEngine;

public class FallingObstacle : MonoBehaviour
{
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
