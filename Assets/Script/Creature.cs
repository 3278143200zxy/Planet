using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CreatureState
{
    Air,
    Idle,
    Walk,
    CutTree,
    MineStone,
    Build,
}
public enum ProfessionType
{
    None,
    Lumberjack,
}

[System.Serializable]
public class GameObjectList
{
    public List<GameObject> gameObjects = new List<GameObject>();
}
[System.Serializable]
public class ProfessionShowItemDictionary : SerializableDictionary<ProfessionType, GameObjectList> { }

public class Creature : BaseUnit
{
    [Header("Creature")]

    public CreatureState creatureState;
    private Animator animator;
    public float lastNormalizedTime;
    public PolarCoord polarCoord
    {
        get { return planet.PosToPolarCoord(transform.position); }
    }
    public Cell lastCurrentCell;
    public float currentAngle
    {
        get { return Vector2.SignedAngle(Vector2.right, transform.position - planet.transform.position); }
    }
    public Vector3 velocity;
    public float creatureHeight;

    public Task task;
    public bool isSettingTask = false;

    public float idleWalkSpeed;
    public float minIdleWalkInterval, maxIdleWalkInterval;
    private float idleWalkTimer, idleWalkInterval;
    public float idleWalkAngleOffset;
    public bool isIdleWalking;

    [HideInInspector] public List<Cell> path = new List<Cell>();
    public float walkSpeed;
    public float climbSpeed;

    public List<TaskType> priorityTaskTypes = new List<TaskType>();

    public float processPerCutTree;
    [HideInInspector] public Wood wood;
    public float processPreMineStone;
    [HideInInspector] public Stone stone;
    public float processPerBuildPlacedObject;

    public Item reservedItem;
    public Warehouse reservedWarehouse;
    public Transform itemPos;

