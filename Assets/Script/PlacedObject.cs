using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlacedObject : BaseUnit
{
    public List<Dot> dots = new List<Dot>() { new Dot(0, 0) };

    public List<ItemNode> itemNodes = new List<ItemNode>();
    public Dictionary<ItemType, int> itemTypeToNumber = new Dictionary<ItemType, int>();
    public List<ItemType> requiredItemTypes = new List<ItemType>();

    public Dictionary<Item, Task> itemToTask = new Dictionary<Item, Task>();

    public Building buildingPrefab;
    private Building building;

    public Task buildTask = null;

    public override void Awake()
    {
        base.Awake();

        clickCircles = buildingPrefab.clickCircles;
        baseUnitInfo = buildingPrefab.baseUnitInfo;
        dots = buildingPrefab.dots;

        foreach (var itemNode in itemNodes)
        {
            itemTypeToNumber[itemNode.itemType] = 0;
            requiredItemTypes.Add(itemNode.itemType);
        }

        canClick = false;
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
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = d.x + cell.angleIdx;
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
        StartBuildingTask();
    }
    public void StartBuildingTask()
    {
        buildTask = new Task(TaskType.Build, new BaseUnit[] { this });
        TaskManager.instance.AddTask(buildTask);
    }
    public void CancelBuildingTask()
    {
        TaskManager.instance.RemoveTask(buildTask);
    }
    public override void DestoryBaseUnit()
    {

        base.DestoryBaseUnit();
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
    public void AddItem(Item item)
    {
        itemTypeToNumber[item.itemType]++;
        item.DestoryBaseUnit();
    }
}
