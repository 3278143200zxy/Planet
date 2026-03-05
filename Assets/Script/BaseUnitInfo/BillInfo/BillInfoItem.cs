using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BillInfo
{
    public ItemRecipe itemRecipe;
    public float craftProcess;

    public BillInfo(ItemRecipe _itemRecie)
    {
        itemRecipe = _itemRecie;
        craftProcess = 0f;
    }
    public void Craft(float p)
    {
        craftProcess += p;
    }

}
public class BillInfoItem : MonoBehaviour
{
    public Text itemTypeText;

    public void SetData(BillInfo bi)
    {
        itemTypeText.text = bi.itemRecipe.itemType.ToString();
    }
}
