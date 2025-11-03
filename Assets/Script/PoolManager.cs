using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BaseUnitDataNode
{
    public BaseUnitType baseUnitType;

    public BaseUnit baseUnitPrefab;
}
[Serializable]
public struct ItemDataNode
{
    public ItemType itemType;
    public Sprite itemSprite;
}
public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    public Dictionary<BaseUnitType, List<BaseUnit>> baseUnitPool = new Dictionary<BaseUnitType, List<BaseUnit>>();
    public Dictionary<ItemType, List<Item>> itemPool = new Dictionary<ItemType, List<Item>>();

    public List<BaseUnitDataNode> baseUnitDataNodes = new List<BaseUnitDataNode>();
    public List<ItemDataNode> itemDataNodes = new List<ItemDataNode>();
    public Item itemPrefab;

    public Dictionary<BaseUnitType, BaseUnit> baseUnitTypeToPrefab = new Dictionary<BaseUnitType, BaseUnit>();
    public Dictionary<ItemType, Sprite> itemTypeToSprite = new Dictionary<ItemType, Sprite>();

    public Planet planet;
    private void Awake()
    {
        instance = this;

        foreach (BaseUnitType baseUnitType in Enum.GetValues(typeof(BaseUnitType))) baseUnitPool[baseUnitType] = new List<BaseUnit>();
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType))) itemPool[itemType] = new List<Item>();

        foreach (var baseUnitDataNode in baseUnitDataNodes) baseUnitTypeToPrefab[baseUnitDataNode.baseUnitType] = baseUnitDataNode.baseUnitPrefab;
        foreach (var itemDataNode in itemDataNodes) itemTypeToSprite[itemDataNode.itemType] = itemDataNode.itemSprite;
    }
    // Start is called before the first frame update
    void Start()
    {
        planet = MouseManager.instance.planets[0];


    }

    // Update is called once per frame
    void Update()
    {

    }
    public void DestoryBaseUnit(BaseUnit baseUnit)
    {
        if (MouseManager.instance.baseUnit == baseUnit) MouseManager.instance.DeselectBaseUnit();
        baseUnit.OnDestoryEvent.Invoke();

        switch (baseUnit.baseUnitType)
        {
            case BaseUnitType.Item:
                Item item = (Item)baseUnit;
                if (item.reserver != null) item.reserver.reservedItem = null;
                item.transform.SetParent(null);
                baseUnit.gameObject.SetActive(false);
                itemPool[item.itemType].Add(item);
                if (planet.items.Contains(item)) planet.items.Remove(item);
                break;
            default:
                baseUnit.gameObject.SetActive(false);
                baseUnitPool[baseUnit.baseUnitType].Add(baseUnit);
                break;
        }
    }
    public BaseUnit InstantiateBaseUnit(BaseUnitType baseUnitType)
    {
        return null;
    }
    public Item InstantiateItem(ItemType itemType)
    {
        Item item = null;
        for (int i = 0; i < itemPool[itemType].Count; i++) if (itemPool[itemType][i] == null) itemPool[itemType].RemoveAt(i);
        if (itemPool[itemType].Count > 0)
        {
            item = itemPool[itemType][0];
            itemPool[itemType].RemoveAt(0);
            item.gameObject.SetActive(true);
        }
        else
        {
            item = Instantiate(itemPrefab);
            item.itemType = itemType;
            item.spriteRenderer.sprite = itemTypeToSprite[itemType];
        }
        item.ResetItem();
        return item;
    }
}


