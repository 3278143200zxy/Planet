using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

[Serializable]
public class ItemRecipe
{
    public ItemType itemType;
    public ItemTypeToNumberDictionary itemTypeToNumberDictionary = new ItemTypeToNumberDictionary();
}
public class BillModule : MonoBehaviour
{
    public BaseUnit baseUnit;
    public WarehouseModule warehouseModule;

    public Task craftTask;

    public List<ItemRecipe> itemRecipes = new List<ItemRecipe>();
    public UnityEvent<ItemType> OnOptionItemClickedEvent = new UnityEvent<ItemType>();

    public List<BillInfo> billInfos = new List<BillInfo>();
    public ItemTypeToNumberDictionary itemTypeToNumberDictionary = new ItemTypeToNumberDictionary();

    public ItemTypeToNumberDictionary missingItems = new ItemTypeToNumberDictionary();

    public Transform spawnItemPos;
    private void Awake()
    {
        baseUnit.OnBaseUnitSelectedEvent.AddListener(OnBaseUnitSelected);
        baseUnit.OnBaseUnitDeselectedEvent.AddListener(OnBaseUnitDeselected);

        warehouseModule.AddItemEvent.AddListener(WarehouseModuleAddItem);
    }
    public void OnOptionItemClicked(ItemRecipe ir)
    {
        billInfos.Add(new BillInfo(ir));

        if (billInfos.Count == 1)
        {
            missingItems = new ItemTypeToNumberDictionary(ir.itemTypeToNumberDictionary);
            warehouseModule.SetNeededItemTypes(new List<ItemType>(missingItems.Keys));
        }
    }
    public void WarehouseModuleAddItem(ItemType itemType)
    {
        missingItems[itemType]--;
        if (missingItems[itemType] == 0)
        {
            missingItems.Remove(itemType);
            warehouseModule.SetNeededItemTypes(new List<ItemType>(missingItems.Keys));
            if (missingItems.Keys.Count == 0) StartCraftTask();
        }
    }
    public void Craft(float p)
    {
        billInfos[0].Craft(p);
        if (billInfos[0].craftProcess >= 3)
        {
            Item item = PoolManager.instance.InstantiateItem(billInfos[0].itemRecipe.itemType);
            item.transform.position = spawnItemPos.position;
            item.ChangeLayer(item.gameObject, gameObject.layer);

            billInfos.RemoveAt(0);
            MouseManager.instance.billInfoPanel.RefreshShownValue();
            CancelCraftTask();

            if (billInfos.Count > 0)
            {
                missingItems = new ItemTypeToNumberDictionary(billInfos[0].itemRecipe.itemTypeToNumberDictionary);
                warehouseModule.SetNeededItemTypes(new List<ItemType>(missingItems.Keys));
            }
        }
    }
    public void StartCraftTask()
    {
        craftTask = new Task(TaskType.Craft, new BaseUnit[] { baseUnit });
        TaskManager.instance.AddTask(craftTask);
    }
    public void CancelCraftTask()
    {
        TaskManager.instance.RemoveTask(craftTask);
    }
    public void OnBaseUnitSelected()
    {
        MouseManager.instance.billInfoPanel.SetBillInfoPanel(this);
    }
    public void OnBaseUnitDeselected()
    {
        MouseManager.instance.billInfoPanel.gameObject.SetActive(false);
    }
}
