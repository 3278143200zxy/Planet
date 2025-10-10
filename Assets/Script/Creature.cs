using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Transactions;
using UnityEngine;


public enum CreatureState
{
    Air,
    Idle,
    Walk,
    CutTree,
}
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

    public Task task;
    public bool isSettingTask = false;

    public float idleWalkSpeed;
    public float minIdleWalkInterval, maxIdleWalkInterval;
    private float idleWalkTimer, idleWalkInterval;
    public float idleWalkAngleOffset;
    public bool isIdleWalking;

    public List<Cell> path = new List<Cell>();
    public float walkSpeed;
    public float climbSpeed;

    public List<TaskType> priorityTaskTypes = new List<TaskType>();

    public float processPerCutTree;
    public Wood wood;

    public PlacedObject buildingPlacedObject;
    public Item reseverdItem;
    public Transform itemPos;
    public override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        task = null;
    }
    public override void Start()
    {
        base.Start();

        planet = MouseManager.instance.planets[0];
        lastCurrentCell = currentCell;
        //ChangeCreatureState(CreatureState.Idle);

        idleWalkInterval = UnityEngine.Random.Range(minIdleWalkInterval, maxIdleWalkInterval);
        idleWalkAngleOffset = UnityEngine.Random.Range(-planet.cellIntervalAngle / 2, planet.cellIntervalAngle / 2);

        task = null;
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

        /*
        Cell belowCell = currentCell.neighbourCellNodes[1].cell;
        if ((belowCell != null && !belowCell.canStand) || currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) < 0) ChangeCreatureState(CreatureState.Air);
        */

        switch (creatureState)
        {
            /*
            case CreatureState.Air:
                transform.position += velocity * Time.deltaTime;
                if (belowCell != null && belowCell.canStand && currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) >= 0)
                {
                    FindTask();
                }

                break;
                */
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
                            case TaskType.MoveItem:
                                if (reseverdItem == null)
                                {
                                    Item it = task.baseUnits[0].GetComponent<Item>();
                                    PickUpItem(it);
                                    it.PickUp();
                                    SetTargetCell(planet.PosToCell(task.baseUnits[1].transform.position));
                                }
                                else
                                {

                                }
                                break;
                            case TaskType.Build:
                                if (reseverdItem.isPickedUp)
                                {
                                    buildingPlacedObject.AddItem(reseverdItem);
                                    List<Cell> tempPath = PathToClosetItem(buildingPlacedObject.requiredItemTypes, out reseverdItem);
                                    if (reseverdItem == null) buildingPlacedObject.CancelBuildingTask();
                                    else
                                    {
                                        path = tempPath;
                                        ChangeCreatureState(CreatureState.Walk);
                                        reseverdItem.reserver = this;
                                    }
                                }
                                else
                                {
                                    PickUpItem(reseverdItem);
                                    SetTargetCell(buildingPlacedObject.currentCell);
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
            case CreatureState.Air:
                velocity = planet.transform.position - transform.position;
                velocity = velocity.normalized;
                break;
        }
    }
    public void CutTree()
    {
        wood.CutTree(processPerCutTree);
    }
    public void PickUpItem(Item it)
    {
        it.transform.SetParent(transform);
        it.transform.position = itemPos.position;
        it.PickUp();

    }
    public void ReserveItem(Item it)
    {
        it.reserver = this;
        reseverdItem = it;
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
        task = t;
        TaskManager.instance.taskToCreatureTaskNodes[t].Add(new CreatureTaskNode(this, c));
        switch (task.taskType)
        {
            case TaskType.CutTree:
                SetTargetCell(planet.PosToCell(task.baseUnits[0].transform.position));
                wood = task.baseUnits[0].GetComponent<Wood>();
                break;
            case TaskType.MoveItem:
                SetTargetCell(planet.PosToCell(task.baseUnits[0].transform.position));
                break;
            case TaskType.Build:
                buildingPlacedObject = task.baseUnits[0].GetComponent<PlacedObject>();
                List<Cell> tempPath = PathToClosetItem(buildingPlacedObject.requiredItemTypes, out reseverdItem);
                if (reseverdItem == null) buildingPlacedObject.CancelBuildingTask();
                else
                {
                    path = tempPath;
                    ChangeCreatureState(CreatureState.Walk);
                    reseverdItem.reserver = this;
                }
                break;
        }
    }
    public void FindTask()
    {
        if (task != null) return;
        //float tempDis = float.MaxValue;
        Task tempTask = null;
        float tempCost = float.MaxValue;
        List<Task> priorityTasks = new List<Task>();
        List<Task> normalTasks = new List<Task>();
        List<Task> tasks = new List<Task>();
        foreach (var t in TaskManager.instance.tasks)
        {
            if (TaskManager.instance.taskToCreatureTaskNodes.ContainsKey(t) && TaskManager.instance.taskToCreatureTaskNodes[t].Count > 0 && !TaskManager.instance.taskToCreatureTaskNodes[t][0].creature.isSettingTask) continue;
            tasks.Add(t);
            if (priorityTaskTypes.Contains(t.taskType)) priorityTasks.Add(t);
            else normalTasks.Add(t);
        }
        foreach (var t in tasks)
        {
            float cost = planet.GetPathLength(planet.FindPath(currentCell, t.baseUnits[0].currentCell));
            if (cost >= tempCost) continue;

            if (TaskManager.instance.taskToCreatureTaskNodes.ContainsKey(t) && TaskManager.instance.taskToCreatureTaskNodes[t].Count > 0)
            {
                if (cost >= TaskManager.instance.taskToCreatureTaskNodes[t][0].cost) continue;
                TaskManager.instance.taskToCreatureTaskNodes[t][0].creature.CancelTask();
                //continue;
            }

            tempCost = cost;
            switch (t.taskType)
            {
                case TaskType.CutTree:
                case TaskType.Build:
                    if (planet.FindPath(currentCell, t.baseUnits[0].currentCell) == null) continue;
                    break;
            }
            if (tempTask == null) tempTask = t;
            else if (priorityTaskTypes.Contains(t.taskType) && !priorityTaskTypes.Contains(tempTask.taskType)) tempTask = t;
            else
            {
                tempTask = t;
            }
        }
        SetTask(tempTask, tempCost);
    }
    public void CancelTask()
    {
        isSettingTask = false;
        if (task != null)
        {
            for (int i = 0; i < TaskManager.instance.taskToCreatureTaskNodes[task].Count; i++)
            {
                CreatureTaskNode taskNode = TaskManager.instance.taskToCreatureTaskNodes[task][i];
                if (taskNode.creature == this) TaskManager.instance.taskToCreatureTaskNodes[task].RemoveAt(i);
            }
        }
        task = null;
        SetTargetCell(currentCell);
        FindTask();
    }
    public override void DestoryBaseUnit()
    {
        base.DestoryBaseUnit();
    }
    public List<Cell> PathToClosetItem(List<ItemType> itemTypes, out Item itm)
    {
        List<Cell> result = null;
        float minCost = float.MaxValue;
        itm = null;
        foreach (Item it in planet.items)
        {
            if (itemTypes.Contains(it.itemType) && it.reserver == null)
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
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
}
