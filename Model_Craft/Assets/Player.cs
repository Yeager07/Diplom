using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player1 : MonoBehaviour
{
    private float speed = 4.0f;
    private float speedHorRot = 1.5f;
    private float speedVerRot = 1.5f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private float verRotLim = 60.0f;
    private Rigidbody rigidBody;
    private float horizontalInput;
    private float verticalInput;
    private float horizontalRotation;
    private float verticalRotation;
    private Vector3 moveDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Move()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        horizontalRotation = Input.GetAxis("Mouse X");
        verticalRotation = Input.GetAxis("Mouse Y");

        yaw += speedHorRot * horizontalRotation;
        pitch -= speedVerRot * verticalRotation;
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        //rigidBody.AddForce(moveDirection * speed, ForceMode.Force);
        rigidBody.MovePosition(rigidBody.position + moveDirection * speed *Time.deltaTime);
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
}
