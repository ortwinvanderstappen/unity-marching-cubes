using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotationSpeed;

    void Start()
    {

    }

    void Update()
    {
        // Read input
        float inputH = Input.GetAxisRaw("Horizontal");
        float inputV = Input.GetAxisRaw("Vertical");
        int verticalMovement = Input.GetKey(KeyCode.Q) ? 1 : Input.GetKey(KeyCode.E) ? -1 : 0;

        // Move player
        Vector3 t = transform.position;
        t += transform.right * inputH * _moveSpeed * Time.deltaTime;
        t += transform.forward * inputV * _moveSpeed * Time.deltaTime;
        t += -transform.up * verticalMovement * _moveSpeed * Time.deltaTime;
        transform.position = t;

        // Rotate player
        Quaternion r = transform.rotation;
        float pitch = r.eulerAngles.y + Input.GetAxisRaw("Mouse X") * _rotationSpeed * Time.deltaTime;
        float yaw = r.eulerAngles.x + Input.GetAxisRaw("Mouse Y") * -_rotationSpeed * Time.deltaTime;
        r.eulerAngles = new Vector3(yaw, pitch, 0.0f);
        transform.rotation = r;
    }
}
