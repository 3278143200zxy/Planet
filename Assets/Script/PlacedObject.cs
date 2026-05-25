using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemTypeToNumberDictionary : SerializableDictionary<ItemType, int>
{
    public ItemTypeToNumberDictionary(ItemTypeToNumberDictionary other)
    {
        if (other != null)
        {
            foreach (var kvp in other)
            {
                Add(kvp.Key, kvp.Value);
            }
        }
    }
    public ItemTypeToNumberDictionary() { }
}
public class PlacedObject : BaseUnit
{
    public List<Dot> dots = new List<Dot>() { new Dot(0, 0, 0) };

    //public List<ItemNode> itemNodes = new List<ItemNode>();
    //stored items
    [HideInInspector]
    public ItemTypeToNumberDictionary itemTypeToNumber = new ItemTypeToNumberDictionary();
    //required items, need to validate in the inspector
    public ItemTypeToNumberDictionary requiredItemTypeToNumber = new ItemTypeToNumberDictionary();
    //requiring items' types
    [HideInInspector]
    public List<ItemType> requiredItemTypes = new List<ItemType>();

    public Dictionary<Item, Task> itemToTask = new Dictionary<Item, Task>();

    public Building buildingPrefab;

    public Task task = null;

    [HideInInspector]
    public float totalItemNumber;
    [HideInInspector]
    public float itemNumber;
    public float totalBuildingProgress;
    [HideInInspector]
    public float buildingProgress;

    public SpriteRenderer spriteRenderer;
    public MaterialPropertyBlock mpb;

    public GameObject buildTip;

