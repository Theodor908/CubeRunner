using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityController : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravityStrength = 20f;

    [SerializeField] private float maxFallSpeed = 50f;

    [SerializeField] private bool useCustomGravity = true;

    [Header("Gravity Direction")]
    [SerializeField] private Vector3 gravityDirection = Vector3.down;

    [Header("Ground Detection")]
    [SerializeField] private bool isGrounded = false;

    [SerializeField] private float groundCheckDistance = 0.1f;

    [SerializeField] private LayerMask groundLayer;

    [Header("Advanced Options")]
    [SerializeField] private float fallGravityMultiplier = 1.5f;

    [SerializeField] private bool useFallMultiplier = true;

    private Rigidbody rb;
    private Vector3 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (useCustomGravity)
        {
            rb.useGravity = false;
        }
    }

    private void FixedUpdate()
    {
        if (!useCustomGravity)
        {
            rb.useGravity = true;
            return;
        }

        CheckGrounded();
        ApplyCustomGravity();
    }

    private void ApplyCustomGravity()
    {
        Vector3 normalizedGravityDir = gravityDirection.normalized;

        float gravityToApply = gravityStrength;

        if (useFallMultiplier && !isGrounded)
        {
            float velocityInGravityDir = Vector3.Dot(rb.linearVelocity, normalizedGravityDir);

            if (velocityInGravityDir > 0) 
            {
                gravityToApply *= fallGravityMultiplier;
            }
        }

        Vector3 gravityForce = normalizedGravityDir * gravityToApply;

        rb.AddForce(gravityForce, ForceMode.Acceleration);

        ClampVelocity(normalizedGravityDir);
    }

    private void ClampVelocity(Vector3 gravityDir)
    {
        float velocityInGravityDir = Vector3.Dot(rb.linearVelocity, gravityDir);

        if (velocityInGravityDir > maxFallSpeed)
        {
            Vector3 excessVelocity = gravityDir * (velocityInGravityDir - maxFallSpeed);
            rb.linearVelocity -= excessVelocity;
        }
    }

    private void CheckGrounded()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = gravityDirection.normalized;

        isGrounded = Physics.SphereCast(rayOrigin, 0.2f, rayDirection, out RaycastHit hit, groundCheckDistance, groundLayer);
    }

    #region Public Methods

    public void SetGravityStrength(float strength)
    {
        gravityStrength = strength;
    }

    public void SetGravityDirection(Vector3 direction)
    {
        gravityDirection = direction.normalized;
    }

    public void SetCustomGravityEnabled(bool enabled)
    {
        useCustomGravity = enabled;
        rb.useGravity = !enabled;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void Jump(float jumpForce)
    {
        if (isGrounded)
        {
            // Apply force opposite to gravity direction
            rb.AddForce(-gravityDirection.normalized * jumpForce, ForceMode.Impulse);
        }
    }

    public void ReverseGravity()
    {
        gravityDirection = -gravityDirection;
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, gravityDirection.normalized * 2f);

        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        Gizmos.DrawRay(transform.position, gravityDirection.normalized * groundCheckDistance);
    }

    #endregion
}