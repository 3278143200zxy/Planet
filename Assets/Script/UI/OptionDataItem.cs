using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionDataItem : MonoBehaviour
{
    public CustomDropdown dropdown;

    public ItemRecipe itemRecipe;
    public Text text;
    public Image image;

    public void SetData(ItemRecipe _itemRecipe)
    {
        itemRecipe = _itemRecipe;
        text.text = itemRecipe.itemType.ToString();
        //image.sprite = sprite;
    }
    public void OnClick()
    {
        dropdown.OnOptionItemClickedEvent.Invoke(itemRecipe);
        //Debug.Log(text.text);
    }
}
