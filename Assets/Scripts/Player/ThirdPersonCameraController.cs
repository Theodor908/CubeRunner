using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _zoomLerpSpeed = 10f;
    [SerializeField] private float _minDistance = 3f;
    [SerializeField] private float _maxDistance = 15f;

    [SerializeField] private Transform _playerTransform;

    private PlayerControls playerControls;

    private CinemachineCamera _camera;
    private CinemachineOrbitalFollow _orbitalFollow;
    private Vector2 scrollDelta;

    private float _targetZoom;
    private float _currentZoom;

    void Start()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
        playerControls.CameraControls.MouseZoom.performed += HandleMouseScroll;

        Cursor.lockState = CursorLockMode.Locked;
        _camera = GetComponent<CinemachineCamera>();
        _orbitalFollow = _camera.GetComponent<CinemachineOrbitalFollow>();

        _targetZoom = _currentZoom = _orbitalFollow.Radius;
    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
    }

    void Update()
    {

        if (_playerTransform == null)
        {
            GameObject gm = GameObject.FindGameObjectWithTag("Player");

            if (gm != null)
            {
                _playerTransform = gm.transform;
                _camera.Follow = _playerTransform;
            }
        }

        if (_playerTransform == null)
            return;

        if(scrollDelta.y != 0)
        {
            if(_orbitalFollow != null)
            {
                _targetZoom = Mathf.Clamp(_orbitalFollow.Radius - scrollDelta.y * _zoomSpeed, _minDistance, _maxDistance);
                scrollDelta = Vector2.zero;
            }
        }

        _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.deltaTime * _zoomLerpSpeed);
        _orbitalFollow.Radius = _currentZoom;

    }

    private void OnDisable()
    {
        playerControls.CameraControls.MouseZoom.performed -= HandleMouseScroll;
    }
}
