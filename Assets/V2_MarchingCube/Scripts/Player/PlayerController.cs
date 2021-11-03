using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _sprintSpeedMultiplier = 2.0f;
    [SerializeField] private float jumpHeight = 3f;
    [Header("Ground collision")]
    [SerializeField] private Transform _groundCheckTransfrom;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;

    private CharacterController _characterController;
    private float _xRotation = 0f;
    private float _yRotation = 0f;
    private bool _isGrounded;
    private Vector3 _velocity;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckTransfrom.position, _groundDistance, _groundMask);

        RotatePlayer();
        MovePlayer();
    }

    private void RotatePlayer()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * _rotationSpeed * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _rotationSpeed * Time.deltaTime;

        _yRotation += mouseX;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        Quaternion rotation = transform.rotation;
        rotation.eulerAngles = new Vector3(_xRotation, _yRotation, 0f);
        transform.rotation = rotation;
    }

    private void MovePlayer()
    {
        // Read input
        float inputH = Input.GetAxisRaw("Horizontal");
        float inputV = Input.GetAxisRaw("Vertical");
        float multipliedMoveSpeed = _moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? _sprintSpeedMultiplier : 1.0f);

        _velocity.y += Physics.gravity.y * Time.deltaTime;
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        }

        // Move player
        Vector3 movementVector = transform.right * inputH + transform.forward * inputV;
        _characterController.Move(movementVector * multipliedMoveSpeed * Time.deltaTime);
        _characterController.Move(_velocity * Time.deltaTime);
    }
}
