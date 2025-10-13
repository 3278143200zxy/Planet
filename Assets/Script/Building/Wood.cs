using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;

public class Wood : MonoBehaviour
{
    public Building building;
    public Task cutTreeTask;
    public GameObject cutTreeTip;
    public float treeCuttingProcess = 1f;
    public Slider treeCuttingProcessSlider;
    public GameObject sliderTickMarkPrefab;

    public Item woodItemPrefab;
    public int woodItemNumber;
    public float spawnItemRange;
    public void Awake()
    {
    }
    // Start is called before the first frame update
    public void Start()
    {
        building = GetComponent<Building>();
        building.actionTypeToEvent[ActionType.CutTree].AddListener(StartCuttingTreeTask);
        building.actionTypeToEvent[ActionType.CancelCuttingTree].AddListener(CancelCuttingTreeTask);

    }

    // Update is called once per frame
    public void Update()
    {

    }
    public void CutTree(float p)
    {
        treeCuttingProcess -= p;
        if (!treeCuttingProcessSlider.gameObject.activeInHierarchy) treeCuttingProcessSlider.gameObject.SetActive(true);
        treeCuttingProcessSlider.value = treeCuttingProcess / 1;
        if (treeCuttingProcess <= 0f) OnCuttedDown();
    }
    public void SetBuilding()
    {

    }
    public void OnCuttedDown()
    {
        for (int i = 0; i < woodItemNumber; i++) Instantiate(woodItemPrefab, transform.position + Random.Range(-spawnItemRange / 2, spawnItemRange / 2) * transform.right, Quaternion.identity);

        CancelCuttingTreeTask();
        building.DestoryBaseUnit();

    }

    public void StartCuttingTreeTask()
    {
        cutTreeTip.SetActive(true);
        building.AddActionType(ActionType.CancelCuttingTree);
        building.RemoveActionType(ActionType.CutTree);
        cutTreeTask = new Task(TaskType.CutTree, new BaseUnit[] { building });
        TaskManager.instance.AddTask(cutTreeTask);

    }
    public void CancelCuttingTreeTask()
    {
        cutTreeTip.SetActive(false);
        building.AddActionType(ActionType.CutTree);
        building.RemoveActionType(ActionType.CancelCuttingTree);
        TaskManager.instance.RemoveTask(cutTreeTask);
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position - new Vector3(spawnItemRange / 2, 0, 0), transform.position + new Vector3(spawnItemRange / 2, 0, 0));
    }
}
