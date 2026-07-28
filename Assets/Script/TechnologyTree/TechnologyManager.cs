using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TechnologyManager : MonoBehaviour
{
    public static TechnologyManager instance;

    public TechnologyPanel technologyPanel;

    public TechnologyNode technologyNode;

    public UnityEvent startResearchEvent = new UnityEvent();
    public UnityEvent cancelResearchEvent = new UnityEvent();

    public GameObject placedObjectButtonContainer;
    public GameObject hidingPlacedObjectButtonContainer;

    public Dictionary<Building, PlacedObjectButton> buildingTypeToButton = new Dictionary<Building, PlacedObjectButton>();

    public List<PlacedObjectButton> initialPlacedObjectButtons = new List<PlacedObjectButton>();
    private void Awake()
    {
        instance = this;

        List<PlacedObjectButton> placedObjectButtons = new List<PlacedObjectButton>(placedObjectButtonContainer.GetComponentsInChildren<PlacedObjectButton>());
        foreach (var button in placedObjectButtons)
        {
            buildingTypeToButton[button.placedObjectPrefab.buildingPrefab] = button;
            if (!initialPlacedObjectButtons.Contains(button)) button.transform.SetParent(hidingPlacedObjectButtonContainer.transform);
        }

    }
    public void OnTechnologyResearched(TechnologyNode technology)
    {
        foreach (var building in technology.buildings)
        {
            PlacedObjectButton button = buildingTypeToButton[building];
            button.transform.SetParent(placedObjectButtonContainer.transform);
        }
    }
    public void Research(float p)
    {
        technologyNode.AddProgress(p);

        technologyPanel.RefreshDisplay();
    }
}