    [Header("Profession")]
    public ProfessionType professionType = ProfessionType.None;
    public ProfessionShowItemDictionary professionTypeToGameobjects = new ProfessionShowItemDictionary();
    public override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        task = null;
    }
    public override void Start()
    {
        base.Start();

        QtreeManager.instance.AddBaseUnit(this);

        planet = MouseManager.instance.planets[0];
        lastCurrentCell = currentCell;
        //ChangeCreatureState(CreatureState.Idle);

        idleWalkInterval = UnityEngine.Random.Range(minIdleWalkInterval, maxIdleWalkInterval);
        idleWalkAngleOffset = UnityEngine.Random.Range(-planet.cellIntervalAngle / 2, planet.cellIntervalAngle / 2);

        task = null;

        ChangeCreatureState(CreatureState.Air);
    }
    public override void Update()
    {
        base.Update();
        //Debug.Log(currentCell.radiusIdx + " " + currentCell.angleIdx + " " + lastCurrentCell.radiusIdx + " " + lastCurrentCell.angleIdx);

        Vector3 dir = transform.position - planet.transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        float angleRad = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
        transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);

        if (currentCell != null && currentCell.neighbourCellNodes[1].cell.canStand == false && creatureState != CreatureState.Air) ChangeCreatureState(CreatureState.Air);
        /*
        Cell belowCell = currentCell.neighbourCellNodes[1].cell;
        if ((belowCell != null && !belowCell.canStand) || currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) < 0) ChangeCreatureState(CreatureState.Air);
        */

        switch (creatureState)
        {

            case CreatureState.Air:
                transform.position += velocity * Time.deltaTime;
                //if (currentCell == null) break;
                Cell belowCell = null;
                belowCell = currentCell.neighbourCellNodes[1].cell;
                if (belowCell != null && belowCell.canStand && (currentCell.radiusIdx - 1f / 2f) * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) >= -creatureHeight) CancelTask();
                /*
                if (belowCell != null && belowCell.canStand && currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) >= 0)
                {
                    FindTask();
                }
                */
                break;

            case CreatureState.Idle:
                /*
                if (isIdleWalking)
                {
                    float step = idleWalkSpeed / (polarCoord.r * planet.cellHeight) * Time.deltaTime * Mathf.Rad2Deg;
                    float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                    float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                    Debug.Log(targetAngle + " " + currentAngle);
                    Debug.Log(angleDiff);
                    if (Mathf.Abs(angleDiff) <= step)
                    {
                        isIdleWalking = false;
                        transform.RotateAround(planet.transform.position, Vector3.forward, angleDiff);

                        animator.Play("Idle", 0, 0f);
                        animator.Update(0f);
                    }
                    else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                }
                else
                {
                    idleWalkTimer += Time.deltaTime;
                    if (idleWalkTimer > idleWalkInterval)
                    {
                        isIdleWalking = true;
                        idleWalkTimer = 0;
                        idleWalkInterval = Random.Range(minIdleWalkInterval, maxIdleWalkInterval);
                        idleWalkAngleOffset = Random.Range(-planet.cellIntervalAngle / 2, planet.cellIntervalAngle / 2);

                        animator.Play("Walk", 0, 0f);
                        animator.Update(0f);
                    }
                }
                */
                break;
            case CreatureState.Walk:
                if (path.Count > 0)
                {
                    //Debug.Log(currentCell.radiusIdx * planet.cellHeight + " " + Vector2.Distance(transform.position, planet.transform.position));
                    if (path[0].radiusIdx == lastCurrentCell.radiusIdx && Math.Abs(currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position)) <= 0.01f)
                    {
                        //Debug.Log(currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) + " " + 1 + " " + Time.time);
                        float step = walkSpeed / (polarCoord.r * planet.cellHeight) * Time.deltaTime * Mathf.Rad2Deg;
                        //float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                        float targetAngle = path[0].angleIdx * planet.cellIntervalAngle;
                        float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                        if (Mathf.Abs(angleDiff) <= step)
                        {
                            transform.RotateAround(planet.transform.position, Vector3.forward, angleDiff);
                            lastCurrentCell = path[0];
                            path.RemoveAt(0);
                        }
                        else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                    }
                    else
                    {
                        float step = climbSpeed * Time.deltaTime;
                        float distanceDiff = Vector2.Distance(path[0].transform.position, planet.transform.position) - Vector2.Distance(transform.position, planet.transform.position);
                        //Debug.Log(distanceDiff + " " + step);
                        if (Mathf.Abs(distanceDiff) <= step)
                        {
                            transform.position += transform.up * distanceDiff;
                            lastCurrentCell = path[0];
                            path.RemoveAt(0);
                        }
                        else transform.position += transform.up * step * Mathf.Sign(distanceDiff);
                    }
                }

                if (path.Count == 0)
                {
                    if (task == null)
                        ChangeCreatureState(CreatureState.Idle);
                    else
                        switch (task.taskType)
                        {
                            case TaskType.CutTree:
                                ChangeCreatureState(CreatureState.CutTree);
                                break;
                            case TaskType.MineStone:
                                ChangeCreatureState(CreatureState.MineStone);
                                break;
                            case TaskType.Build:
                                ChangeCreatureState(CreatureState.Build);
                                /*
                                PlacedObject placedObject = task.baseUnits[0].GetComponent<PlacedObject>();
                                if (reservedItem.isPickedUp)
                                {
                                    placedObject.AddItem(reservedItem);
                                    reservedItem.DestoryBaseUnit();
                                    List<Cell> buildTempPath = PathToClosetItem(placedObject.requiredItemTypes, out reservedItem, out reservedWarehouse);
                                    if (reservedItem == null) placedObject.CancelBuildTask();
                                    else
                                    {
                                        path = buildTempPath;
                                        ChangeCreatureState(CreatureState.Walk);
                                        reservedItem.reserver = this;
                                    }
                                }
                                else
                                {
                                    PickUpItem(reservedItem);
                                    SetTargetCell(placedObject.currentCell);
                                    if (reservedWarehouse != null)
                                    {
                                        reservedWarehouse.RemoveItem(reservedItem.itemType);
                                        reservedWarehouse = null;
                                    }
                                }
                                */
                                break;
                            case TaskType.MoveItem:
                                PlacedObject moveItemPlacedObject = task.baseUnits[0] as PlacedObject;
                                if (moveItemPlacedObject != null)
                                {
                                    if (reservedItem.isPickedUp)
                                    {
                                        moveItemPlacedObject.AddItem(reservedItem);
                                        UnbindReservedItem();
                                        if (moveItemPlacedObject.requiredItemTypes.Count != 0)
                                        {
                                            List<Cell> moveItemTempPath = PathToClosetItem(moveItemPlacedObject.requiredItemTypes, out reservedItem, out reservedWarehouse);
                                            if (reservedItem == null && reservedWarehouse == null)
                                            {
                                                if (moveItemPlacedObject != null) moveItemPlacedObject.CancelMoveItemTask();
                                            }
                                            else
                                            {
                                                if (reservedWarehouse != null) reservedItem = reservedWarehouse.ReserveItem(reservedWarehouse.AvailableItemTypes(moveItemPlacedObject.requiredItemTypes)[0]);

                                                path = moveItemTempPath;
                                                ChangeCreatureState(CreatureState.Walk);
                                                reservedItem.reserver = this;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        PickUpItem(reservedItem);
                                        SetTargetCell(moveItemPlacedObject.currentCell);
                                        if (reservedWarehouse != null)
                                        {
                                            reservedWarehouse.RemoveItem(reservedItem.itemType);
                                            reservedWarehouse = null;
                                        }
                                    }
                                }
                                else
                                {
                                    Warehouse warehouse = task.baseUnits[0].GetComponent<Warehouse>();
                                    if (reservedItem.isPickedUp)
                                    {
                                        warehouse.AddItem(reservedItem.itemType);
                                        reservedItem.DestoryBaseUnit();
                                        if (warehouse.IsFull())
                                        {
                                            ChangeCreatureState(CreatureState.Idle);
                                            TaskManager.instance.RemoveTask(warehouse.moveItemTask);
                                            break;
                                        }
                                        List<Cell> moveItemTempPath = PathToClosetItem(out reservedItem);
                                        if (reservedItem == null) warehouse.CancelMoveItemTask();
                                        else
                                        {
                                            path = moveItemTempPath;
                                            ChangeCreatureState(CreatureState.Walk);
                                            reservedItem.reserver = this;
                                        }
                                    }
                                    else
                                    {
                                        PickUpItem(reservedItem);
                                        SetTargetCell(warehouse.building.currentCell);
                                    }
                                }
                                break;

                        }

                }
                break;
            case CreatureState.CutTree:
                /*
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                float current = stateInfo.normalizedTime % 1f;
                Debug.Log(current + " " + lastNormalizedTime);
                if (lastNormalizedTime > current && lastNormalizedTime - current < 0.9f)
                {
                    wood.CutTree(processPerCutTree);
                }

                lastNormalizedTime = current;
                */
                break;

        }
    }
    public override void LateUpdate()
    {
        base.LateUpdate();

        isSettingTask = false;
    }
    public void ChangeCreatureState(CreatureState cs)
    {
        creatureState = cs;
        switch (cs)
        {
            case CreatureState.Idle:
                animator.Play("Idle", 0, 0f);
                //animator.Update(0f);
                break;
            case CreatureState.Walk:
                animator.Play("Walk", 0, 0f);
                //animator.Update(0f);
                break;
            case CreatureState.CutTree:
                animator.Play("PunchTree", 0, 0f);
                //animator.Update(0f);
                break;
            case CreatureState.MineStone:
                animator.Play("PunchStone", 0, 0f);
                break;
            case CreatureState.Air:
                velocity = planet.transform.position - transform.position;
                velocity = velocity.normalized;
                animator.Play("Fall", 0, 0f);
                break;
            case CreatureState.Build:
                animator.Play("Build", 0, 0f);
                break;
        }
    }
    public void CutTree()
    {
        wood.CutTree(processPerCutTree);
    }
    public void MineStone()
    {
        stone.MineStone(processPreMineStone);
    }
    public void BuildPlacedObject()
    {
        ((PlacedObject)task.baseUnits[0]).BuildPlacedObject(processPerBuildPlacedObject);
    }
    public void PickUpItem(Item it)
    {
        it.transform.SetParent(transform);
        it.transform.position = itemPos.position;
        it.gameObject.SetActive(true);
        it.PickUp();

    }
    public void SetTargetCell(Cell tc)
    {
        List<Cell> temp = planet.FindPath(planet.PosToCell(transform.position), tc);
        if (temp != null)
        {
            path = temp;
            ChangeCreatureState(CreatureState.Walk);
        }
    }
    public void SetTask(Task t, float c)
    {
        if (t == null)
        {
            //ChangeCreatureState(CreatureState.Idle);
            return;
        }
        //Debug.Log(1 + " " + transform.position);
        isSettingTask = true;
        task = t;
        if (!TaskManager.instance.sharedTaskTypes.Contains(t.taskType)) TaskManager.instance.taskToCreatureTaskNodes[t].Clear();
        TaskManager.instance.taskToCreatureTaskNodes[t].Add(new CreatureTaskNode(this, c));
        switch (task.taskType)
        {
            case TaskType.CutTree:
                SetTargetCell(task.baseUnits[0].currentCell);
                wood = task.baseUnits[0].GetComponent<Wood>();
                break;
            case TaskType.MineStone:
                SetTargetCell(task.baseUnits[0].currentCell.neighbourCellNodes[0].cell);
                stone = task.baseUnits[0].GetComponent<Stone>();
                break;
            case TaskType.MoveItem:
                PlacedObject moveItemPlacedObject = task.baseUnits[0] as PlacedObject;
                if (moveItemPlacedObject != null)
                {
                    List<Cell> buildTempPath = PathToClosetItem(moveItemPlacedObject.requiredItemTypes, out reservedItem, out reservedWarehouse);
                    if (reservedItem == null && reservedWarehouse == null) moveItemPlacedObject.CancelMoveItemTask();
                    else
                    {
                        path = buildTempPath;
                        ChangeCreatureState(CreatureState.Walk);
                        if (reservedWarehouse != null)
                        {
                            reservedItem = reservedWarehouse.ReserveItem(moveItemPlacedObject.requiredItemTypes.GetIntersection(reservedWarehouse.itemTypeToNumber.Keys)[0]);
                            reservedItem.reserver = this;
                        }
                        else reservedItem.reserver = this;
                    }
                }
                else
                {
                    Warehouse warehouse = task.baseUnits[0].GetComponent<Warehouse>();
                    List<Cell> moveItemTempPath = PathToClosetItem(out reservedItem);
                    if (reservedItem == null) warehouse.CancelMoveItemTask();
                    else
                    {
                        path = moveItemTempPath;
                        ChangeCreatureState(CreatureState.Walk);
                        reservedItem.reserver = this;
                    }
                }
                break;
            case TaskType.Build:
                SetTargetCell(task.baseUnits[0].currentCell);
                break;
        }
    }
    public void FindTask()
    {
        if (task != null && !isSettingTask) return;
        isSettingTask = false;
        //float tempDis = float.MaxValue;
        Task tempTask = null;
        float tempCost = float.MaxValue;
        List<Task> priorityTasks = new List<Task>();
        List<Task> normalTasks = new List<Task>();
        List<Task> tasks = new List<Task>();
        Creature tempCreature = null;
        foreach (var t in TaskManager.instance.tasks)
        {
            if (TaskManager.instance.taskToCreatureTaskNodes.ContainsKey(t) && TaskManager.instance.taskToCreatureTaskNodes[t].Count > 0 && !TaskManager.instance.taskToCreatureTaskNodes[t][0].creature.isSettingTask) continue;
            tasks.Add(t);
            if (priorityTaskTypes.Contains(t.taskType)) priorityTasks.Add(t);
            else normalTasks.Add(t);
        }
        foreach (var t in tasks)
        {
            List<Cell> tempPath = null;
            switch (t.taskType)
            {
                case TaskType.MineStone:
                    tempPath = planet.FindPath(currentCell, t.baseUnits[0].currentCell.neighbourCellNodes[0].cell);
                    break;
                default:
                    tempPath = planet.FindPath(currentCell, t.baseUnits[0].currentCell);
                    break;
            }
            float cost = planet.GetPathLength(tempPath);
            //Debug.Log(cost + " " + Time.time);
            if (cost >= tempCost) continue;

            if (TaskManager.instance.taskToCreatureTaskNodes[t].Count > 0)
            {
                //Debug.Log(cost + " " + TaskManager.instance.taskToCreatureTaskNodes[t][0].cost + " " + currentCell.angleIdx);
                if (cost >= TaskManager.instance.taskToCreatureTaskNodes[t][0].cost) continue;
                tempCreature = TaskManager.instance.taskToCreatureTaskNodes[t][0].creature;
                //continue;
            }

            tempCost = cost;

            if (tempTask == null) tempTask = t;
            else if (priorityTaskTypes.Contains(t.taskType) && !priorityTaskTypes.Contains(tempTask.taskType)) tempTask = t;
            else
            {
                tempTask = t;
            }
        }
        if (tempCreature != null && tempTask != null && TaskManager.instance.taskToCreatureTaskNodes[tempTask].Count > 0 && tempCreature != TaskManager.instance.taskToCreatureTaskNodes[tempTask][0].creature) tempCreature = null;
        if (tempCreature != null) tempCreature.UnbindReservedItem();
        if (tempCost != float.MaxValue) SetTask(tempTask, tempCost);
        if (tempCreature != null) tempCreature.CancelTask();

    }
    public void CancelTask()
    {
        isSettingTask = false;
        UnbindReservedItem();
        /*
        if (task != null)
        {
            for (int i = 0; i < TaskManager.instance.taskToCreatureTaskNodes[task].Count; i++)
            {
                CreatureTaskNode taskNode = TaskManager.instance.taskToCreatureTaskNodes[task][i];
                if (taskNode.creature == this) TaskManager.instance.taskToCreatureTaskNodes[task].RemoveAt(i);
            }
        }
        */
        task = null;
        ChangeCreatureState(CreatureState.Idle);
        //SetTargetCell(currentCell);
        FindTask();
    }
    public void UnbindReservedItem()
    {
        //Debug.Log(Time.time);
        if (reservedWarehouse != null)
        {
            if (!reservedWarehouse.itemTypeToNumber.ContainsKey(reservedItem.itemType)) reservedWarehouse.itemTypeToNumber[reservedItem.itemType] = 0;
            reservedWarehouse.itemTypeToNumber[reservedItem.itemType]++;
            reservedItem.DestoryBaseUnit();
        }
        else if (reservedItem != null)
        {
            reservedItem.ResetItem();
            //reservedItem.reserver = null;
            reservedItem = null;
        }
    }
    public override void DestoryBaseUnit()
    {
        QtreeManager.instance.baseUnits.Remove(this);

        base.DestoryBaseUnit();

    }
    public List<Cell> PathToClosetItem(List<ItemType> itemTypes, out Item itm, out Warehouse wh)
    {
        List<Cell> result = null;
        float minCost = float.MaxValue;
        itm = null;
        wh = null;
        foreach (Item item in planet.items)
        {
            if (itemTypes.Contains(item.itemType) && item.reserver == null && !item.isInAir)
            {
                List<Cell> tempPath = planet.FindPath(currentCell, item.currentCell);
                //Debug.Log(item.itemType);
                //List<Cell> tempPath = planet.FindPathWithMaxDistance(currentCell, it.currentCell, minCost);
                if (tempPath != null)
                {
                    float cost = planet.GetPathLength(tempPath);
                    if (cost < minCost)
                    {
                        result = tempPath;
                        itm = item;
                        minCost = cost;
                    }
                }
            }
        }

        foreach (Warehouse warehouse in planet.warehouses)
        {
            if (warehouse.IsItemAvailable(itemTypes))
            {
                List<Cell> tempPath = planet.FindPath(currentCell, warehouse.building.currentCell);
                //List<Cell> tempPath = planet.FindPathWithMaxDistance(currentCell, it.currentCell, minCost);
                if (tempPath != null)
                {
                    float cost = planet.GetPathLength(tempPath);
                    if (cost < minCost)
                    {
                        result = tempPath;
                        wh = warehouse;
                        minCost = cost;
                    }
                }
            }
        }

        return result;
    }
    public List<Cell> PathToClosetItem(out Item itm)
    {
        List<Cell> result = null;
        float minCost = float.MaxValue;
        itm = null;
        foreach (Item it in planet.items)
        {
            if (it.reserver == null)
            {
                List<Cell> tempPath = planet.FindPath(currentCell, it.currentCell);
                //List<Cell> tempPath = planet.FindPathWithMaxDistance(currentCell, it.currentCell, minCost);
                if (tempPath != null)
                {
                    float cost = planet.GetPathLength(tempPath);
                    if (cost < minCost)
                    {
                        result = tempPath;
                        itm = it;
                        minCost = cost;
                    }
                }
            }
        }
        return result;
    }
    public void ChangeProfession(ProfessionType pt)
    {
        foreach (var gameObject in professionTypeToGameobjects[professionType].gameObjects) gameObject.SetActive(false);
        professionType = pt;
        foreach (var gameObject in professionTypeToGameobjects[professionType].gameObjects) gameObject.SetActive(true);

    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0, creatureHeight));
    }
}
