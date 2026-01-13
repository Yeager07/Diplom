using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private float speed = 4.0f;
    private float minDistance = 2f;
    private float maxDistance = 10f;
    private float buildSpeed = 25.0f;
    private float speedRot = 1.5f;
    private float verRotLim = 60.0f;
    private float speedBuildRot = 3.0f;
    private Rigidbody rigidBody;
    private Vector3 targetOffset = Vector3.zero;
    private Vector3 targetPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 moveDirection;
    private int selectedItem;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public List<string> keys = new List<string>();
    public List<int> values = new List<int>();
    public Vector3 rotateDirection;
    public bool isBuildMode = false;
    public float distance = 0.0f;
    public Vector3 target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Move()
    {
        GetComponent<MeshRenderer>().enabled = !isBuildMode;
        rigidBody.constraints = RigidbodyConstraints.FreezePositionY;

        rotateDirection.x -= speedRot * Input.GetAxis("Mouse Y");
        rotateDirection.y += speedRot * Input.GetAxis("Mouse X");
        rotateDirection.z = 0;

        if(rotateDirection.x < -verRotLim)
        rotateDirection.x = -verRotLim;

        if(rotateDirection.x > verRotLim)
        rotateDirection.x = verRotLim;

        moveDirection = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");

        rigidBody.MovePosition(rigidBody.position + moveDirection * speed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(rotateDirection);
    }
    
    void MoveBuildMode()
    {
        rigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
        distance -= Input.GetAxis("Mouse ScrollWheel") * speed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
            
        if(Input.GetMouseButton(2))
        {
            if(Input.GetKey(KeyCode.LeftShift))
            targetPosition += (-transform.up * Input.GetAxis("Mouse Y") - transform.right * Input.GetAxis("Mouse X")) * Time.deltaTime * buildSpeed;

            else
            {
                rotateDirection.x -= speedBuildRot * Input.GetAxis("Mouse Y");
                rotateDirection.y += speedBuildRot * Input.GetAxis("Mouse X");
                rotateDirection.z = 0;
            }
        }

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);

        if(!Input.GetKeyDown(KeyCode.KeypadPeriod))
        moveDirection = Quaternion.Euler(rotateDirection) * negDistance + targetPosition + targetOffset;
        else
        targetPosition = target;

        transform.rotation = Quaternion.Euler(rotateDirection);
        transform.position = moveDirection;
    }

    void RemoveBlockfromInventory()
    {
        if(inventory.Count != 0)
        {
            //keys.RemoveAt();
        }
        else
        return;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isBuildMode)
        Move();

        else
        MoveBuildMode();
    }
}
