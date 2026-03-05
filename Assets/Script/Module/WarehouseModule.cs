using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WarehouseModule : MonoBehaviour
{
    public BaseUnit baseUnit;

    [HideInInspector] public Task moveItemTask;

    public bool isAllItemTypesNeeded = false;
    public List<ItemType> neededItemTypes = new List<ItemType>();
    public Dictionary<ItemType, int> itemTypeToNumber = new Dictionary<ItemType, int>();

    public int capacity;
    public int storage;

    public Dictionary<ItemType, Sprite> itemTypeToSprite = new Dictionary<ItemType, Sprite>();
    public Dictionary<ItemType, ShowItemNode> itemTypeToShowItemNode = new Dictionary<ItemType, ShowItemNode>();
    public ShowItemNode showItemNodePrefab;
    public Transform showItemNodePool;

    public bool isStartedMoveItemTask = false;
    public bool isChangeTaskFrame = false;

    public UnityEvent<ItemType> AddItemEvent = new UnityEvent<ItemType>();

    private void Awake()
    {
        baseUnit.OnDestoryEvent.AddListener(OnDestoryFunction);

        baseUnit.planet.warehouseModules.Add(this);

        isStartedMoveItemTask = !IsNeededItemAvailableOnPlanet(neededItemTypes);
        if (!isStartedMoveItemTask) StartMoveItemTask();
        else CancelMoveItemTask();

        itemTypeToSprite = new Dictionary<ItemType, Sprite>(PoolManager.instance.itemTypeToSprite);
    }
    private void LateUpdate()
    {
        isChangeTaskFrame = false;
    }
    public void StartMoveItemTask()
    {
        if (isStartedMoveItemTask) return;
        isStartedMoveItemTask = true;
        isChangeTaskFrame = true;

        moveItemTask = new Task(TaskType.MoveItem, new BaseUnit[] { baseUnit });
        TaskManager.instance.AddTaskWithoutCreatureFindTask(moveItemTask);
        TaskManager.instance.isCreatureFindTaskFrame = true;
        baseUnit.planet.ItemHitGroundEvent.RemoveListener(ItemHitGround);

    }
    public void CancelMoveItemTask()
    {
        if (!isStartedMoveItemTask) return;
        isStartedMoveItemTask = false;
        isChangeTaskFrame = true;

        TaskManager.instance.RemoveTaskWithoutCreatureFindTask(moveItemTask);
        TaskManager.instance.isCreatureFindTaskFrame = true;
        baseUnit.planet.ItemHitGroundEvent.AddListener(ItemHitGround);

    }

    public void ItemHitGround(ItemType itemType)
    {
        //Debug.Log(IsNeededItemAvailableOnPlanet(neededItemTypes));
        if (IsNeededItemAvailableOnPlanet(neededItemTypes)) StartMoveItemTask();
    }
    public bool IsFull()
    {
        return storage >= capacity;
    }
    public void SetNeededItemTypes(List<ItemType> itt)
    {
        neededItemTypes = new List<ItemType>(itt);
        if (IsNeededItemAvailableOnPlanet(neededItemTypes)) StartMoveItemTask();
        else CancelMoveItemTask();
    }
    public void AddNeededItemTypes(List<ItemType> itt)
    {
        neededItemTypes.AddRange(itt);
        if (IsNeededItemAvailableOnPlanet(neededItemTypes)) StartMoveItemTask();
        else CancelMoveItemTask();
    }
    public bool IsNeededItemAvailableOnPlanet(List<ItemType> itemTypes)
    {
        if (isAllItemTypesNeeded) return true;
        if (itemTypes.Count == 0) return false;
        foreach (var it in baseUnit.planet.items)
        {
            if (itemTypes.Contains(it.itemType) && !it.isInAir && it.reserver == null) return true;
        }
        foreach (var wm in baseUnit.planet.warehouseModules)
        {
            if (wm != this && wm.IsItemAvailable(itemTypes)) return true;
        }
        return false;
    }
    public bool IsItemAvailable(List<ItemType> itemTypes)
    {
        //Debug.Log(itemTypeToNumber.Keys.HasIntersection(itemTypes) + " " + Time.time);
        return (itemTypeToNumber.Keys.HasIntersection(itemTypes));
    }
    public Item ReserveItem(ItemType itemType)
    {
        itemTypeToNumber[itemType]--;
        if (itemTypeToNumber[itemType] == 0) itemTypeToNumber.Remove(itemType);
        Item item = PoolManager.instance.InstantiateItem(itemType);
        item.gameObject.SetActive(false);
        item.transform.position = transform.position;

        return item;
    }
    public List<ItemType> AvailableItemTypes(List<ItemType> itemTypes)
    {
        return itemTypes.GetIntersection(itemTypeToNumber.Keys);
    }
    public void AddItem(ItemType itemType)
    {
        storage++;
        if (itemTypeToNumber.ContainsKey(itemType)) itemTypeToNumber[itemType]++;
        else
        {
            itemTypeToNumber[itemType] = 1;
            /*
            if (!itemTypeToShowItemNode.ContainsKey(itemType))
            {
                ShowItemNode showItemNode = itemTypeToShowItemNode[itemType] = Instantiate(showItemNodePrefab);
                showItemNode.SetShowItemNode(itemTypeToSprite[itemType], showItemNodePool);
            }
            */
        }
        //itemTypeToShowItemNode[itemType].AddNumber(1);

        AddItemEvent.Invoke(itemType);
        baseUnit.planet.ItemHitGround(itemType);

    }
    public void RemoveItem(ItemType itemType)
    {
        //if (IsFull()) TaskManager.instance.AddTask(moveItemTask);
        storage--;
        //itemTypeToShowItemNode[itemType].AddNumber(-1);
        /*
        if (itemTypeToShowItemNode[itemType].number <= 0)
        {
            Destroy(itemTypeToShowItemNode[itemType].gameObject);
            itemTypeToShowItemNode.Remove(itemType);
        }
        */
    }
    public void OnDestoryFunction()
    {
        baseUnit.planet.warehouseModules.Remove(this);
    }
}
