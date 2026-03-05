using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillInfoPanel : MonoBehaviour
{
    public BillModule billModule;
    public CustomDropdown dropDown;

    public List<BillInfoItem> billInfoItems = new List<BillInfoItem>();
    public List<BillInfoItem> billInfoItemPool = new List<BillInfoItem>();
    public Transform content;

    private void Awake()
    {
        dropDown.OnOptionItemClickedEvent.AddListener(OnOptionItemClicked);
        gameObject.SetActive(false);

    }
    public void OnOptionItemClicked(ItemRecipe itemRecipe)
    {
        billModule.OnOptionItemClicked(itemRecipe);
        RefreshShownValue();
    }
    public void SetBillInfoPanel(BillModule bm)
    {
        billModule = bm;
        gameObject.SetActive(true);
        dropDown.ClearOptions();
        dropDown.AddOptions(bm.itemRecipes);
        RefreshShownValue();
    }
    public void RefreshShownValue()
    {
        for (int i = 0; i < billInfoItems.Count; i++)
        {
            BillInfoItem billInfoItem = billInfoItems[i];
            billInfoItem.gameObject.SetActive(false);
            billInfoItem.transform.SetParent(transform);

            billInfoItemPool.Add(billInfoItem);
        }
        billInfoItems.Clear();
        List<BillInfo> billInfos = billModule.billInfos;
        for (int i = 0; i < billInfos.Count; i++)
        {
            BillInfoItem item;
            if (billInfoItemPool.Count > 0)
            {
                item = billInfoItemPool[0];
                billInfoItemPool.RemoveAt(0);

                item.transform.SetParent(content);
                item.gameObject.SetActive(true);
            }
            else item = Instantiate(billInfoItems[0], content);

            item.SetData(billInfos[i]);
            item.transform.SetAsLastSibling();
            billInfoItems.Add(item);
        }
    }
}
