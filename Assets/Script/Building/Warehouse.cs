using System;
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

    public Dictionary<ItemType, Sprite> itemTypeToSprite = new Dictionary<ItemType, Sprite>();
    public Dictionary<ItemType, ShowItemNode> itemTypeToShowItemNode = new Dictionary<ItemType, ShowItemNode>();
    public ShowItemNode showItemNodePrefab;
    public Transform showItemNodePool;

    private void Awake()
    {
        building = GetComponent<Building>();



        building.OnDestoryEvent.AddListener(OnDestoryFunction);

        building.planet.warehouses.Add(this);

        if (building.planet.items.Count > 0) StartMoveItemTask();
        else CancelMoveItemTask();

        itemTypeToSprite = new Dictionary<ItemType, Sprite>(PoolManager.instance.itemTypeToSprite);
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
        StartMoveItemTask();
    }
    public bool IsFull()
    {
        if (storage >= capacity) return true;
        return false;
    }
    public bool IsItemAvailable(List<ItemType> itemTypes)
    {
        if (itemTypeToNumber.Keys.HasIntersection(itemTypes)) return true;
        return false;
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
        if (itemTypeToNumber.Keys.Contains(itemType)) itemTypeToNumber[itemType]++;

        else
        {
            itemTypeToNumber[itemType] = 1;

            ShowItemNode showItemNode = itemTypeToShowItemNode[itemType] = Instantiate(showItemNodePrefab);
            showItemNode.SetShowItemNode(itemTypeToSprite[itemType], showItemNodePool);
        }
        itemTypeToShowItemNode[itemType].AddNumber(1);

        building.planet.ItemHitGround(itemType);
    }
    public void RemoveItem(ItemType itemType)
    {
        if (IsFull()) TaskManager.instance.AddTask(moveItemTask);
        storage--;
        itemTypeToShowItemNode[itemType].AddNumber(-1);
        if (itemTypeToShowItemNode[itemType].number == 0)
        {
            Destroy(itemTypeToShowItemNode[itemType].gameObject);
            itemTypeToShowItemNode.Remove(itemType);
        }
    }
    public void OnDestoryFunction()
    {
        building.planet.warehouses.Remove(this);
    }
}
