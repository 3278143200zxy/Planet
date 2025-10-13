using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Rendering.HybridV2;
using UnityEngine;



public class Warehouse : MonoBehaviour
{
    public Building building;

    public Task moveItemTask;

    private float climbSpeed;
    private float walkSpeed;

    public Dictionary<ItemType, int> itemTypeToNumber = new Dictionary<ItemType, int>();

    public int capacity;
    public int storage;
    private void Awake()
    {
        building = GetComponent<Building>();
        building.SetBuildingEvent.AddListener(SetBuilding);

        StartMoveItemTask();
        building.OnDestoryEvent.AddListener(OnDestoryFunction);

        building.planet.warehouses.Add(this);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void StartMoveItemTask()
    {
        moveItemTask = new Task(TaskType.MoveItem, new BaseUnit[] { building });
        TaskManager.instance.AddTask(moveItemTask);
        building.planet.ItemHitGroundEvent.RemoveListener(ItemHitGround);

    }
    public void CancelMoveItemTask()
    {

        TaskManager.instance.RemoveTask(moveItemTask);
        building.planet.ItemHitGroundEvent.AddListener(ItemHitGround);
    }
    public void ItemHitGround(ItemType itemType)
    {
        if (IsFull()) return;
        StartMoveItemTask();
    }
    public bool IsFull()
    {
        if (storage >= capacity) return true;
        return false;
    }
    public bool IsItemAvailable(ItemType itemType)
    {
        if (itemTypeToNumber.Keys.Contains(itemType)) return true;
        return false;
    }
    public void ReserveItem(ItemType itemType)
    {
        itemTypeToNumber[itemType]--;
        storage--;
        if (itemTypeToNumber[itemType] == 0) itemTypeToNumber.Remove(itemType);
    }
    public void AddItem(ItemType itemType)
    {
        storage++;
        if (itemTypeToNumber.Keys.Contains(itemType)) itemTypeToNumber[itemType]++;
        else itemTypeToNumber[itemType] = 1;
    }
    public void SetBuilding()
    {
        foreach (Dot d in building.standDots)
        {
            int radiusIdx = d.y + building.cell.radiusIdx, angleIdx = -d.x + building.cell.angleIdx;
            if (radiusIdx >= building.cell.planet.innerRadius && radiusIdx < building.cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / building.cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = building.cell.planet.grid[radiusIdx, angleIdx];
                //processingCell.AddAboveNeighbour(climbSpeed);
                Cell aboveCell = processingCell.neighbourCellNodes[0].cell;

                if (aboveCell != null)
                {
                    aboveCell.standNumber++;
                    //aboveCell.AddBelowNeighbour(climbSpeed);
                    Cell aboveLeftCell = aboveCell.neighbourCellNodes[2].cell;
                    if (aboveLeftCell.canStand)
                    {
                        aboveCell.AddLeftNeighbour(walkSpeed);
                        aboveLeftCell.AddRightNeighbour(walkSpeed);
                    }
                    Cell aboveRightCell = aboveCell.neighbourCellNodes[3].cell;
                    if (aboveRightCell.canStand)
                    {
                        aboveCell.AddRightNeighbour(walkSpeed);
                        aboveRightCell.AddLeftNeighbour(walkSpeed);
                    }
                }
            }
        }
    }

    public void OnDestoryFunction()
    {
        building.planet.warehouses.Remove(this);
    }
}
