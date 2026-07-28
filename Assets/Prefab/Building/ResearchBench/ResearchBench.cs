using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchBench : MonoBehaviour
{
    public Building building;
    public Task researchTask;

    public GameObject researchTip;

    public bool isResearching = false;

    private void Start()
    {
        TechnologyManager.instance.startResearchEvent.AddListener(StartResearchTask);
        TechnologyManager.instance.cancelResearchEvent.AddListener(CancelResearchTask);

        building.actionTypeToEvent[ActionType.Research].AddListener(StartResearchButton);
        building.actionTypeToEvent[ActionType.CancelResearch].AddListener(CancelResearchButton);
    }
    public void StartResearchButton()
    {
        researchTip.SetActive(true);
        building.AddActionType(building, ActionType.CancelResearch);
        building.RemoveActionType(building, ActionType.Research);

        isResearching = true;

        if (TechnologyManager.instance.technologyNode != null) StartResearchTask();
    }
    public void CancelResearchButton()
    {
        researchTip.SetActive(false);
        building.AddActionType(building, ActionType.Research);
        building.RemoveActionType(building, ActionType.CancelResearch);

        if (TechnologyManager.instance.technologyNode != null) CancelResearchTask();

        isResearching = false;

    }
    public void StartResearchTask()
    {
        if (!isResearching) return;

        researchTask = new Task(TaskType.Research, new BaseUnit[] { building });
        TaskManager.instance.AddTask(researchTask);
    }
    public void CancelResearchTask()
    {
        if (!isResearching) return;

        TaskManager.instance.RemoveTask(researchTask);
    }
    public void Research(float p)
    {
        TechnologyManager.instance.Research(p);
    }
}
