using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlacedObject : BaseUnit
{
    public List<Dot> dots = new List<Dot>() { new Dot(0, 0) };

    public List<ItemNode> itemNodes = new List<ItemNode>();
    public Dictionary<ItemType, int> itemTypeToNumber = new Dictionary<ItemType, int>();
    public Dictionary<ItemType, int> requiredItemTypeToNumber = new Dictionary<ItemType, int>();
    public List<ItemType> requiredItemTypes = new List<ItemType>();

    public Dictionary<Item, Task> itemToTask = new Dictionary<Item, Task>();

    public Building buildingPrefab;
    private Building building;

    public Task task = null;

    public float totalItemNumber;
    public float itemNumber;
    public float totalBuildingProgress;
    public float buildingProgress;

    public SpriteRenderer spriteRenderer;
    public MaterialPropertyBlock mpb;

    public override void Awake()
    {
        base.Awake();

        //spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        clickCircles = buildingPrefab.clickCircles;
        baseUnitInfo = buildingPrefab.baseUnitInfo;
        dots = buildingPrefab.dots;

        totalBuildingProgress = 0;
        foreach (var itemNode in itemNodes)
        {
            requiredItemTypeToNumber[itemNode.itemType] = itemNode.number;
            itemTypeToNumber[itemNode.itemType] = 0;
            requiredItemTypes.Add(itemNode.itemType);

            totalBuildingProgress += itemNode.number;
            totalItemNumber += itemNode.number;
        }

        canClick = false;

        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FillAmount_White", 0);
        mpb.SetFloat("_FillAmount_Original", 0);
        spriteRenderer.SetPropertyBlock(mpb);
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
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = -d.x + cell.angleIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx];
                processingCell.placedObject = this;
            }
        }
        //building = Instantiate(buildingPrefab, transform.position, transform.rotation);


        //building.SetBuilding(cell, dots, standDots);
        //Destroy(gameObject);
        if (IsItemAvailable()) StartMoveItemTask();
        else CancelMoveItemTask();
    }
    public void StartMoveItemTask()
    {
        task = new Task(TaskType.MoveItem, new BaseUnit[] { this });
        TaskManager.instance.AddTask(task);
        planet.ItemHitGroundEvent.RemoveListener(ItemHitGround);
    }
    public void CancelMoveItemTask()
    {
        TaskManager.instance.RemoveTask(task);
        planet.ItemHitGroundEvent.AddListener(ItemHitGround);
    }
    public void StartBuildTask()
    {
        task = new Task(TaskType.Build, new BaseUnit[] { this });
        TaskManager.instance.AddTask(task);
    }
    public void CancelBuildTask()
    {
        TaskManager.instance.RemoveTask(task);
    }
    public bool IsItemAvailable()
    {
        foreach (var it in planet.items)
        {
            if (requiredItemTypes.Contains(it.itemType) && !it.isInAir) return true;
        }
        foreach (var warehouse in planet.warehouses)
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
            int radiusIdx = d.y + currentCell.radiusIdx, angleIdx = -d.x + currentCell.angleIdx;
            if (radiusIdx >= currentCell.planet.innerRadius && radiusIdx < currentCell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / currentCell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = currentCell.planet.grid[radiusIdx, angleIdx];
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

        if (itemTypeToNumber[item.itemType] >= requiredItemTypeToNumber[item.itemType])
        {
            itemNumber++;
            requiredItemTypes.Remove(item.itemType);


            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_FillAmount_White", Mathf.Clamp01(itemNumber / totalItemNumber));
            spriteRenderer.SetPropertyBlock(mpb);

            if (itemNumber >= totalItemNumber)
            {
                CancelMoveItemTask();
                StartBuildTask();
            }


        }
    }
    public void BuildPlacedObject(float p)
    {
        buildingProgress += p;

        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_FillAmount_Original", Mathf.Clamp01(buildingProgress / totalBuildingProgress));
        spriteRenderer.SetPropertyBlock(mpb);

        if (buildingProgress >= totalBuildingProgress) SetBuilding();
    }
    public void SetBuilding()
    {

        building = Instantiate(buildingPrefab, transform.position, transform.rotation);
        building.SetBuilding(currentCell);

        CancelBuildTask();

        //QtreeManager.instance.baseUnits.Add(building);

        DestoryBaseUnit();

    }
}
