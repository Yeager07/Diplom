using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static System.Math;

using System.Linq;

public class Block : MonoBehaviour
{
    private InventoryManager inventoryManager;
    private Player playerScript;
    private MainScript mainScript;
    private GameObject player;
    private Vector3 pointScreen;
    private List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 bulgePosition;
    public Vector3 localHollowPosition;
    public Vector3 localBulgePosition;
    public Vector3 placeHollowPosition;
    private Material mainBlockMaterial;
    public Vector3 offset;
    private float distance;
    public Vector3 moveVector = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 curPosition;
    public Vector3 previousPosition;
    public GameObject colorChoosePanel;
    public GameObject place;
    private float bulgeWidth = 0.16f;
    public float blockHeight = 0.064f;
    private Vector3 moveX = new Vector3(0.16f, 0.0f, 0.0f);
    private Vector3 moveY = new Vector3(0.0f, 0.16f, 0.0f);
    private Vector3 moveZ = new Vector3(0.0f, 0.0f, 0.16f);
    private Vector3 playerRotation;
    public float xDistance = 0.0f;
    public float zDistance = 0.0f;
    public List<Transform> hollowChild;
    public List<Transform> bulgeChild;
    public List<Vector3> hollowChildCoordinat = new List<Vector3>();
    public List<Vector3> bulgeChildCoordinat = new List<Vector3>();
    public List<Vector3> hollowChildRotation = new List<Vector3>();
    public List<Vector3> bulgeChildRotation = new List<Vector3>();
    public Transform nearestBulge;
    public List<Transform> blockChild;
    public List<GameObject> previousBlock = new List<GameObject>();
    public bool isActive = false;
    public List<Vector3> positionHistory = new List<Vector3>();
    public bool isFree = true;
    public bool isMagnetic = false;
    public int countPoint = 0; //Число занятых вершин
    public static List<Connection> connections = new List<Connection>();

    private Vector3 pendingSnapPosition;
    private Transform pendingSnapParent;
    private int pendingSnapPoints;
    private bool hasPendingConnection;

    private Vector2 lastMouseScreenPos; // последняя позиция мыши в экранных координатах
    private bool hasStoredMousePos;     // флаг, что позиция сохранена


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mainScript = Camera.main.GetComponent<MainScript>();

        GameObject inventory = GameObject.FindGameObjectWithTag("InventoryManager");
        if(inventory != null)
        inventoryManager = inventory.GetComponent<InventoryManager>();

        if (player != null)
        playerScript = player.GetComponent<Player>();

        previousPosition = transform.position;
        positionHistory.Add(transform.position);
        previousRotate.Add(rotateDirection);

        FindChildPoint("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);
        FindChildPoint("Bulge", bulgeChildCoordinat, bulgeChildRotation, transform, bulgeChild);

        mainBlockMaterial = gameObject.GetComponent<Renderer>().material;
    }

    private Transform FindMainParent(Transform currentObject)
    {
        while(currentObject.parent != null)
        {
            currentObject = currentObject.parent;
        }
        
        return currentObject;
    }

    private void FindChildPoint(string childName, List<Vector3> childCoordinat, List<Vector3> childRotation, Transform objectTransform, List<Transform> massiveChild)
    {
        childCoordinat.Clear();
        childRotation.Clear();
        massiveChild.Clear();

        foreach(Transform child in objectTransform)
        {
            if(child.name == childName)
            {
                childCoordinat.Add(child.position);
                childRotation.Add(transform.localRotation.eulerAngles);
                massiveChild.Add(child);
            }
        }
    }

    private int CountOccupiedPointsBetween(GameObject blockA, GameObject blockB)
    {
        int occupied = 0;
        
        foreach(Transform bulge in blockA.GetComponent<Block>().bulgeChild)
        {
            foreach(Transform hollow in blockB.GetComponent<Block>().hollowChild)
            {
                if(bulge.position == hollow.position)
                occupied++;
            }
        }
        foreach(Transform bulge in blockB.GetComponent<Block>().bulgeChild)
        {
            foreach(Transform hollow in blockA.GetComponent<Block>().hollowChild)
            {
                if(bulge.position == hollow.position)
                occupied++;
            }
        }
        return occupied;
    }

