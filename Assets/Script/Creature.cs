using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public enum CreatureState
{
    Air,
    Idle,
    Walk,
    CutTree,
    MineStone,
    Build,
    Craft,
    Sleep,
    Research,
}
public enum ProfessionType
{
    None,
    Lumberjack,
    Miner,
    Mover,
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
    public Animator animator;
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

    public float minIdleWalkInterval, maxIdleWalkInterval;
    private float idleWalkTimer, idleWalkInterval;
    public float idleWalkAngleOffset;

    public List<Cell> path = new List<Cell>();
    public float walkSpeed;
    public float moverSpeed;
    public float climbSpeed;
    //horizontal walk offset
    public float walkAngleOffset = 0f;

    public List<TaskType> priorityTaskTypes = new List<TaskType>();

    public float progressPerCutTree;
    public float lumberjackProgress;
    [HideInInspector] public Wood wood;
    public float progressPreMineStone;
    public float minerProgress;
    [HideInInspector] public Stone stone;
    private Vector2 punchStoneDirection;
    public float processPerCraft;
    [HideInInspector] public BillModule billModule;
    public float processPerBuildPlacedObject;
    [HideInInspector] public ResearchBench researchBench;
    public float processPerResearch;

    public Item reservedItem;
    public WarehouseModule reservedWarehouseModule;
    public Transform itemPos;

    float lastVerticalVelocity;
    public bool isChangeLayerAtVertex = false;

    public Transform groundEffectPos;
    public GameObject groundEffectPrefab;

    [Header("Profession")]
    public ProfessionType professionType = ProfessionType.None;
    public ProfessionShowItemDictionary professionTypeToGameobjects = new ProfessionShowItemDictionary();

    [Header("Sleep")]
    [Range(0, 1)] public float energy = 1;
    public float idleEnergyDecrease;
    public float walkEnergyDecrease;
    public float workEnergyDecrease;
    public float sleepEnergyIncrease;

    public float tiredThreshold;
    public float restedThreshold;
    public float wakeUpOffset;
    public RestPoint restPoint;

