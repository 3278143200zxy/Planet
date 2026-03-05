using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomDropdown : MonoBehaviour
{
    public UnityEvent<ItemRecipe> OnOptionItemClickedEvent = new UnityEvent<ItemRecipe>();

    public List<OptionDataItem> optionDataItemPool = new List<OptionDataItem>();
    public List<OptionDataItem> optionDataItems = new List<OptionDataItem>();

    public List<ItemRecipe> options = new List<ItemRecipe>();

    public Transform content;

    public void AddOptions(List<ItemRecipe> _options)
    {
        options.AddRange(_options);
        RefreshShownValue();
    }
    public void ClearOptions()
    {
        options.Clear();
        RefreshShownValue();
    }
    public void HideShownValue()
    {
        for (int i = 0; i < optionDataItems.Count; i++)
        {
            OptionDataItem item = optionDataItems[i];
            optionDataItemPool.Add(item);
            item.gameObject.SetActive(false);
            item.transform.SetParent(transform);
        }
        optionDataItems.Clear();
    }
    public void ActiveShownValue()
    {
        HideShownValue();
        for (int i = 0; i < options.Count; i++)
        {
            OptionDataItem item;
            if (optionDataItemPool.Count > 0)
            {
                item = optionDataItemPool[0];
                optionDataItemPool.RemoveAt(0);

                item.transform.SetParent(content);
                item.gameObject.SetActive(true);
            }
            else item = Instantiate(optionDataItems[0], content);

            item.dropdown = this;
            item.SetData(options[i]);
            item.transform.SetAsLastSibling();
            optionDataItems.Add(item);
        }
    }
    public void RefreshShownValue()
    {
        if (content.gameObject.activeInHierarchy) ActiveShownValue();
    }
}