    public void RecalculateAllPoints()
    {
        HashSet<Block> allBlocks = new HashSet<Block>();
        foreach (var conn in connections)
        {
            allBlocks.Add(conn.blockA);
            allBlocks.Add(conn.blockB);
        }
        
        foreach (Block b in allBlocks)
        b.countPoint = 0;
        
        foreach (var conn in connections)
        {
            conn.blockA.countPoint += conn.occupiedPoints;
            conn.blockB.countPoint += conn.occupiedPoints;
        }
        Debug.Log($"=== Recalculated {connections.Count} connections ===");
        foreach (Block b in allBlocks)
        {
            if (b.countPoint > 0)
            Debug.Log($"{b.name}: {b.countPoint} points");
        }
    }

    void SavePosition(Vector3 position)
    {
        positionHistory.Add(position);

        if(positionHistory.Count > 100)
        positionHistory.RemoveAt(0);
    }

    void PlaceObjectCorrectly(Transform movingObject)
    {
        List<Transform> transformHollows = new List<Transform>();

        foreach(Transform child in movingObject)
        {
            if(child.name == "Hollow")
            transformHollows.Add(child);
        }

        Vector3 hollowPosition = transformHollows[0].position;
        
        if(hollowPosition.x % bulgeWidth != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.x = hollowPosition.x % bulgeWidth;

        if(hollowPosition.y % blockHeight != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.y = hollowPosition.y % (4 * bulgeWidth / 10);

        if(hollowPosition.z % bulgeWidth != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.z = hollowPosition.z % bulgeWidth;
    }

    private void CalculateMoveVector(float distance, Transform movingObject, Vector3 moveSide, Vector3 moveHeight)
    {
        if(distance > 0)
        {
            if(distance == movingObject.gameObject.GetComponent<Block>().xDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position += moveSide;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position -= moveHeight;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position -= moveSide;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position += moveHeight;

                movingObject.gameObject.GetComponent<Block>().xDistance = 0.0f;
            }
            
            else if(distance == movingObject.gameObject.GetComponent<Block>().zDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position += moveHeight;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position += moveSide;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position -= moveHeight;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position -= moveSide;
                
                movingObject.gameObject.GetComponent<Block>().zDistance = 0.0f;
            }
        }

        else if (distance < 0)
        {
            if(distance == movingObject.gameObject.GetComponent<Block>().xDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position -= moveSide;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position += moveHeight;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position += moveSide;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position -= moveHeight;

                movingObject.gameObject.GetComponent<Block>().xDistance = 0.0f;
            }
            
            else if(distance == movingObject.gameObject.GetComponent<Block>().zDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position -= moveHeight;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position -= moveSide;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position += moveHeight;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position += moveSide;

                movingObject.gameObject.GetComponent<Block>().zDistance = 0.0f;
            }
        }
    }
    
    public void Move(float distance, GameObject currentObject)
    {
        Transform movingObject = currentObject.transform;
        Block blockScript = currentObject.GetComponent<Block>();

        if(blockScript.isFree)
        {   
            blockScript.curPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
            
            if(playerScript.isBuildMode)
            movingObject.position = Camera.main.ScreenToWorldPoint(curPosition) + blockScript.offset;

            else
            movingObject.position = Camera.main.ScreenToWorldPoint(curPosition);
        }

        else
        {
            playerRotation = playerScript.transform.rotation.eulerAngles;
            
            // Шаговое перемещение с привязкой к перемещению мыши
            if(!hasStoredMousePos)
            {
                lastMouseScreenPos = Input.mousePosition;
                hasStoredMousePos = true;
            }

            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - lastMouseScreenPos;
            float threshold = 75f; // пикселей для одного шага (настройте под ваши ощущения)

            bool moved = false;
            
            // Обработка горизонтального перемещения (влияет на X или Z в зависимости от поворота камеры)
            if(Mathf.Abs(delta.x) >= threshold)
            {
                int dir = delta.x > 0 ? 1 : -1;
                ApplyStepMove(movingObject, dir, threshold, ref delta, true);
                moved = true;
            }
            
            // Обработка вертикального перемещения (если нужно)
            if(Mathf.Abs(delta.y) >= threshold)
            {
                int dir = delta.y > 0 ? 1 : -1;
                ApplyStepMove(movingObject, dir, threshold, ref delta, false);
                moved = true;
            }

            // Корректируем накопленное положение мыши, оставляя остаток
            if(moved)
            lastMouseScreenPos = currentMousePos - delta;
        }

        PlaceObjectCorrectly(movingObject);
    }

    private void ApplyStepMove(Transform movingObject, int direction, float threshold, ref Vector2 delta, bool isHorizontal)
    {
        float stepSize = bulgeWidth; // 0.16
        Vector3 move = Vector3.zero;
        float angle = playerRotation.y;
        // Нормализуем угол в диапазон [0, 360)
        angle = (angle % 360 + 360) % 360;

        if (isHorizontal)
        {
            // Горизонтальное перемещение мыши -> влево/вправо относительно камеры
            if (angle >= 315 || angle <= 45)
            move = Vector3.right * direction;
    
            else if (angle >= 45 && angle <= 135)
            move = Vector3.forward * direction;
            
            else if (angle >= 135 && angle <= 225)
            move = Vector3.left * direction;
            
            else // 225..315
            move = Vector3.back * direction;
        }
    
        else
        {
            // Вертикальное перемещение мыши -> вперёд/назад относительно камеры
            // Определяем горизонтальную ось так же, как выше
            Vector3 horizAxis;
        
            if (angle >= 315 || angle <= 45)
            horizAxis = Vector3.right;
    
            else if (angle >= 45 && angle <= 135)
            horizAxis = Vector3.forward;
        
            else if (angle >= 135 && angle <= 225)
            horizAxis = Vector3.left;
            
            else
            horizAxis = Vector3.back;

            // Поворачиваем горизонтальную ось на 90° вокруг глобальной оси Y, получаем вертикальную ось
            // Например, из right получим forward, из forward -> left, и т.д.
            Vector3 vertAxis = Quaternion.Euler(0, 90, 0) * horizAxis;
            move = vertAxis * -direction;
        }

        movingObject.position += move * stepSize;

        // Корректируем накопленное движение мыши
        if (isHorizontal)
        delta.x -= direction * threshold;
        
        else
        delta.y -= direction * threshold;
    }

    private void CalculateOffset(GameObject movingObject)
    {
        if(movingObject.GetComponent<Block>().offset == new Vector3(0.0f, 0.0f, 0.0f))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance));
            movingObject.GetComponent<Block>().offset = movingObject.transform.position - mouseWorldPos;        
        }
    }