    public GameObject sleepTip;
    public override void Awake()
    {
        base.Awake();

        task = new Task(TaskType.None, new BaseUnit[0]);

        lastVerticalVelocity = -float.MaxValue;
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

        ChangeCreatureState(CreatureState.Air);
        //CancelTaskWithoutFindTask();
        TimeManager.instance.ChangeTimeScaleEvent.AddListener(OnTimeScaleChange);

        //AddForce(new Vector2(10, 10f));
    }
    public override void Update()
    {
        base.Update();
        //Debug.Log(planet.CellRadiusFromDistance(Vector2.Distance(transform.position, planet.transform.position)) + " " + currentCell.radiusIdx);
        //Debug.Log(currentCell.radiusIdx + " " + currentCell.angleIdx + " " + lastCurrentCell.radiusIdx + " " + lastCurrentCell.angleIdx);

        Vector2 dir = transform.position - planet.transform.position;
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (currentCell != null && currentCell.neighbourCellNodes[1].cell.canStand == false && creatureState != CreatureState.Air)
        {
            CancelTaskWithoutFindTask();
            ChangeCreatureState(CreatureState.Air);
        }
        /*
        Cell belowCell = currentCell.neighbourCellNodes[1].cell;
        if ((belowCell != null && !belowCell.canStand) || currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) < 0) ChangeCreatureState(CreatureState.Air);
        */

        switch (creatureState)
        {
            case CreatureState.Air:
                velocity -= (Vector3)dir * planet.gravity * TimeManager.deltaTime * TimeManager.timeScale;
                velocity.z = 0;

                if (isChangeLayerAtVertex)
                {
                    float verticalVelocity = Vector2.Dot(velocity, dir);
                    if (verticalVelocity < 0 && lastVerticalVelocity > 0)
                    {
                        isChangeLayerAtVertex = false;
                        lastVerticalVelocity = -float.MaxValue;
                        ChangeLayer(gameObject, LayerMask.NameToLayer((1 - currentCell.layerIdx).ToString()));
                        transform.position = new Vector3(transform.position.x, transform.position.y, 1 - currentCell.layerIdx);

                    }
                    else lastVerticalVelocity = verticalVelocity;
                }

                Cell belowCell = null;
                belowCell = currentCell.neighbourCellNodes[1].cell;
                float distanceToGround = float.MaxValue;
                if (belowCell != null && belowCell.canStand)
                    distanceToGround = Vector2.Distance(transform.position, planet.transform.position) - planet.CellRadiusDistance(belowCell.radiusIdx) - planet.CellHeight(belowCell.radiusIdx) / 2f - creatureHeight;
                if (distanceToGround <= (velocity * TimeManager.deltaTime).magnitude && distanceToGround > -creatureHeight / 3 && Vector2.Dot(velocity, dir) < 0)
                {

                    transform.position += velocity.normalized * distanceToGround;
                    velocity = Vector3.zero;
                    if (path.Count == 0 || currentCell != path[0])
                    {
                        ChangeCreatureState(CreatureState.Idle);
                        CancelTask();
                        //FindTask();
                    }
                    else ChangeCreatureState(CreatureState.Walk);
                    lastCurrentCell = currentCell;

                    GameObject groundEffect = Instantiate(groundEffectPrefab, groundEffectPos.position, transform.rotation);
                    groundEffect.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer(currentCell.layerIdx.ToString());
                }

                transform.position += velocity * TimeManager.deltaTime;
                //if (currentCell == null) break;

                /*
                if (belowCell != null && belowCell.canStand && currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) >= 0)
                {
                    FindTask();
                }
                */
                break;

            case CreatureState.Idle:
                /*
                idleWalkTimer += TimeManager.deltaTime;
                if (idleWalkInterval > idleWalkTimer)
                {
                    walkAngleOffset = idleWalkAngleOffset;

                    idleWalkAngleOffset = UnityEngine.Random.Range(-planet.cellIntervalAngle / 2, planet.cellIntervalAngle / 2);
                    idleWalkInterval = UnityEngine.Random.Range(minIdleWalkInterval, maxIdleWalkInterval);
                    idleWalkTimer = 0;
                }
                */
                break;
            case CreatureState.Walk:
                //if creature is not at the last cell pr work horizontally, do not consider the angle offset
                foreach (var cell in path) if (!cell.neighbourCellNodes[1].cell.canStand)
                    {
                        ChangeCreatureState(CreatureState.Idle);
                        CancelTask();
                        break;
                    }
                if (path.Count > 0 || path[0].radiusIdx != lastCurrentCell.radiusIdx)
                {
                    //Debug.Log(currentCell.radiusIdx * planet.cellHeight + " " + Vector2.Distance(transform.position, planet.transform.position));
                    //x walk
                    //Debug.Log(path[0].angleIdx + " " + path[0].radiusIdx + " " + path[0].layerIdx);
                    //Debug.Log(lastCurrentCell.angleIdx + " " + lastCurrentCell.radiusIdx + " " + lastCurrentCell.layerIdx);
                    if (Mathf.Abs(path[0].angleIdx - lastCurrentCell.angleIdx) == 1 && path[0].radiusIdx == lastCurrentCell.radiusIdx && path[0].layerIdx == lastCurrentCell.layerIdx)
                    //&& Math.Abs(planet.CellRadiusDistance(currentCell.radiusIdx) - Vector2.Distance(transform.position, planet.transform.position)) <= 0.01f)
                    {
                        //Debug.Log("x");
                        //Debug.Log(currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) + " " + 1 + " " + Time.time);
                        float step = walkSpeed / planet.CellRadiusDistance(currentCell.radiusIdx) * TimeManager.deltaTime * Mathf.Rad2Deg;
                        //float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                        float targetAngle = path[0].angleIdx * planet.cellIntervalAngle;
                        if (path.Count == 1) targetAngle += walkAngleOffset;
                        float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                        if (Mathf.Abs(angleDiff) <= step)
                        {
                            if (path.Count > 1 && Mathf.Abs(path[0].angleIdx - path[1].angleIdx) == 1 && path[0].radiusIdx == path[1].radiusIdx && path[0].layerIdx == path[1].layerIdx)
                                transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                            else transform.RotateAround(planet.transform.position, Vector3.forward, angleDiff);
                            lastCurrentCell = path[0];
                            path.RemoveAt(0);
                        }
                        else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                    }
                    //y walk
                    else if (Mathf.Abs(path[0].radiusIdx - lastCurrentCell.radiusIdx) == 1 && path[0].angleIdx == lastCurrentCell.angleIdx && path[0].layerIdx == lastCurrentCell.layerIdx)
                    {
                        //Debug.Log("y");
                        float step = climbSpeed * TimeManager.deltaTime;
                        float distanceDiff = Vector2.Distance(path[0].position, planet.transform.position) - Vector2.Distance(transform.position, planet.transform.position);
                        //Debug.Log(distanceDiff + " " + step);
                        if (Mathf.Abs(distanceDiff) <= step)
                        {
                            if (path.Count > 1 && Mathf.Abs(path[0].radiusIdx - path[1].radiusIdx) == 1 && path[0].angleIdx == path[1].angleIdx && path[0].layerIdx == path[1].layerIdx)
                                transform.position += transform.up * step * Mathf.Sign(distanceDiff);
                            else transform.position += transform.up * distanceDiff;
                            lastCurrentCell = path[0];
                            path.RemoveAt(0);
                        }
                        else transform.position += transform.up * step * Mathf.Sign(distanceDiff);
                    }
                    //z walk
                    else if (Mathf.Abs(path[0].layerIdx - lastCurrentCell.layerIdx) == 1 && path[0].radiusIdx == lastCurrentCell.radiusIdx && path[0].angleIdx == lastCurrentCell.angleIdx)
                    {
                        //Debug.Log("z");
                        float step = walkSpeed * TimeManager.deltaTime;
                        float distanceDiff = path[0].position.z - transform.position.z;
                        //Debug.Log(distanceDiff + " " + step);
                        if (Mathf.Abs(distanceDiff) <= step)
                        {
                            if (path.Count > 1 && Mathf.Abs(path[0].layerIdx - lastCurrentCell.layerIdx) == 1 && path[0].radiusIdx == path[1].radiusIdx && path[0].angleIdx == path[1].angleIdx)
                                transform.position += transform.forward * step * Mathf.Sign(distanceDiff);
                            else transform.position += transform.forward * distanceDiff;
                            lastCurrentCell = path[0];
                            path.RemoveAt(0);

                            ChangeLayer(gameObject, LayerMask.NameToLayer(lastCurrentCell.layerIdx.ToString()));
                        }
                        else transform.position += transform.forward * step * Mathf.Sign(distanceDiff);
                    }
                    else if (path[0].layerIdx == lastCurrentCell.layerIdx && Mathf.Abs(path[0].radiusIdx - lastCurrentCell.radiusIdx) == 1 && Mathf.Abs(path[0].angleIdx - lastCurrentCell.angleIdx) == 1)
                    {
                        //Debug.Log("d");
                        Vector3 force = Vector3.zero;
                        //force = MathEx.CalculateInitialVelocity(lastCurrentCell.transform.position, path[0].transform.position, planet.gravity, planet.transform.position);
                        force = MathEx.CalculateInitialVelocity(transform.position, path[0].position + (Vector3)MathEx.RotateVector2(new Vector3(0, -planet.cellHeight / 2 + creatureHeight), Mathf.Deg2Rad * angle), planet.gravity * TimeManager.timeScale, planet.transform.position);
                        AddForce(force * 0.95f);

                    }
                    else if (Mathf.Abs(path[0].layerIdx - lastCurrentCell.layerIdx) == 1 && Mathf.Abs(path[0].radiusIdx - lastCurrentCell.radiusIdx) == 1 && path[0].angleIdx == lastCurrentCell.angleIdx)
                    {
                        Vector3 force = Vector3.zero;
                        force = MathEx.CalculateInitialVelocity(transform.position, path[0].position + (Vector3)MathEx.RotateVector2(new Vector3(0, -planet.cellHeight / 2 + creatureHeight), Mathf.Deg2Rad * angle), planet.gravity * TimeManager.timeScale, planet.transform.position);
                        force.z = 0f;
                        AddForce(force * 0.95f);
                        isChangeLayerAtVertex = true;
                    }
                    else if (path[0].layerIdx == lastCurrentCell.layerIdx && path[0].radiusIdx == lastCurrentCell.radiusIdx && path[0].angleIdx == lastCurrentCell.angleIdx)
                    {
                        //Debug.Log("x");
                        //Debug.Log(currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) + " " + 1 + " " + Time.time);
                        float step = walkSpeed / planet.CellRadiusDistance(currentCell.radiusIdx) * TimeManager.deltaTime * Mathf.Rad2Deg;
                        //float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                        float targetAngle = path[0].angleIdx * planet.cellIntervalAngle;
                        if (path.Count == 1) targetAngle += walkAngleOffset;
                        float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                        if (Mathf.Abs(angleDiff) <= step)
                        {
                            lastCurrentCell = path[0];
                            path.RemoveAt(0);
                        }
                        else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));

                    }
                    else
                    {
                        Debug.Log("null");
                        CancelTask();
                    }
                }
                /*
                if (path.Count == 1 && path[0].radiusIdx == lastCurrentCell.radiusIdx)
                {
                    float step = walkSpeed / (polarCoord.r * planet.cellHeight) * TimeManager.deltaTime * Mathf.Rad2Deg;
                    //float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                    float targetAngle = path[0].angleIdx * planet.cellIntervalAngle + walkAngleOffset;
                    float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                    if (Mathf.Abs(angleDiff) <= step)
                    {
                        transform.RotateAround(planet.transform.position, Vector3.forward, angleDiff);
                        lastCurrentCell = path[0];
                        path.RemoveAt(0);
                    }
                    else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                }
                */
                if (path.Count == 0)
                {

                    switch (task.taskType)
                    {
                        case TaskType.None:
                            ChangeCreatureState(CreatureState.Idle);
                            if (restPoint != null) StartSleep();
                            break;
                        case TaskType.CutTree:
                            ChangeCreatureState(CreatureState.CutTree);
                            break;
                        case TaskType.MineStone:
                            ChangeCreatureState(CreatureState.MineStone);
                            break;
                        case TaskType.Build:
                            ChangeCreatureState(CreatureState.Build);
                            break;
                        case TaskType.Craft:
                            ChangeCreatureState(CreatureState.Craft);
                            break;
                        case TaskType.Research:
                            ChangeCreatureState(CreatureState.Research);
                            break;
                        case TaskType.MoveItem:
                            if (task.baseUnits[0] is PlacedObject)
                            {
                                PlacedObject moveItemPlacedObject = task.baseUnits[0] as PlacedObject;
                                if (reservedItem.isPickedUp)
                                {
                                    //Debug.Log(2);
                                    moveItemPlacedObject.AddItem(reservedItem);
                                    //UnbindReservedItem();
                                    if (moveItemPlacedObject.requiredItemTypes.Count != 0)
                                    {
                                        List<Cell> moveItemTempPath = PathToClosetItem(moveItemPlacedObject.requiredItemTypes, planet.warehouseModules, out reservedItem, out reservedWarehouseModule);
                                        if (reservedItem == null && reservedWarehouseModule == null)
                                        {
                                            if (moveItemPlacedObject != null) moveItemPlacedObject.CancelMoveItemTask();
                                        }
                                        else
                                        {
                                            //if (reservedWarehouseModule != null) reservedItem = reservedWarehouseModule.ReserveItem(reservedWarehouseModule.AvailableItemTypes(moveItemPlacedObject.requiredItemTypes)[0]);

                                            path = moveItemTempPath;
                                            ChangeCreatureState(CreatureState.Walk);
                                            reservedItem.reserver = this;
                                        }
                                    }
                                    else
                                    {
                                        CancelTaskWithoutFindTask();
                                        ChangeCreatureState(CreatureState.Idle);
                                    }
                                }
                                else
                                {
                                    PickUpItem(reservedItem);
                                    SetTargetCell(moveItemPlacedObject.currentCell);
                                    if (reservedWarehouseModule != null)
                                    {
                                        reservedWarehouseModule.RemoveItem(reservedItem.itemType);
                                        reservedWarehouseModule = null;
                                    }
                                }
                            }
                            else if (task.baseUnits[0] is Item)
                            {
                                Item weapon = task.baseUnits[0] as Item;
                                ArmWeapon(weapon.itemType);
                                CancelTaskWithoutFindTask();
                                weapon.DestoryBaseUnit();
                            }
                            else
                            {
                                WarehouseModule warehouseModule = task.baseUnits[0].GetComponent<WarehouseModule>();
                                if (reservedItem.isPickedUp)
                                {
                                    ItemType tempItemType = reservedItem.itemType;
                                    reservedItem.DestoryBaseUnit();
                                    warehouseModule.AddItem(tempItemType);
                                    if (warehouseModule.isChangeTaskFrame)
                                    {
                                        CancelTaskWithoutFindTask();
                                        ChangeCreatureState(CreatureState.Idle);
                                    }
                                    else
                                    {
                                        if (warehouseModule.IsFull())
                                        {
                                            ChangeCreatureState(CreatureState.Idle);
                                            TaskManager.instance.RemoveTask(warehouseModule.moveItemTask);
                                            break;
                                        }

                                        List<Cell> moveItemTempPath = null;
                                        if (warehouseModule.GetComponent<BillModule>() == null) moveItemTempPath = PathToClosetItem(warehouseModule.neededItemTypes, out reservedItem);
                                        else
                                        {
                                            List<WarehouseModule> tempWarehouseModules = new List<WarehouseModule>(planet.warehouseModules);
                                            tempWarehouseModules.Remove(warehouseModule);

                                            moveItemTempPath = PathToClosetItem(warehouseModule.neededItemTypes, tempWarehouseModules, out reservedItem, out reservedWarehouseModule);
                                        }
                                        //if (reservedWarehouseModule != null) reservedItem = reservedWarehouseModule.ReserveItem(reservedWarehouseModule.AvailableItemTypes(warehouseModule.neededItemTypes)[0]);
                                        if (reservedItem == null) warehouseModule.CancelMoveItemTask();
                                        else
                                        {
                                            path = moveItemTempPath;
                                            ChangeCreatureState(CreatureState.Walk);
                                            reservedItem.reserver = this;
                                        }
                                    }
                                }
                                else
                                {
                                    PickUpItem(reservedItem);
                                    SetTargetCell(warehouseModule.baseUnit.currentCell);
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

        float ed = 0;
        switch (creatureState)
        {
            case CreatureState.Idle:
                ed = idleEnergyDecrease;
                break;
            case CreatureState.Walk:
                ed = walkEnergyDecrease;
                break;
            case CreatureState.CutTree:
            case CreatureState.MineStone:
            case CreatureState.Craft:
            case CreatureState.Build:
            case CreatureState.Research:
                ed = workEnergyDecrease;
                break;
            case CreatureState.Sleep:
                ed = sleepEnergyIncrease;
                if (restPoint != null) ed *= restPoint.energyRestoreBonus;
                break;
        }
        energy += ed * TimeManager.deltaTime;
        energy = Mathf.Clamp01(energy);
        RefreshBaseUnitInfoPanel();
        if (energy < tiredThreshold && task.taskType == TaskType.None && creatureState != CreatureState.Air && creatureState != CreatureState.Sleep && restPoint == null) GetClosestRestPoint();
        if (creatureState == CreatureState.Sleep && energy > restedThreshold + wakeUpOffset) EndSleep();
    }
    public void GetClosestRestPoint()
    {
        restPoint = null;
        float tempCost = float.MaxValue;
        List<Cell> tempPath = new List<Cell>();
        foreach (var rp in planet.restPoints)
        {
            if (rp.IsAvailable())
            {
                List<Cell> tempPath1 = planet.FindPath(currentCell, rp.building.currentCell);
                float tempCost1 = planet.GetPathLength(tempPath1);
                if (tempCost1 < tempCost)
                {
                    tempCost = tempCost1;
                    restPoint = rp;
                    tempPath = tempPath1;
                }
            }
        }
        if (restPoint != null)
        {
            SetPath(tempPath);
            restPoint.creatures.Add(this);
            walkAngleOffset = UnityEngine.Random.Range(-planet.cellIntervalAngle / 2, planet.cellIntervalAngle / 2);
        }
        else StartSleep();
    }
    public void StartSleep()
    {
        wakeUpOffset = UnityEngine.Random.Range(0, 1 - restedThreshold);
        ChangeCreatureState(CreatureState.Sleep);
        sleepTip.SetActive(true);
    }
    public void EndSleep()
    {
        ChangeCreatureState(CreatureState.Idle);
        FindTask();
        sleepTip.SetActive(false);

        restPoint.RemoveCreature(this);
        restPoint = null;
    }
    public void RefreshBaseUnitInfoPanel()
    {
        if (MouseManager.instance.baseUnit == this) MouseManager.instance.baseUnitInfoPanel.SetBaseUnitInfoPanel(this);
    }
    public override void LateUpdate()
    {
        base.LateUpdate();

        isSettingTask = false;
    }
    public void ChangeCreatureState(CreatureState cs)
    {
        transform.localScale = Vector3.one;
        creatureState = cs;
        switch (cs)
        {
            case CreatureState.Idle:
                path.Clear();
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
                string dir = "";
                if (punchStoneDirection == Vector2.down) dir = "Down";
                else if (punchStoneDirection == Vector2.right) dir = "Right";
                else if (punchStoneDirection == Vector2.left) dir = "Left";
                if (punchStoneDirection == Vector2.left) transform.localScale = new Vector3(-1, 1, 1);
                animator.Play("PunchStone" + dir, 0, 0f);
                break;
            case CreatureState.Air:
                animator.Play("Fall", 0, 0f);
                break;
            case CreatureState.Build:
                animator.Play("Build", 0, 0f);
                break;
            case CreatureState.Craft:
                animator.Play("Craft", 0, 0f);
                break;
            case CreatureState.Research:
                animator.Play("Research", 0, 0f);
                break;
            case CreatureState.Sleep:
                animator.Play("Sleep", 0, 0f);
                break;
        }
    }
    public void CutTree()
    {
        wood.CutTree(progressPerCutTree);
    }
    public void MineStone()
    {
        stone.MineStone(progressPreMineStone);
    }
    public void Craft()
    {
        billModule.Craft(processPerCraft);
    }
    public void Research()
    {
        researchBench.Research(processPerResearch);
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
        SetPath(temp);
    }
    public void SetPath(List<Cell> p)
    {
        if (p != null)
        {
            path = p;
            ChangeCreatureState(CreatureState.Walk);
        }
    }
    public void SetTask(Task t, float c)
    {
        if (t.taskType == TaskType.None)
        {
            //ChangeCreatureState(CreatureState.Idle);
            return;
        }
        //Debug.Log(Time.tim;
        Debug.Log(t.taskType + " " + gameObject.name + " " + Time.time);
        isSettingTask = true;
        task = t;
        walkAngleOffset = 0;
        if (!TaskManager.instance.sharedTaskTypes.Contains(t.taskType)) TaskManager.instance.taskToCreatureTaskNodes[t].Clear();
        TaskManager.instance.taskToCreatureTaskNodes[t].Add(new CreatureTaskNode(this, c));
        //Debug.Log("SetTask" + " " + task.taskType + " " + Time.time);
        switch (task.taskType)
        {
            case TaskType.CutTree:
                SetTargetCell(task.baseUnits[0].currentCell);
                wood = task.baseUnits[0].GetComponent<Wood>();
                break;
            case TaskType.MineStone:
                if (punchStoneDirection == Vector2.right)
                {
                    walkAngleOffset = -0.3f;
                    SetTargetCell(task.baseUnits[0].currentCell.neighbourCellNodes[2].cell);
                }
                else if (punchStoneDirection == Vector2.left)
                {
                    walkAngleOffset = 0.3f;
                    SetTargetCell(task.baseUnits[0].currentCell.neighbourCellNodes[3].cell);
                }
                else
                {
                    walkAngleOffset = 0;
                    SetTargetCell(task.baseUnits[0].currentCell.neighbourCellNodes[0].cell);
                }

                stone = task.baseUnits[0].GetComponent<Stone>();
                break;
            case TaskType.Craft:
                SetTargetCell(task.baseUnits[0].currentCell);
                billModule = task.baseUnits[0].GetComponent<BillModule>();
                break;
            case TaskType.Research:
                SetTargetCell(task.baseUnits[0].currentCell);
                researchBench = task.baseUnits[0].GetComponent<ResearchBench>();
                break;
            case TaskType.MoveItem:
                if (task.baseUnits[0] is PlacedObject)
                {
                    PlacedObject moveItemPlacedObject = task.baseUnits[0] as PlacedObject;
                    List<Cell> buildTempPath = PathToClosetItem(moveItemPlacedObject.requiredItemTypes, planet.warehouseModules, out reservedItem, out reservedWarehouseModule);
                    if (reservedItem == null && reservedWarehouseModule == null) moveItemPlacedObject.CancelMoveItemTask();
                    else
                    {
                        path = buildTempPath;
                        ChangeCreatureState(CreatureState.Walk);
                        //if (reservedWarehouseModule != null) reservedItem = reservedWarehouseModule.ReserveItem(moveItemPlacedObject.requiredItemTypes.GetIntersection(reservedWarehouseModule.itemTypeToNumber.Keys)[0]);
                        reservedItem.reserver = this;
                    }
                }
                else if (task.baseUnits[0] is Item)
                {
                    Item weaponItem = task.baseUnits[0] as Item;
                    List<Cell> armPath = planet.FindPath(currentCell, weaponItem.currentCell);
                    path = armPath;
                    ChangeCreatureState(CreatureState.Walk);
                    weaponItem.reserver = this;
                }
                else
                {
                    WarehouseModule warehouseModule = task.baseUnits[0].GetComponent<WarehouseModule>();
                    List<ItemType> tempItemTypes = new List<ItemType>();
                    if (warehouseModule.isAllItemTypesNeeded) foreach (ItemType itt in Enum.GetValues(typeof(ItemType)))
                        {
                            if (!PoolManager.instance.weaponItemTypes.Contains(itt)) tempItemTypes.Add(itt);
                        }
                    else tempItemTypes = new List<ItemType>(warehouseModule.neededItemTypes);
                    List<WarehouseModule> tempWarehouseModules = new List<WarehouseModule>();
                    if (warehouseModule.GetComponent<BillModule>() != null)
                    {
                        tempWarehouseModules = new List<WarehouseModule>(planet.warehouseModules);
                        tempWarehouseModules.Remove(warehouseModule);
                    }
                    List<Cell> moveItemTempPath = PathToClosetItem(tempItemTypes, tempWarehouseModules, out reservedItem, out reservedWarehouseModule);
                    if (reservedItem == null) warehouseModule.CancelMoveItemTask();
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
        path.Insert(0, currentCell);
        /*
        string temp = "";
        foreach (var cell in path) temp += cell.radiusIdx + " " + cell.angleIdx + " " + cell.layerIdx + " | ";
        Debug.Log(temp);
        */
    }
    public void FindTask()
    {
        if (task.taskType != TaskType.None && !isSettingTask) return;
        if (creatureState == CreatureState.Air || creatureState == CreatureState.Sleep || restPoint != null) return;

        if (currentCell != null && currentCell.neighbourCellNodes[1].cell.canStand == false && creatureState != CreatureState.Air)
        {
            CancelTaskWithoutFindTask();
            ChangeCreatureState(CreatureState.Air);
            return;
        }

        //Debug.Log("FindingTask" + " " + Time.time);
        isSettingTask = false;
        //UnbindReservedItem();
        //float tempDis = float.MaxValue;
        Task tempTask = new Task(TaskType.None, new BaseUnit[0]);
        float tempCost = float.MaxValue;
        List<Task> priorityTasks = new List<Task>();
        List<Task> normalTasks = new List<Task>();
        List<Task> tasks = new List<Task>();
        Creature tempCreature = null;
        foreach (var t in TaskManager.instance.tasks)
        {
            if (t == task) continue;
            //Debug.Log(t.taskType + " " + t.baseUnits[0].transform.position);
            //Debug.Log(TaskManager.instance.taskToCreatureTaskNodes.ContainsKey(t));
            //Debug.Log(TaskManager.instance.taskToCreatureTaskNodes[t].Count > 0);
            if (TaskManager.instance.taskToCreatureTaskNodes.ContainsKey(t) && TaskManager.instance.taskToCreatureTaskNodes[t].Count > 0 && !TaskManager.instance.taskToCreatureTaskNodes[t][0].creature.isSettingTask) continue;
            if (t.taskType == TaskType.MoveItem && t.baseUnits[0] is Item && professionType != ProfessionType.None) continue;
            tasks.Add(t);
            if (priorityTaskTypes.Contains(t.taskType)) priorityTasks.Add(t);
            else normalTasks.Add(t);
        }

        punchStoneDirection = Vector2.zero;
        Vector2 tempPunchStoneDirection = Vector2.zero;

        foreach (var t in tasks)
        {
            tempPunchStoneDirection = Vector2.zero;
            List<Cell> tempPath = null;
            //Debug.Log(t.taskType + " " + t.baseUnits[0].transform.position);
            switch (t.taskType)
            {
                case TaskType.MineStone:
                    tempPath = planet.FindPath(currentCell, t.baseUnits[0].currentCell.neighbourCellNodes[0].cell);
                    tempPunchStoneDirection = Vector2.down;
                    List<Cell> tempPath1 = planet.FindPath(currentCell, t.baseUnits[0].currentCell.neighbourCellNodes[2].cell);
                    if (planet.GetPathLength(tempPath1) < planet.GetPathLength(tempPath))
                    {
                        tempPath = new List<Cell>(tempPath1);
                        tempPunchStoneDirection = Vector2.right;
                    }
                    tempPath1 = planet.FindPath(currentCell, t.baseUnits[0].currentCell.neighbourCellNodes[3].cell);
                    if (planet.GetPathLength(tempPath1) < planet.GetPathLength(tempPath))
                    {
                        tempPath = new List<Cell>(tempPath1);
                        tempPunchStoneDirection = Vector2.left;
                    }
                    break;
                default:
                    tempPath = planet.FindPath(currentCell, t.baseUnits[0].currentCell);
                    break;
            }
            float cost = planet.GetPathLength(tempPath);
            //Debug.Log(cost + " " + Time.time);
            if (cost >= tempCost || cost == float.MaxValue) continue;

            if (TaskManager.instance.taskToCreatureTaskNodes[t].Count > 0)
            {
                //Debug.Log(cost + " " + TaskManager.instance.taskToCreatureTaskNodes[t][0].cost + " " + currentCell.angleIdx);
                if (cost >= TaskManager.instance.taskToCreatureTaskNodes[t][0].cost) continue;
                tempCreature = TaskManager.instance.taskToCreatureTaskNodes[t][0].creature;
                //continue;
            }

            UnbindReservedItem();

            tempCost = cost;

            if (tempTask.taskType == TaskType.None) tempTask = t;
            else if (priorityTaskTypes.Contains(t.taskType) && !priorityTaskTypes.Contains(tempTask.taskType)) tempTask = t;
            else
            {
                tempTask = t;
            }
            if (tempPunchStoneDirection != Vector2.zero) punchStoneDirection = tempPunchStoneDirection;

        }
        //if (tempTask != null) Debug.Log(tempTask.taskType.ToString() + " " + Time.time);
        if (tempCreature != null && tempTask.taskType != TaskType.None && TaskManager.instance.taskToCreatureTaskNodes[tempTask].Count > 0 && tempCreature != TaskManager.instance.taskToCreatureTaskNodes[tempTask][0].creature) tempCreature = null;
        if (tempCreature != null) tempCreature.UnbindReservedItem();
        if (tempCost != float.MaxValue) SetTask(tempTask, tempCost);

        if (tempCreature != null) tempCreature.CancelTask();

    }
    public void CancelTask()
    {
        CancelTaskWithoutFindTask();
        //SetTargetCell(currentCell);
        FindTask();
    }
    public void CancelTaskWithoutFindTask()
    {
        isSettingTask = false;
        UnbindReservedItem();
        if (task.taskType != TaskType.None) Debug.Log("Cancel" + " " + task.taskType + " " + Time.time);
        if (task.taskType != TaskType.None && TaskManager.instance.taskToCreatureTaskNodes.Keys.Contains(task)) for (int i = 0; i < TaskManager.instance.taskToCreatureTaskNodes[task].Count; i++) if (TaskManager.instance.taskToCreatureTaskNodes[task][i].creature == this) TaskManager.instance.taskToCreatureTaskNodes[task].RemoveAt(i);
        task = new Task(TaskType.None, new BaseUnit[0]);
        ChangeCreatureState(CreatureState.Idle);

    }
    public void UnbindReservedItem()
    {
        if (reservedItem == null) return;
        //Debug.Log(Time.time);
        if (reservedWarehouseModule != null)
        {
            if (!reservedWarehouseModule.itemTypeToNumber.ContainsKey(reservedItem.itemType)) reservedWarehouseModule.itemTypeToNumber[reservedItem.itemType] = 0;
            reservedWarehouseModule.itemTypeToNumber[reservedItem.itemType]++;
            reservedItem.reserver = null;
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
    public List<Cell> PathToClosetItem(List<ItemType> itemTypes, List<WarehouseModule> warehouseModules, out Item itm, out WarehouseModule wh)
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
                //List<Cell> tempPath = planet.FindPathWithMaxDistance(currentCell, item.currentCell, minCost);
                //Debug.Log(Time.time + " " + planet.GetPathLength(tempPath) + " " + minCost + " " + planet.items.Count);
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

        foreach (WarehouseModule warehouseModule in planet.warehouseModules)
        {
            if (warehouseModules.Contains(warehouseModule) && warehouseModule.IsItemAvailable(itemTypes))
            {
                //List<Cell> tempPath = planet.FindPathWithMaxDistance(currentCell, warehouseModule.baseUnit.currentCell, minCost);
                List<Cell> tempPath = planet.FindPath(currentCell, warehouseModule.baseUnit.currentCell);
                if (tempPath != null)
                {
                    float cost = planet.GetPathLength(tempPath);
                    if (cost < minCost)
                    {
                        result = tempPath;
                        wh = warehouseModule;
                        minCost = cost;
                    }
                }
            }
        }
        if (wh != null) itm = wh.ReserveItem(itemTypes.GetIntersection(wh.itemTypeToNumber.Keys)[0]);
        return result;
    }
    public List<Cell> PathToClosetItem(List<ItemType> itemTypes, out Item itm)
    {
        List<Cell> result = null;
        float minCost = float.MaxValue;
        itm = null;
        foreach (Item item in planet.items)
        {
            if (itemTypes.Contains(item.itemType) && item.reserver == null && !item.isInAir)
            {
                List<Cell> tempPath = planet.FindPath(currentCell, item.currentCell);
                //List<Cell> tempPath = planet.FindPathWithMaxDistance(currentCell, item.currentCell, minCost);
                //Debug.Log(Time.time + " " + planet.GetPathLength(tempPath) + " " + minCost + " " + planet.items.Count);
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

        return result;
    }
    public List<Cell> PathToClosetItem(out Item itm)
    {
        List<Cell> result = null;
        float minCost = float.MaxValue;
        itm = null;
        foreach (Item it in planet.items)
        {
            if (it.reserver == null && !it.isInAir)
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
    public void ArmWeapon(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Axe:
                ChangeProfession(ProfessionType.Lumberjack);
                break;
            case ItemType.Pickaxe:
                ChangeProfession(ProfessionType.Miner);
                break;
            case ItemType.Shoe:
                ChangeProfession(ProfessionType.Mover);
                break;
        }
    }
    public void ChangeProfession(ProfessionType pt)
    {
        foreach (var gameObject in professionTypeToGameobjects[professionType].gameObjects) gameObject.SetActive(false);
        professionType = pt;
        foreach (var gameObject in professionTypeToGameobjects[professionType].gameObjects) gameObject.SetActive(true);

        switch (professionType)
        {
            case ProfessionType.Lumberjack:
                progressPerCutTree = lumberjackProgress;
                break;
            case ProfessionType.Miner:
                progressPreMineStone = minerProgress;
                break;
            case ProfessionType.Mover:
                walkSpeed = moverSpeed;
                break;
        }

    }
    public void OnTimeScaleChange(float ts)
    {
        animator.speed = ts;
    }

    //physics
    public void AddForce(Vector2 force)
    {
        ChangeCreatureState(CreatureState.Air);
        velocity += (Vector3)force;
    }
    public void AddForce(Vector3 force)
    {
        ChangeCreatureState(CreatureState.Air);
        velocity += force;
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0, creatureHeight));
    }
}
