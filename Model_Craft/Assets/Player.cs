using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private float speed = 4.0f;
    private float buildSpeed = 25.0f;
    private float speedRot = 1.5f;
    private float speedBuildRot = 3.0f;
    private float verRotLim = 60.0f;
    private Rigidbody rigidBody;
    public Vector3 moveDirection;
    public Vector3 rotateDirection;
    public bool isBuildMode = false;
    public GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Rotate(float speed)
    {
        GetComponent<MeshRenderer>().enabled = !isBuildMode;
        rotateDirection.x -= speed * Input.GetAxis("Mouse Y");
        rotateDirection.y += speed * Input.GetAxis("Mouse X");
        rotateDirection.z = 0;

        if(!isBuildMode)
        {
            if(rotateDirection.x < -verRotLim)
            rotateDirection.x = -verRotLim;

            if(rotateDirection.x > verRotLim)
            rotateDirection.x = verRotLim;
        }

        transform.rotation = Quaternion.Euler(rotateDirection);
    }
    
    void MoveBuildMode()
    {
        rigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
        moveDirection = transform.forward * Input.GetAxis("Mouse ScrollWheel") * 200;
            
        if(Input.GetMouseButton(2))
        {
            if(Input.GetKey(KeyCode.LeftShift))
            moveDirection = -transform.up * Input.GetAxis("Mouse Y") - transform.right * Input.GetAxis("Mouse X");

            else
            Rotate(speedBuildRot);
        }

        rigidBody.MovePosition(rigidBody.position + moveDirection * buildSpeed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if(!isBuildMode)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;

            Rotate(speedRot);

            moveDirection = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
            rigidBody.MovePosition(rigidBody.position + moveDirection * speed * Time.deltaTime);
        }

        else
        MoveBuildMode();
    }
}