    private bool IsChildRecursively(GameObject potentialParent, GameObject potentialChild)
    {
        Transform current = potentialChild.transform;
        
        while(current != null)
        {
            if(current == potentialParent.transform)
            return true;
            
            current = current.parent;
        }
        
        return false;
    }

    void OnMouseDown()
    {
        hasStoredMousePos = false;
        hasPendingConnection = false;

        if(FindMainParent(transform) == transform && playerScript.isBuildMode)
        CalculateOffset(gameObject);
    }

    void OnMouseDrag()
    {   
        if((playerScript.colorChoosePanel == null || !playerScript.colorChoosePanel.gameObject.activeInHierarchy) &&
        !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            if(Input.GetMouseButton(0))
            playerScript.movedObject = FindMainParent(transform).gameObject;

            playerScript.distance += Input.GetAxis("Mouse ScrollWheel") * 4.0f;
    
            if(playerScript.isBuildMode)
            {   
                if(!Input.GetKey(KeyCode.R))
                {
                    if(FindMainParent(transform) == transform)
                    Move(playerScript.distance, gameObject);

                    else
                    {       
                        CalculateOffset(FindMainParent(transform).gameObject);
                        Move(playerScript.distance, FindMainParent(transform).gameObject);
                    }

                    if(!FindMainParent(transform).gameObject.GetComponent<Block>().isFree)
                    {
                        FindMainParent(transform).gameObject.GetComponent<MeshRenderer>().material = mainScript.rendgenMaterial;

                        FindMainParent(transform).gameObject.GetComponent<Block>().blockChild.Clear();

                        foreach(Transform child in FindMainParent(transform))
                        {
                            if(child.GetComponent<Block>() != null)
                            {
                                FindMainParent(transform).gameObject.GetComponent<Block>().blockChild.Add(child);
                                child.GetComponent<MeshRenderer>().material = mainScript.rendgenMaterial;
                            }
                        }
                    }
                }
            }
    
            else
            Move(playerScript.minDistance, gameObject);
        }
    }

