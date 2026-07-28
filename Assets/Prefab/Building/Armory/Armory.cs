using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armory : MonoBehaviour
{
    public Building building;

    public BillModule billModule;

    private void Awake()
    {
        billModule.OnOptionItemClickedEvent.AddListener(OnOptionItemClicked);
    }

    public void OnOptionItemClicked(ItemType itemType)
    {

    }
}
