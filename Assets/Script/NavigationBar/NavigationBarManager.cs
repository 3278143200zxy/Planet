using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationBarManager : MonoBehaviour
{
    public static NavigationBarManager instance;

    public GameObject buildingGroup;
    public GameObject TechnologyGroup;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

    }
    public void BuildingButtonClick()
    {
        if (!buildingGroup.activeInHierarchy)
        {
            buildingGroup.SetActive(true);
            TechnologyGroup.SetActive(false);
        }
        else
        {
            buildingGroup.SetActive(false);
        }
    }
    public void TechnologyButtonClick()
    {
        if (!TechnologyGroup.activeInHierarchy)
        {
            buildingGroup.SetActive(false);
            TechnologyGroup.SetActive(true);
            MouseManager.instance.DeselectBaseUnit();
        }
        else
        {
            TechnologyGroup.SetActive(false);
        }
    }
}

