using UnityEngine;

public class BasePlatform : MonoBehaviour
{
    [Header("Platform Dimensions")]
    [SerializeField] private float basePlatformWidth = 5f;
    [SerializeField] private float basePlatformHeight = 1f;
    [SerializeField] private float basePlatformLength = 5f;

    public float BasePlatformWidth => basePlatformWidth;
    public float BasePlatformHeight => basePlatformHeight;
    public float BasePlatformLength => basePlatformLength;
}