    public override void Awake()
    {
        //Debug.Log(baseUnitInfo.actionTypes.Count + " " + Time.time);
        base.Awake();

        //Debug.Log(baseUnitInfo.actionTypes.Count);
        actionTypeToEvent[ActionType.CancelBuild].AddListener(CancelTaskButton);
        actionTypeToEvent[ActionType.Build].AddListener(StartTaskButton);

        clickCircles = buildingPrefab.clickCircles;
        dots = buildingPrefab.dots;

        mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FillAmount_White", 0);
        mpb.SetFloat("_FillAmount_Original", 0);
        spriteRenderer.SetPropertyBlock(mpb);

        foreach (var itemNode in requiredItemTypeToNumber)
        {
            totalItemNumber += requiredItemTypeToNumber[itemNode.Key];
            requiredItemTypes.Add(itemNode.Key);
            itemTypeToNumber[itemNode.Key] = 0;
        }
        //totalBuildingProgress = 0;
        /*
        foreach (var itemNode in itemNodes)
        {
            requiredItemTypeToNumber[itemNode.itemType] = itemNode.number;
            itemTypeToNumber[itemNode.itemType] = 0;
            requiredItemTypes.Add(itemNode.itemType);

            //totalBuildingProgress += itemNode.number;
            totalItemNumber += itemNode.number;
        }
        */
        canClick = false;

        //Debug.Log(baseUnitInfo.actionTypes.Count);
    }
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    public void SetPlacedObject(Cell cell)
    {
        canClick = true;

        foreach (Dot d in dots)
        {
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = -d.x + cell.angleIdx, layerIdx = -d.z + cell.layerIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx, layerIdx];
                processingCell.placedObject = this;
            }
        }
        //building = Instantiate(buildingPrefab, transform.position, transform.rotation);


        //building.SetBuilding(cell, dots, standDots);
        //Destroy(gameObject);
        buildTip.SetActive(true);

        if (IsItemAvailable()) StartMoveItemTask();
        else CancelMoveItemTask();

        MouseManager.instance.placedObject = null;
        MouseManager.instance.SelectBaseUnit(this);

        transform.localScale = new Vector3(1 + (cell.radiusIdx - planet.surfaceRadius) * planet.cellSizeCorrection, 1, 1);
    }
    public void StartMoveItemTask()
    {
        task = new Task(TaskType.MoveItem, new BaseUnit[] { this });
        TaskManager.instance.AddTaskWithoutCreatureFindTask(task);
        TaskManager.instance.isCreatureFindTaskFrame = true;
        planet.ItemHitGroundEvent.RemoveListener(ItemHitGround);
    }
    public void CancelMoveItemTask()
    {
        TaskManager.instance.RemoveTask(task);
        planet.ItemHitGroundEvent.AddListener(ItemHitGround);
        task = null;
    }
    public void StartBuildTask()
    {
        task = new Task(TaskType.Build, new BaseUnit[] { this });
        TaskManager.instance.AddTaskWithoutCreatureFindTask(task);
        TaskManager.instance.isCreatureFindTaskFrame = true;
    }
    public void CancelBuildTask()
    {
        TaskManager.instance.RemoveTask(task);
        task = null;
    }
    public void StartTaskButton()
    {
        AddActionType(this, ActionType.CancelBuild);
        RemoveActionType(this, ActionType.Build);

        if (itemNumber < totalItemNumber)
        {
            if (IsItemAvailable()) StartMoveItemTask();
            else CancelMoveItemTask();
        }
        else
        {
            StartBuildTask();
        }

        buildTip.SetActive(true);
    }
    public void CancelTaskButton()
    {

        AddActionType(this, ActionType.Build);
        RemoveActionType(this, ActionType.CancelBuild);

        if (task == null) planet.ItemHitGroundEvent.RemoveListener(ItemHitGround);
        else
            switch (task.taskType)
            {
                case TaskType.MoveItem:
                    TaskManager.instance.RemoveTask(task);
                    planet.ItemHitGroundEvent.RemoveListener(ItemHitGround);
                    task = null;
                    break;
                case TaskType.Build:
                    CancelBuildTask();
                    break;
            }

        buildTip.SetActive(false);
    }
    public bool IsItemAvailable()
    {
        foreach (var it in planet.items)
        {
            if (requiredItemTypes.Contains(it.itemType) && !it.isInAir) return true;
        }
        foreach (var warehouse in planet.warehouseModules)
        {
            if (warehouse.IsItemAvailable(requiredItemTypes)) return true;
        }
        return false;
    }
    public override void DestoryBaseUnit()
    {
        planet.ItemHitGroundEvent.RemoveListener(ItemHitGround);

        foreach (Dot d in dots)
        {
            int radiusIdx = d.y + currentCell.radiusIdx, angleIdx = -d.x + currentCell.angleIdx, layerIdx = -d.z + currentCell.layerIdx;
            if (radiusIdx >= currentCell.planet.innerRadius && radiusIdx < currentCell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / currentCell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = currentCell.planet.grid[radiusIdx, angleIdx, layerIdx];
                processingCell.placedObject = this;
            }
        }
        base.DestoryBaseUnit();
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
    public void ItemHitGround(ItemType itemType)
    {
        if (!requiredItemTypes.Contains(itemType)) return;
        StartMoveItemTask();
    }
    public void AddItem(Item item)
    {
        itemTypeToNumber[item.itemType]++;
        item.DestoryBaseUnit();
        // Debug.Log(1);

        itemNumber++;
        spriteRenderer.GetPropertyBlock(mpb);
        //Debug.Log(Mathf.Clamp01(itemNumber / totalItemNumber));
        mpb.SetFloat("_FillAmount_White", Mathf.Clamp01(itemNumber / totalItemNumber));
        spriteRenderer.SetPropertyBlock(mpb);


        if (itemTypeToNumber[item.itemType] >= requiredItemTypeToNumber[item.itemType])
        {
            requiredItemTypes.Remove(item.itemType);


            if (itemNumber >= totalItemNumber)
            {
                TaskManager.instance.RemoveTaskWithoutCancelCreatureTask(task);
                StartBuildTask();
            }

        }
        RefreshBaseUnitInfoPanel();
    }
    public void BuildPlacedObject(float p)
    {
        buildingProgress += p;

        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FillAmount_Original", Mathf.Clamp01(buildingProgress / totalBuildingProgress));
        spriteRenderer.SetPropertyBlock(mpb);

        if (buildingProgress >= totalBuildingProgress) SetBuilding();

        RefreshBaseUnitInfoPanel();
    }
    public void SetBuilding()
    {

        Building building = Instantiate(buildingPrefab, transform.position, transform.rotation);
        building.ChangeLayer(building.gameObject, gameObject.layer);
        building.SetBuilding(currentCell);
        if (MouseManager.instance.IsBaseUnitSelected(this)) MouseManager.instance.SelectBaseUnit(building);

        CancelBuildTask();

        //QtreeManager.instance.baseUnits.Add(building);

        DestoryBaseUnit();

    }
    public void RefreshBaseUnitInfoPanel()
    {
        if (MouseManager.instance.baseUnit == this) MouseManager.instance.baseUnitInfoPanel.SetBaseUnitInfoPanel(this);
    }
}
