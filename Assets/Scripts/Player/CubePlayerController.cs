using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CustomGravityController))]
public class CubePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool isFrozen = false;
    [SerializeField] private float initialSpeed = 5f;
    [SerializeField] private float acceleration = 0.5f; // Speed increase per second
    [SerializeField] private float horizontalMoveSpeed = 15f;
    [SerializeField] private float maxSpeed = 50f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCooldown = 0.2f;
    [SerializeField] private float numberOfAirJumps = 1;

    [Header("Crouch Settings")]
    [SerializeField] private float normalHeight = 1f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    private float crouchSpeedBoost = 0f;
    private float maxCrouchBoost = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private bool enableWheelRotation = true;
    [SerializeField] private float rotationSpeedMultiplier = 360f; // Degrees per unit moved

    [Header("Input")]
    private PlayerControls controls;
    private Vector2 movementInput;

    // Components
    private Rigidbody rb;
    private CustomGravityController gravityController;
    private Transform visualCube; // The actual cube mesh that will rotate

    // State
    private float currentSpeed;
    private float horizontalInput;
    private bool isCrouching;
    private bool jumpInput;
    private bool canJump = true;
    private bool allowAirJump = true;
    private float numberOfJumps = 1;
    private float lastJumpTime;
    private Vector3 originalScale;
    private float targetHeight;
    private float currentHeight;

    private float totalDistanceMoved;
    private Vector3 lastPosition;

    public float CurrentSpeed => currentSpeed;
    public bool IsCrouching => isCrouching;
    public float Acceleration => acceleration;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gravityController = GetComponent<CustomGravityController>();

        originalScale = transform.localScale;
        currentHeight = normalHeight;
        targetHeight = normalHeight;

        SetupVisualCube();

        currentSpeed = initialSpeed;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        lastPosition = transform.position;
    }

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerControls();
            controls.Gameplay.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();

            controls.Gameplay.Crouch.performed += ctx => isCrouching = true;
            controls.Gameplay.Crouch.canceled += ctx => isCrouching = false;

            controls.Gameplay.Jump.performed += ctx => jumpInput = true;
        }
        controls.Enable();
    }

    private void OnDisable()
    {

        controls.Gameplay.Move.performed -= ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Crouch.performed -= ctx => isCrouching = true;
        controls.Gameplay.Crouch.canceled -= ctx => isCrouching = false;
        controls.Gameplay.Jump.performed -= ctx => jumpInput = true;

        controls.Disable();
    }

    private void SetupVisualCube()
    {
        visualCube = transform.Find("VisualCube");

        if (visualCube == null)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "VisualCube";
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;

            Destroy(visual.GetComponent<Collider>());

            visualCube = visual.transform;

        }
    }

    private void Update()
    {
        if (isFrozen == false)
        {
            HandleInput();
            HandleCrouching();
        }
    }

    private void FixedUpdate()
    {
        if (isFrozen == false)
        {
            HandleHorizontalMovement();
            AccelerateForward();
            HandleWheelRotation();
            UpdateCanJump();
        }
    }

    private void HandleInput()
    {
        horizontalInput = movementInput.x;
       
        if(jumpInput)
            Jump();
    }

    private void AccelerateForward()
    {

        currentSpeed += acceleration * Time.fixedDeltaTime;

        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        Vector3 forwardMovement = Vector3.forward * currentSpeed;

        forwardMovement.y = rb.linearVelocity.y;

        forwardMovement.x = rb.linearVelocity.x;

        rb.linearVelocity = forwardMovement;
    }

    private void HandleHorizontalMovement()
    {

        float horizontalVelocity = horizontalInput * horizontalMoveSpeed * Time.fixedDeltaTime;

        float targetX = transform.position.x + horizontalVelocity;

        Vector3 newPosition = transform.position;
        newPosition.x = targetX;
        transform.position = newPosition;
    }

    private void HandleCrouching()
    {
        targetHeight = isCrouching ? crouchHeight : normalHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        Vector3 newScale = originalScale;
        newScale.y = currentHeight;
        transform.localScale = newScale;



        if (crouchSpeedBoost < 0)
        {
            crouchSpeedBoost = 0f;
        }

        if (isCrouching && gravityController.IsGrounded())
        {
            if (crouchSpeedBoost < maxCrouchBoost)
            {
                float boostGain = 5f * Time.deltaTime;
                crouchSpeedBoost += boostGain;
                crouchSpeedBoost = Mathf.Min(crouchSpeedBoost, maxCrouchBoost);
                AddSpeed(boostGain);
            }
        }
        else if (crouchSpeedBoost > 0.5f)
        {
            float boostLoss = 3f * Time.deltaTime;
            crouchSpeedBoost -= boostLoss;
            AddSpeed(-boostLoss);
        }
    }

    private void Jump()
    {
        jumpInput = false;
        if (canJump)
        {
            if (Time.time - lastJumpTime < jumpCooldown)
                return;
            gravityController.Jump(jumpForce);
            lastJumpTime = Time.time;
            canJump = false;
            numberOfJumps = numberOfAirJumps;
            Debug.Log("Jumped");
            return;
        }

        if (!canJump && allowAirJump == true)
        {
            if (numberOfJumps > 0)
            {
                gravityController.Jump(jumpForce);
                numberOfJumps--;
                Debug.Log("Air Jumped");
            }
        }


    }

    private void UpdateCanJump()
    {
        if (gravityController.IsGrounded())
        {
            canJump = true;
        }
    }

    private void HandleWheelRotation()
    {
        if (!enableWheelRotation || visualCube == null)
            return;

        float distanceThisFrame = (transform.position - lastPosition).magnitude;
        totalDistanceMoved += distanceThisFrame;

        // Calculate rotation based on distance (like a wheel)
        // Circumference = 2 * PI * radius, for a unit cube radius ? 0.5
        // For each unit moved forward, the cube should rotate 360 degrees
        float rotationAmount = (distanceThisFrame / (2f * Mathf.PI * 0.5f)) * rotationSpeedMultiplier;

        visualCube.Rotate(Vector3.right, rotationAmount, Space.Self);

        lastPosition = transform.position;
    }

    public void SetFreeze(bool freeze)
    {
        isFrozen = freeze;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }


    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    public void AddSpeed(float speedBoost)
    {
        currentSpeed = currentSpeed + speedBoost;
    }

    public void ResetSpeed()
    {
        currentSpeed = initialSpeed;
    }

    public float GetHorizontalOffset()
    {
        return transform.position.x;
    }
}