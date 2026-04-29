using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Block : MonoBehaviour
{
    private InventoryManager inventoryManager;
    private Player playerScript;
    private MainScript mainScript;
    private GameObject player;

    private Material mainBlockMaterial;
    public Vector3 offset;
    public Vector3 curPosition;
    public Vector3 previousPosition;
    public GameObject colorChoosePanel;
    public GameObject place;
    private float bulgeWidth = 0.16f;
    public float blockHeight = 0.064f;

    private Vector3 playerRotation;
    public List<Transform> hollowChild;
    public List<Transform> bulgeChild;
    public List<Vector3> hollowChildCoordinat = new List<Vector3>();
    public List<Vector3> bulgeChildCoordinat = new List<Vector3>();
    public List<Vector3> hollowChildRotation = new List<Vector3>();
    public List<Vector3> bulgeChildRotation = new List<Vector3>();
    public Transform nearestBulge;
    public Transform nearestHollow;
    //public List<Transform> blockChild;
    public List<GameObject> previousBlock = new List<GameObject>();
    public bool isActive = false;
    public List<Vector3> positionHistory = new List<Vector3>();
    public bool isFree = true;
    public bool isMagnetic = false;
    public int countPoint = 0;               // число занятых вершин
    public static List<Connection> connections = new List<Connection>();
    public Vector3 moveVector = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 bulgePosition;
    public Vector3 localHollowPosition;
    public Vector3 localBulgePosition;
    public Vector3 placeHollowPosition;

    private Transform pendingSnapParent;     // блок, к которому присоединяем
    private Transform pendingSnapBlock;      // конкретный блок сборки, который участвует в соединении
    private Transform pendingSnapRoot;
    private int pendingSnapPoints;
    private bool hasPendingConnection;
    private BlockPoint pendingSnapBlockPoint; // точка на блоке сборки (шип или впадина)
    private BlockPoint pendingSnapOtherPoint; // точка на целевом блоке
    private Transform lastProcessedPlace = null;   // Чтобы не обрабатывать один и тот же place дважды
    private Vector3 originalRootPosition;

    private Vector2 lastMouseScreenPos;
    private bool hasStoredMousePos;

    private List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0f, 0f, 0f);

    public BlockData blockData;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mainScript = Camera.main.GetComponent<MainScript>();

        GameObject inventory = GameObject.FindGameObjectWithTag("InventoryManager");
        
        if(inventory != null)
        inventoryManager = inventory.GetComponent<InventoryManager>();

        if(player != null)
        playerScript = player.GetComponent<Player>();

        previousPosition = transform.position;
        positionHistory.Add(transform.position);
        previousRotate.Add(rotateDirection);

        FindChildPoint("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);
        FindChildPoint("Bulge", bulgeChildCoordinat, bulgeChildRotation, transform, bulgeChild);

        mainBlockMaterial = gameObject.GetComponent<Renderer>().material;
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

    private Transform FindMainParent(Transform currentObject)
    {
        while(currentObject.parent != null)
        currentObject = currentObject.parent;
        
        return currentObject;
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

    // Пересчёт всех точек на основе списка connections
    public void RecalculateAllPoints()
    {
        HashSet<Block> allBlocks = new HashSet<Block>();
        
        foreach(var conn in connections)
        {
            allBlocks.Add(conn.blockA);
            allBlocks.Add(conn.blockB);
        }
        
        if(!allBlocks.Contains(this))
        allBlocks.Add(this);

        foreach(Block b in allBlocks)
        b.countPoint = 0;

        foreach(var conn in connections)
        {
            conn.blockA.countPoint += conn.occupiedPoints;
            conn.blockB.countPoint += conn.occupiedPoints;
        }

        Debug.Log($"=== Recalculated {connections.Count} connections ===");
        
        foreach(Block b in allBlocks)
        {
            if(b.countPoint > 0)
            Debug.Log($"{b.name}: {b.countPoint} points");
        }
    }

    public void Move(float distance, GameObject currentObject)
    {
        if(hasPendingConnection)
        return;

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

            if(!hasStoredMousePos)
            {
                lastMouseScreenPos = Input.mousePosition;
                hasStoredMousePos = true;
            }

            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - lastMouseScreenPos;
            float threshold = 70f; // чувствительность

            bool moved = false;
            
            if(Mathf.Abs(delta.x) >= threshold)
            {
                int dir = delta.x > 0 ? 1 : -1;
                ApplyStepMove(movingObject, dir, threshold, ref delta, true);
                moved = true;
            }
            
            if(Mathf.Abs(delta.y) >= threshold)
            {
                int dir = delta.y > 0 ? 1 : -1;
                ApplyStepMove(movingObject, dir, threshold, ref delta, false);
                moved = true;
            }

            if(moved)
            lastMouseScreenPos = currentMousePos - delta;
        }

        PlaceObjectCorrectly(movingObject);
    }

    private void ApplyStepMove(Transform movingObject, int direction, float threshold, ref Vector2 delta, bool isHorizontal)
    {
        float stepSize = bulgeWidth;
        Vector3 move = Vector3.zero;
        float angle = playerRotation.y;
        angle = (angle % 360 + 360) % 360;

        if(isHorizontal)
        {
            if(angle >= 315 || angle <= 45)
            move = Vector3.right * direction;
            
            else if(angle >= 45 && angle <= 135)
            move = Vector3.forward * direction;
            
            else if(angle >= 135 && angle <= 225)
            move = Vector3.left * direction;
            
            else
            move = Vector3.back * direction;
        }
        
        else
        {
            // Вертикальное перемещение мыши – двигаем перпендикулярно горизонтальной оси
            Vector3 horizAxis;
        
            if(angle >= 315 || angle <= 45)
            horizAxis = Vector3.right;
            
            else if(angle >= 45 && angle <= 135)
            horizAxis = Vector3.forward;
            
            else if(angle >= 135 && angle <= 225)
            horizAxis = Vector3.left;
            
            else
            horizAxis = Vector3.back;

            Vector3 vertAxis = Quaternion.Euler(0, 90, 0) * horizAxis;
            move = vertAxis * -direction;
        }

        movingObject.position += move * stepSize;

        if(isHorizontal)
        delta.x -= direction * threshold;
        
        else
        delta.y -= direction * threshold;
    }

    private void PlaceObjectCorrectly(Transform movingObject)
    {
        List<Transform> transformHollows = new List<Transform>();
        
        foreach(Transform child in movingObject)
        {
            if(child.name == "Hollow")
            transformHollows.Add(child);
        }

        if(transformHollows.Count == 0)
        return;
        
        Vector3 hollowPosition = transformHollows[0].position;

        if(hollowPosition.x % bulgeWidth != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.x = hollowPosition.x % bulgeWidth;
        
        if(hollowPosition.y % blockHeight != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.y = hollowPosition.y % (4 * bulgeWidth / 10);
        
        if(hollowPosition.z % bulgeWidth != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.z = hollowPosition.z % bulgeWidth;
    }

    private void CalculateOffset(GameObject movingObject)
    {
        if(movingObject.GetComponent<Block>().offset == Vector3.zero)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance));
            movingObject.GetComponent<Block>().offset = movingObject.transform.position - mouseWorldPos;
        }
    }

    private bool FindNearestConnectionPoints(Block current, Block target, 
    out Transform nearestBulge, out Transform nearestHollow,
    out Vector3 localBulgePos, out Vector3 localHollowPos,
    out Vector3 targetWorldPoint)
    {
        nearestBulge = null;
        nearestHollow = null;
        localBulgePos = Vector3.zero;
        localHollowPos = Vector3.zero;
        targetWorldPoint = Vector3.zero;

        float minDistance = 100f;

        // 1) Впадина на current + шип на target
        foreach(Transform hollow in current.hollowChild)
        {
            foreach(Transform bulge in target.bulgeChild)
            {
                float d = Vector3.Distance(hollow.position, bulge.position);
                
                if(d < minDistance)
                {
                    minDistance = d;
                    nearestBulge = bulge;
                    nearestHollow = hollow;
                    localHollowPos = hollow.localPosition;
                    targetWorldPoint = bulge.position;
                }
            }
        }

        // 2) Шип на current + впадина на target
        foreach(Transform bulge in current.bulgeChild)
        {
            foreach(Transform hollow in target.hollowChild)
            {
                float d = Vector3.Distance(bulge.position, hollow.position);
                
                if(d < minDistance)
                {
                    minDistance = d;
                    nearestBulge = bulge;
                    nearestHollow = hollow;
                    localBulgePos = bulge.localPosition;
                    targetWorldPoint = hollow.position;
                }
            }
        }

        return nearestBulge != null && nearestHollow != null;
    }

    private void PrepareGroupConnection(GameObject currentBlock, GameObject place, Transform rootGroup)
    {
        if(rootGroup == null && currentBlock != null)
        rootGroup = FindMainParent(currentBlock.transform);
        
        if(rootGroup == null)
        return;

        Block rootBlock = rootGroup.GetComponent<Block>();
        
        if(rootBlock == null)
        return;

        // Блокируем повторные вызовы
        if(rootBlock.lastProcessedPlace == place.transform)
        return;

        Block current = currentBlock.GetComponent<Block>();
        Block target = place.GetComponent<Block>();
        
        if(current == null || target == null)
        return;
        
        if(IsChildRecursively(place, currentBlock))
        return;

        // Поиск ближайших точек
        if(!FindNearestConnectionPoints(current, target, out Transform nearestBulge, out Transform nearestHollow,
        out Vector3 localBulgePos, out Vector3 localHollowPos, out Vector3 targetWorldPoint))
        return;

        // Проверка свободных точек
        if(!nearestBulge.GetComponent<BlockPoint>().isFree || !nearestHollow.GetComponent<BlockPoint>().isFree)
        {
            Debug.Log("Cannot connect – point already occupied");
            return;
        }
        
        if(Vector3.Dot(current.hollowChild[0].up, target.bulgeChild[0].up) <= 0.99f)
        {
            Debug.Log("Axes not aligned");
            return;
        }

        // Определяем, сверху или снизу
        float yCurrent = current.bulgeChild.Count > 0 ? current.bulgeChild[0].position.y : current.transform.position.y;
        float yPlace = target.bulgeChild.Count > 0 ? target.bulgeChild[0].position.y : place.transform.position.y;
        bool isCurrentAbove = yCurrent > yPlace;

        Vector3 currentSourceWorld;
        
        if(isCurrentAbove)
        currentSourceWorld = currentBlock.transform.TransformPoint(localHollowPos);
        
        else
        currentSourceWorld = currentBlock.transform.TransformPoint(localBulgePos);

        Vector3 offset = targetWorldPoint - currentSourceWorld;

        originalRootPosition = rootGroup.position;

        // Применяем смещение
        rootGroup.position += offset;
        rootGroup.GetComponent<Block>().isFree = false;

        // Сбрасываем накопленное движение мыши у всех блоков сборки
        foreach(Block b in rootGroup.GetComponentsInChildren<Block>())
        {
            b.hasStoredMousePos = false;
            b.lastMouseScreenPos = Vector2.zero;
        }

        // Сохраняем данные для фиксации
        pendingSnapParent = place.transform;
        pendingSnapBlock = currentBlock.transform;
        pendingSnapPoints = CountOccupiedPointsBetween(currentBlock, place);
        pendingSnapBlockPoint = isCurrentAbove ? nearestHollow.GetComponent<BlockPoint>() : nearestBulge.GetComponent<BlockPoint>();
        pendingSnapOtherPoint = isCurrentAbove ? nearestBulge.GetComponent<BlockPoint>() : nearestHollow.GetComponent<BlockPoint>();
        hasPendingConnection = true;
        pendingSnapRoot = rootGroup;

        rootBlock.lastProcessedPlace = place.transform;

        Debug.Log($"PrepareGroupConnection: {currentBlock.name} -> {place.name}, offset={offset}");
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

                    // Визуальное выделение при соединении
                    if(!FindMainParent(transform).gameObject.GetComponent<Block>().isFree)
                    {
                        FindMainParent(transform).gameObject.GetComponent<MeshRenderer>().material = mainScript.rendgenMaterial;
                        //FindMainParent(transform).gameObject.GetComponent<Block>().blockChild.Clear();
                      
                        Block rootBlock = FindMainParent(transform).GetComponent<Block>();
                        rootBlock.GetComponent<MeshRenderer>().material = mainScript.rendgenMaterial;
                    
                        foreach(Block child in rootBlock.GetComponentsInChildren<Block>())
                        {
                            if(child != rootBlock)
                            child.GetComponent<MeshRenderer>().material = mainScript.rendgenMaterial;
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
        playerScript.target = transform.position + new Vector3(0f, -0.4f, 0f);
    }

    void OnMouseExit()
    {
        isActive = false;
        
        if(playerScript.isBuildMode)
        playerScript.target = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(hasPendingConnection)
        return; // не обрабатываем новое соединение, пока старое не завершено

        if(playerScript.isBuildMode && playerScript.movedObject != null)
        {
            Transform currentRoot = FindMainParent(transform);
            
            if(currentRoot == playerScript.movedObject.transform)
            {
                if(other.CompareTag("Selectable"))
                {
                    place = other.gameObject;
                    
                    if(!previousBlock.Contains(place))
                    previousBlock.Add(place);

                    Transform root = playerScript.movedObject.transform;
                    PrepareGroupConnection(gameObject, place, root);
                }
            }
        }
        
        else if(isFree && Input.GetMouseButtonUp(0))
        PrepareGroupConnection(gameObject, other.gameObject, null);
    }

    private void OnTriggerExit(Collider other)
    {
        // Если вышли из триггера, сбрасываем флаг у корня
        Transform root = FindMainParent(transform);

        if(root != null)
        {
            Block rootBlock = root.GetComponent<Block>();
            
            if(rootBlock != null)
            rootBlock.lastProcessedPlace = null;
        }

        // Если вышли из триггера до фиксации – откатываем позицию корня
        if(hasPendingConnection && pendingSnapRoot != null)
        {
            pendingSnapRoot.position = originalRootPosition;
            pendingSnapRoot.GetComponent<Block>().isFree = true;
        }

        // Сбрасываем флаги обработки
        lastProcessedPlace = null;
        hasPendingConnection = false;
        pendingSnapParent = null;
        pendingSnapRoot = null;

        if(!isMagnetic && transform.parent == null)
        {
            if(other.gameObject == place || other.gameObject == pendingSnapParent?.gameObject)
            {
                hasPendingConnection = false;
                pendingSnapParent = null;
                lastProcessedPlace = null;
                isFree = true;
                hasStoredMousePos = false;
                GetComponent<MeshRenderer>().material = mainBlockMaterial;

                moveVector = Vector3.zero;
                Transform parentRoot = FindMainParent(transform);
                
                if(parentRoot != null)
                parentRoot.GetComponent<Block>().moveVector = Vector3.zero;

                foreach(Transform child in FindMainParent(transform))
                {
                    Block childBlock = child.GetComponent<Block>();
                    
                    if(childBlock != null)
                    child.GetComponent<MeshRenderer>().material = childBlock.mainBlockMaterial;
                }

                place = null;
                Block thisBlock = GetComponent<Block>();
                Block otherBlock = other.GetComponent<Block>();
                connections.RemoveAll(c => (c.blockA == thisBlock && c.blockB == otherBlock) || (c.blockA == otherBlock && c.blockB == thisBlock));
            }
        }
        
        else if(other.gameObject == place)
        {
            hasPendingConnection = false;
            pendingSnapParent = null;
            lastProcessedPlace = null;
            isFree = true;
            hasStoredMousePos = false;
            place = null;

            moveVector = Vector3.zero;
            Transform parentRoot = FindMainParent(transform);
            
            if(parentRoot != null)
            parentRoot.GetComponent<Block>().moveVector = Vector3.zero;

            Block thisBlock = GetComponent<Block>();
            Block otherBlock = other.GetComponent<Block>();
            connections.RemoveAll(c => (c.blockA == thisBlock && c.blockB == otherBlock) || (c.blockA == otherBlock && c.blockB == thisBlock));
        }

        isMagnetic = false;

        foreach(Transform hollow in hollowChild)    
        hollow.gameObject.GetComponent<BlockPoint>().isFree = true;
        
        foreach(Transform bulge in bulgeChild)
        bulge.gameObject.GetComponent<BlockPoint>().isFree = true;

        if(previousBlock.Count != 0)
        previousBlock.RemoveAt(0);

        if(root != null)
        root.GetComponent<Block>().RecalculateAllPoints();
        
        else
        RecalculateAllPoints();

        pendingSnapRoot = null;
    }

    private void Rotation(Transform currentObject)
    {
        if(Input.GetKeyUp(KeyCode.UpArrow))
        currentObject.Rotate(Vector3.right * 90f, Space.World);
        
        if(Input.GetKeyUp(KeyCode.DownArrow))
        currentObject.Rotate(Vector3.right * -90f, Space.World);
        
        if(Input.GetKeyUp(KeyCode.LeftArrow))
        currentObject.Rotate(Vector3.up * -90f, Space.World);
        
        if(Input.GetKeyUp(KeyCode.RightArrow))
        currentObject.Rotate(Vector3.up * 90f, Space.World);
        
        rotateDirection = currentObject.rotation.eulerAngles;
    }

    void Update()
    {
        mainScript.MakeObjectGravity(gameObject);

        if(gameObject == playerScript.movedObject && Input.GetMouseButton(0) && offset != Vector3.zero)
        Move(playerScript.distance, gameObject);

        if(Input.GetMouseButtonUp(0) && !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            hasStoredMousePos = false;
            offset = Vector3.zero;

            GetComponent<MeshRenderer>().material = mainBlockMaterial;

            if(isFree && playerScript.movedObject == gameObject)
            {
                playerScript.movedObject.GetComponent<Block>().FindMainParent(transform).position -=
                FindMainParent(transform).gameObject.GetComponent<Block>().moveVector;
                
                playerScript.movedObject.GetComponent<Block>().FindMainParent(transform).gameObject.GetComponent<Block>().moveVector = Vector3.zero;
                playerScript.movedObject = null;
            }

            if(!isFree)
            {
                isMagnetic = true;
                
                if(place != null)
                {
                    if(hasPendingConnection && pendingSnapParent != null && playerScript.movedObject != null)
                    {
                        // Смещение уже было применено в PrepareGroupConnection, поэтому не нужно.
                        // Делаем корень (который мы перемещали) дочерним по отношению к pendingSnapParent
                        if(pendingSnapRoot != null)
                        pendingSnapRoot.SetParent(pendingSnapParent);
                        
                        else
                        playerScript.movedObject.transform.SetParent(pendingSnapParent);

                        Block blockA = pendingSnapBlock.GetComponent<Block>();
                        Block blockB = pendingSnapParent.GetComponent<Block>();
                        
                        if(blockA != null && blockB != null)
                        {
                            Connection newConn = new Connection
                            {
                                blockA = blockA,
                                blockB = blockB,
                                occupiedPoints = pendingSnapPoints
                            };
                        
                            if(!connections.Exists(c => (c.blockA == blockA && c.blockB == blockB) || (c.blockA == blockB && c.blockB == blockA)))
                            {
                                connections.Add(newConn);
                                Debug.Log($"Connection fixed between {blockA.name} and {blockB.name}, points={pendingSnapPoints}");
                            }
                        
                            if(pendingSnapBlockPoint != null)
                            pendingSnapBlockPoint.isFree = false;
                        
                            if(pendingSnapOtherPoint != null)
                            pendingSnapOtherPoint.isFree = false;
                        }
                        
                        hasPendingConnection = false;
    
                        if(pendingSnapRoot != null)
                        {
                            Block rootBlock = pendingSnapRoot.GetComponent<Block>();
                            
                            if(rootBlock != null)
                            rootBlock.lastProcessedPlace = null;
                        }
    
                        pendingSnapRoot = null;
                        lastProcessedPlace = null;
                    }
                    
                    else
                    {
                        // Fallback (одиночный блок)
                        transform.SetParent(place.transform);
                        Block parentBlock = place.GetComponent<Block>();
                        
                        if(parentBlock != null && !connections.Exists(c => (c.blockA == this && c.blockB == parentBlock) || (c.blockA == parentBlock && c.blockB == this)))
                        {
                            int points = CountOccupiedPointsBetween(gameObject, place);
                            connections.Add(new Connection { blockA = this, blockB = parentBlock, occupiedPoints = points });
                            Debug.Log($"Forced connection added between {name} and {place.name}, points={points}");
                        }
                    }
                    
                    Transform rootAfter = FindMainParent(transform);
                    
                    if(rootAfter != null)
                    rootAfter.GetComponent<Block>().RecalculateAllPoints();
                    
                    else
                    RecalculateAllPoints();
                }
            }

            // Обработка вторичных присоединений (например, когда несколько блоков одновременно)
            if(previousBlock.Count != 1)
            {
                foreach(GameObject block in previousBlock)
                {
                    if(block != null && block != transform.parent && !IsChildRecursively(block, gameObject))
                    {
                        block.transform.SetParent(transform);
                        // blockChild не используется для соединений, но можно обновить
                        block.GetComponent<Block>().place = gameObject;
                        block.GetComponent<Block>().isMagnetic = true;
                        previousPosition = transform.localPosition;
                    }
                }
            }

            FindChildPoint("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);
            FindChildPoint("Bulge", bulgeChildCoordinat, bulgeChildRotation, transform, bulgeChild);

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
                    Transform oldParent = transform.parent;
                    connections.RemoveAll(c => c.blockA == this || c.blockB == this);
                    transform.parent = null;
                    countPoint = 0;

                    moveVector = Vector3.zero;
                    foreach(Transform child in transform)
                    {
                        Block childBlock = child.GetComponent<Block>();
                        
                        if(childBlock != null)
                        childBlock.moveVector = Vector3.zero;
                    }
                  
                    foreach(Transform child in transform)
                    {
                        Block childBlock = child.GetComponent<Block>();
                        
                        if(childBlock != null)
                        childBlock.countPoint = 0;
                    }
                    
                    if(oldParent != null)
                    {
                        Block parentBlock = oldParent.GetComponent<Block>();
                        
                        if(parentBlock != null)
                        parentBlock.RecalculateAllPoints();
                    }
                    
                    else
                    RecalculateAllPoints();
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

        else if(Input.GetKey(KeyCode.R) && isActive && !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        Rotation(FindMainParent(transform));

        else if(isActive && Input.GetKeyUp(KeyCode.E) && transform.parent == null &&
        !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
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
            
            playerScript.target = Vector3.zero;
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
            }
        }
    }

    private void SavePosition(Vector3 position)
    {
        positionHistory.Add(position);
        
        if(positionHistory.Count > 100)
        positionHistory.RemoveAt(0);
    }
}

[System.Serializable]
public class Connection
{
    public Block blockA;
    public Block blockB;
    public int occupiedPoints;
}