    void OnMouseEnter()
    {
        isActive = true;

        if(playerScript.isBuildMode)
        playerScript.target = transform.position + new Vector3(0.0f, -0.4f, 0.0f);
    }

    void OnMouseExit()
    {
        isActive = false;
        
        if(playerScript.isBuildMode)
        playerScript.target = new Vector3(0.0f, 0.0f, 0.0f);
    }

    private void CalculateDistance(GameObject currentBlock, GameObject place, List<Transform> hollows, List<Transform> bulges)
    {
        float minDistance = 100.0f;
        List<Transform> transformBulges = new List<Transform>();
        
        foreach(Transform child in currentBlock.transform)
        transformBulges.Add(child);

        int jterator = 0;
        
        while(jterator < hollows.Count)
        {
            int iterator = 0;
            
            while(iterator < bulges.Count)
            {
                float distance = (float)Sqrt(Pow(hollows[jterator].position.x - bulges[iterator].position.x, 2) + Pow(hollows[jterator].position.y - bulges[iterator].position.y, 2) + Pow(hollows[jterator].position.z - bulges[iterator].position.z, 2));
                
                if(distance < minDistance)
                {
                    minDistance = distance;
                    currentBlock.GetComponent<Block>().localHollowPosition = hollows[jterator].localPosition;
                    currentBlock.GetComponent<Block>().localBulgePosition = transformBulges[jterator].localPosition;
                    currentBlock.GetComponent<Block>().bulgePosition = bulges[iterator].position;
                    currentBlock.GetComponent<Block>().nearestBulge = bulges[iterator];
                    currentBlock.GetComponent<Block>().placeHollowPosition = place.transform.GetComponent<Block>().hollowChild[iterator].position;
                }

                iterator += 1;
            }

            jterator += 1;
        }
    }

    /*private void MakeConnection(GameObject currentBlock, GameObject place)
    {
        Block currentBlockScript = currentBlock.GetComponent<Block>();
        Block placeBlockScript = place.GetComponent<Block>();

        // Проверка на циклическую ссылку (чтобы не создать петлю в иерархии)
        if(IsChildRecursively(place, currentBlock))
        {
            Debug.LogWarning($"Cannot connect {currentBlock.name} to {place.name} – would create a cycle.");
            return;
        }

        if(currentBlockScript.bulgeChildCoordinat.Count != 0 || currentBlockScript.hollowChildCoordinat.Count != 0)
        {
            CalculateDistance(currentBlock, place, currentBlockScript.hollowChild, placeBlockScript.bulgeChild);

            if(currentBlockScript.nearestBulge != null && Vector3.Dot(currentBlockScript.hollowChild[0].up, currentBlockScript.nearestBulge.up) > 0.99f)
            {
                currentBlockScript.isFree = false;

                // Вычисляем новую позицию
                Vector3 newPosition;
            
                if(place.name.Split(" ")[0] == "Tile" && currentBlock.name.Split(" ")[0] == "Tile")
                return;
                
                else if(place.name.Split(" ")[0] == "Tile")
                newPosition = currentBlockScript.placeHollowPosition - currentBlock.transform.TransformVector(currentBlockScript.localBulgePosition);
                
                else if(currentBlock.name.Split(" ")[0] == "Tile")
                newPosition = currentBlockScript.bulgePosition - currentBlock.transform.TransformVector(currentBlockScript.localHollowPosition);
                
                else
                {
                    if(currentBlockScript.bulgeChild[0].position.y < placeBlockScript.bulgeChild[0].position.y)
                    newPosition = currentBlockScript.placeHollowPosition - currentBlock.transform.TransformVector(currentBlockScript.localBulgePosition);
                    
                    else
                    newPosition = currentBlockScript.bulgePosition - currentBlock.transform.TransformVector(currentBlockScript.localHollowPosition);
                }

                currentBlock.transform.position = newPosition;
                currentBlock.transform.SetParent(place.transform);

                int points = CountOccupiedPointsBetween(currentBlock, place);
                Connection newConn = new Connection
                {
                    blockA = currentBlockScript,
                    blockB = placeBlockScript,
                    occupiedPoints = points
                };

                if(!connections.Exists(c => (c.blockA == currentBlockScript && c.blockB == placeBlockScript) ||
                (c.blockA == placeBlockScript && c.blockB == currentBlockScript)))
                {
                    connections.Add(newConn);
                    Debug.Log($"Connection added between {currentBlock.name} and {place.name}, points={points}");
                }

                // Обновляем вспомогательный список blockChild (если нужен для других целей)
                if(!placeBlockScript.blockChild.Contains(currentBlock.transform))
                placeBlockScript.blockChild.Add(currentBlock.transform);
            }
        }
    }*/

