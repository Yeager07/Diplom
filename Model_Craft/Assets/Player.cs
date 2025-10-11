using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player1 : MonoBehaviour
{
    private float speed = 4.0f;
    private float speedRot = 1.5f;
    private float verRotLim = 60.0f;
    private Rigidbody rigidBody;
    private Vector3 moveDirection;
    private Vector3 rotateDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Move()
    {
        rotateDirection.x -= speedRot * Input.GetAxis("Mouse Y");
        rotateDirection.y += speedRot * Input.GetAxis("Mouse X");

        if(rotateDirection.x < -verRotLim)
        rotateDirection.x = -verRotLim;

        if(rotateDirection.x > verRotLim)
        rotateDirection.x = verRotLim;

        moveDirection = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        rigidBody.MovePosition(rigidBody.position + moveDirection * speed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(rotateDirection);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
}
