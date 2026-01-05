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
    public bool isBuildMode = false;
    public GameObject target;

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

        transform.rotation = Quaternion.Euler(rotateDirection);
    }
    
    void MoveBuildMode()
    {
        rigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
        GetComponent<MeshRenderer>().enabled = false;
        
        moveDirection = transform.forward * Input.GetAxis("Mouse ScrollWheel") * 100;
            
        if(Input.GetMouseButton(2))
        {
            if(Input.GetKey(KeyCode.LeftShift))
            moveDirection = -transform.up * Input.GetAxis("Mouse Y") - transform.right * Input.GetAxis("Mouse X");

            else
            Move();
            /*{
                transform.RotateAround(target.transform.position, Vector3.up, Input.GetAxis("Mouse X"));
                transform.RotateAround(target.transform.position, Vector3.right, Input.GetAxis("Mouse Y"));
            }*/
        }

        rigidBody.MovePosition(rigidBody.position + moveDirection * speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if(!isBuildMode)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
            GetComponent<MeshRenderer>().enabled = true;

            Move();

            moveDirection = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
            rigidBody.MovePosition(rigidBody.position + moveDirection * speed * Time.deltaTime);
        }

        else
        MoveBuildMode();
    }
}