    private void PrepareConnection(GameObject currentBlock, GameObject place)
    {
        Block currentBlockScript = currentBlock.GetComponent<Block>();
        Block placeBlockScript = place.GetComponent<Block>();

        if(IsChildRecursively(place, currentBlock))
        return;

        if(currentBlockScript.bulgeChildCoordinat.Count != 0 || currentBlockScript.hollowChildCoordinat.Count != 0)
        {
            CalculateDistance(currentBlock, place, currentBlockScript.hollowChild, placeBlockScript.bulgeChild);

            if(currentBlockScript.nearestBulge != null && Vector3.Dot(currentBlockScript.hollowChild[0].up, currentBlockScript.nearestBulge.up) > 0.99f)
            {
                Debug.Log($"PrepareConnection: {currentBlock.name} with {place.name}, points={pendingSnapPoints}");
                // Вычисляем новую позицию
                Vector3 newPosition;
    
                if(place.name.Split(" ")[0] == "Tile" && currentBlock.name.Split(" ")[0] == "Tile")
                return;
        
                else if(place.name.Split(" ")[0] == "Tile")
                newPosition = currentBlockScript.placeHollowPosition - currentBlock.transform.TransformVector(currentBlockScript.localBulgePosition);
            
                else if(currentBlock.name.Split(" ")[0] == "Tile")
                newPosition = currentBlockScript.bulgePosition - currentBlock.transform.TransformVector(currentBlockScript.localHollowPosition);
                
                else
                {
                    if(currentBlockScript.bulgeChild[0].position.y < placeBlockScript.bulgeChild[0].position.y)
                    newPosition = currentBlockScript.placeHollowPosition - currentBlock.transform.TransformVector(currentBlockScript.localBulgePosition);
            
                    else
                    newPosition = currentBlockScript.bulgePosition - currentBlock.transform.TransformVector(currentBlockScript.localHollowPosition);
                }

                // Мгновенно перемещаем деталь в примагниченную позицию
                currentBlock.transform.position = newPosition;
                // Включаем шаговое перемещение
                currentBlockScript.isFree = false;

                // Запоминаем данные для фиксации после отпускания мыши
                pendingSnapPosition = newPosition;
                pendingSnapParent = place.transform;
                pendingSnapPoints = CountOccupiedPointsBetween(currentBlock, place);
                hasPendingConnection = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(playerScript.isBuildMode /*&& !isMagnetic*/ && gameObject == playerScript.movedObject)
        {
            if(other.CompareTag("Selectable"))
            {
                place = other.gameObject;
                
                if(!previousBlock.Contains(place))
                previousBlock.Add(place);

                if(FindMainParent(transform) != null)
                PrepareConnection(FindMainParent(transform).gameObject, place);
            
                else
                PrepareConnection(gameObject, place);
            }
        }

        else if(isFree && Input.GetMouseButtonUp(0))
        PrepareConnection(gameObject, other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if(!isMagnetic && transform.parent == null)
        {
            if(other.gameObject == place || other.gameObject == pendingSnapParent?.gameObject)
            {
                hasPendingConnection = false;
                pendingSnapParent = null;
                isFree = true;  // <--- возвращаем свободное перемещение
                hasStoredMousePos = false;
                GetComponent<MeshRenderer>().material = mainBlockMaterial;

                foreach(Transform child in FindMainParent(transform))
                {
                    if(child.GetComponent<Block>() != null)
                    child.GetComponent<MeshRenderer>().material = child.GetComponent<Block>().mainBlockMaterial;
                }

                place = null;
                Block thisBlock = GetComponent<Block>();
                Block otherBlock = other.GetComponent<Block>();
                connections.RemoveAll(c => (c.blockA == thisBlock && c.blockB == otherBlock) || (c.blockA == otherBlock && c.blockB == thisBlock));
                otherBlock.blockChild.Remove(transform);
            }
        }
    
        else if(other.gameObject == place)
        {
            hasPendingConnection = false;
            pendingSnapParent = null;
            transform.parent = null;
            isFree = true;
            hasStoredMousePos = false;
            place = null;
            
            Block thisBlock = GetComponent<Block>();
            Block otherBlock = other.GetComponent<Block>();
            connections.RemoveAll(c => (c.blockA == thisBlock && c.blockB == otherBlock) || (c.blockA == otherBlock && c.blockB == thisBlock));
            otherBlock.blockChild.Remove(transform);
        }

        isMagnetic = false;

        foreach(Transform hollow in hollowChild)
        hollow.gameObject.GetComponent<BlockPoint>().isFree = true;
        
        foreach(Transform bulge in bulgeChild)
        bulge.gameObject.GetComponent<BlockPoint>().isFree = true;

        if(previousBlock.Count != 0)
        previousBlock.RemoveAt(0);

        Transform root = FindMainParent(transform);

        if(root != null)
        root.GetComponent<Block>().RecalculateAllPoints();
        
        else
        RecalculateAllPoints();
    }

    /*private void OnTriggerExit(Collider other)
    {
        if(!isMagnetic && transform.parent == null)
        {   
            if(other.gameObject == place)
            {
                GetComponent<MeshRenderer>().material = mainBlockMaterial;

                if(FindMainParent(transform).GetComponent<Block>().blockChild.Count == 0)
                mainScript.FindAllChild(FindMainParent(transform), FindMainParent(transform).GetComponent<Block>().blockChild);

                foreach(Transform child in FindMainParent(transform))
                {
                    if(child.GetComponent<Block>() != null)
                    child.GetComponent<MeshRenderer>().material = child.GetComponent<Block>().mainBlockMaterial;
                }
                
                isFree = true;
                place = null;

                /*if(blockChild.Count != 0)
                blockChild.Remove(other.transform);
            }
        }

        else if(other.gameObject == place)
        {
            transform.parent = null;
            isFree = true;

            other.gameObject.GetComponent<Block>().blockChild.Remove(transform);

            /*if(blockChild.Count != 0)
            blockChild.Remove(other.transform);
        }

        isMagnetic = false;
        
        foreach(Transform hollow in hollowChild)
        hollow.gameObject.GetComponent<BlockPoint>().isFree = true;

        foreach(Transform bulge in bulgeChild)
        bulge.gameObject.GetComponent<BlockPoint>().isFree = true;

        if(previousBlock.Count != 0)
        previousBlock.RemoveAt(0);

        /*if(blockChild.Count != 0)
        blockChild.Remove(other.transform);

        Block thisBlock = GetComponent<Block>();
        Block otherBlock = other.GetComponent<Block>();
        connections.RemoveAll(c => (c.blockA == thisBlock && c.blockB == otherBlock) || (c.blockA == otherBlock && c.blockB == thisBlock));

        FindMainParent(transform).GetComponent<Block>().RecalculateAllPoints();
    }*/

    private void Rotation(Transform currentObject)
    {
        if(Input.GetKeyUp(KeyCode.UpArrow))
        currentObject.Rotate(Vector3.right * 90.0f, Space.World);
            
        if(Input.GetKeyUp(KeyCode.DownArrow))
        currentObject.Rotate(Vector3.right * (-90.0f), Space.World);
            
        if(Input.GetKeyUp(KeyCode.LeftArrow))
        currentObject.Rotate(Vector3.up * (-90.0f), Space.World);

        if(Input.GetKeyUp(KeyCode.RightArrow))
        currentObject.Rotate(Vector3.up * 90.0f, Space.World);

        rotateDirection = currentObject.rotation.eulerAngles;
    }

    /*void FixedUpdate()
    {   
    }*/

    // Update is called once per frame
    void Update()
    {   
        Camera.main.GetComponent<MainScript>().MakeObjectGravity(gameObject);

        if(gameObject == playerScript.movedObject && Input.GetMouseButton(0) && offset != new Vector3 (0.0f, 0.0f, 0.0f))
        Move(playerScript.distance, gameObject);

        if(Input.GetMouseButtonUp(0) && !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {  
            hasStoredMousePos = false;
            
            offset = new Vector3(0.0f, 0.0f, 0.0f);
            
            GetComponent<MeshRenderer>().material = mainBlockMaterial;

            if(isFree && playerScript.movedObject == gameObject)
            {
                playerScript.movedObject.GetComponent<Block>().FindMainParent(transform).position -= FindMainParent(transform).gameObject.GetComponent<Block>().moveVector;
                playerScript.movedObject.GetComponent<Block>().FindMainParent(transform).gameObject.GetComponent<Block>().moveVector = new Vector3(0.0f, 0.0f, 0.0f);                
                playerScript.movedObject = null;
            }

            if(!isFree)
            {
                isMagnetic = true;
                
                if(place != null)
                {
                    transform.SetParent(place.transform);
                    // Проверяем, есть ли уже соединение между этим блоком и newParent
                    Block parentBlock = place.GetComponent<Block>();
                    
                    if(parentBlock != null && !connections.Exists(c => (c.blockA == this && c.blockB == parentBlock) || (c.blockA == parentBlock && c.blockB == this)))
                    {
                        int points = CountOccupiedPointsBetween(gameObject, place);
                        connections.Add(new Connection { blockA = this, blockB = parentBlock, occupiedPoints = points });
                        Debug.Log($"Forced connection added between {name} and {place.name}, points={points}");
                    }
                    // Пересчитываем очки для всей сборки (корня)
                    Transform root = FindMainParent(transform);
                    
                    if(root != null)
                    root.GetComponent<Block>().RecalculateAllPoints();
                    
                    else
                    RecalculateAllPoints();
                }
            }

            if (previousBlock.Count != 1)
            {
                foreach (GameObject block in previousBlock)
                {
                    if (block != null && block != transform.parent && !IsChildRecursively(block, gameObject))
                    {
                        block.transform.SetParent(transform);
                        // blockChild мы больше не используем для подсчёта, но если он нужен для других целей, оставьте
                        // blockChild.Add(block.transform);
                        block.GetComponent<Block>().place = gameObject;
                        block.GetComponent<Block>().isMagnetic = true;
                        previousPosition = transform.localPosition;
                    }
                }
            }
            
            FindChildPoint("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);
            FindChildPoint("Bulge", bulgeChildCoordinat, bulgeChildRotation, transform, bulgeChild);

            if(hasPendingConnection && pendingSnapParent != null)
            {
                Debug.Log($"Fixing connection between {name} and {pendingSnapParent.name}, points={pendingSnapPoints}");
                transform.position = pendingSnapPosition;
                transform.SetParent(pendingSnapParent);
                Connection newConn = new Connection
                {
                    blockA = this,
                    blockB = pendingSnapParent.GetComponent<Block>(),
                    occupiedPoints = pendingSnapPoints
                };
                
                if(!connections.Exists(c => (c.blockA == this && c.blockB == pendingSnapParent.GetComponent<Block>()) ||
                (c.blockA == pendingSnapParent.GetComponent<Block>() && c.blockB == this)))
                {
                    connections.Add(newConn);
                    Debug.Log($"Connection fixed between {name} and {pendingSnapParent.name}, points={pendingSnapPoints}");
                }
                
                if(!pendingSnapParent.GetComponent<Block>().blockChild.Contains(transform))
                pendingSnapParent.GetComponent<Block>().blockChild.Add(transform);
            
                hasPendingConnection = false;
                //place = null;
                RecalculateAllPoints();
            }

            if(transform.position != previousPosition)
            {
                SavePosition(previousPosition);
                previousPosition = transform.localPosition;
            }

            else
            SavePosition(transform.localPosition);
        }

        else if(Input.GetKeyUp(KeyCode.M))
        {
            if(isActive)
            {
        
                if(isMagnetic)
                {
                    isMagnetic = false;
                    // Запоминаем старого родителя до отсоединения
                    Transform oldParent = transform.parent;
                    // Удаляем все соединения, где участвует этот блок
                    connections.RemoveAll(c => c.blockA == this || c.blockB == this);
                    transform.parent = null;
                    // Обнуляем счётчик этого блока
                    countPoint = 0;
                    // Обнуляем счётчики всех его дочерних блоков
                    foreach(Transform child in transform)
                    {
                        Block childBlock = child.GetComponent<Block>();
                        
                        if(childBlock != null)
                        childBlock.countPoint = 0;
                    }
                    // Пересчитываем очки для старого родителя, если он существует и является блоком
                    if(oldParent != null)
                    {
                        Block parentBlock = oldParent.GetComponent<Block>();
                        
                        if(parentBlock != null)
                        parentBlock.RecalculateAllPoints();
                    }
                    else
                    {
                        // Если у блока не было родителя, то пересчитать его самого (хотя соединений нет)
                        RecalculateAllPoints();
                    }
                }
                
                else
                {
                    isFree = true;
                    Rigidbody rb = transform.GetComponent<Rigidbody>();
                    
                    if(rb != null)
                    rb.constraints = RigidbodyConstraints.None;
                }
            }
        }

        else if(Input.GetKeyUp(KeyCode.R) && !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(new Vector3(rotateDirection.x % 360, rotateDirection.y % 360, rotateDirection.z));
        }

        else if(Input.GetKey(KeyCode.R) && isActive &&
        !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        Rotation(FindMainParent(transform));

        else if(isActive && Input.GetKeyUp(KeyCode.E) && blockChild.Count == 0 &&
        transform.parent == null && !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            if(inventoryManager.inventory.Count != 0)
            {
                foreach(var value in inventoryManager.inventory)
                {
                    if(value.Value == 0)
                    inventoryManager.inventory.Remove(value.Key);
                }
                
                inventoryManager.AddToInventory(transform);
            }
            
            else
            inventoryManager.AddToInventory(transform);
            
            playerScript.target = new Vector3(0.0f, 0.0f, 0.0f);
        }

        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.I) &&
        !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            if(positionHistory.Count >= 1)
            {
                if(previousRotate.Count > 1 && previousRotate.Contains(positionHistory[positionHistory.Count - 1]))
                {
                    if(transform.parent == null)
                    {
                        transform.rotation = Quaternion.Euler(positionHistory[positionHistory.Count - 1]);
                        rotateDirection = positionHistory[positionHistory.Count - 1];
                    }

                    previousRotate.RemoveAt(previousRotate.Count - 1);
                }

                else
                {
                    if(Math.Abs(positionHistory[positionHistory.Count - 1].x) < 0.1f)
                    transform.localPosition = positionHistory[positionHistory.Count - 1];

                    else
                    {
                        transform.parent = null;
                        transform.localPosition = positionHistory[positionHistory.Count - 1];
                    }
                }

                if(positionHistory.Count > 1)
                positionHistory.RemoveAt(positionHistory.Count - 1);

                previousPosition = transform.position;

                return;
            }
        }
    }
}

[System.Serializable]
public class Connection
{
    public Block blockA;
    public Block blockB;
    public int occupiedPoints;
}