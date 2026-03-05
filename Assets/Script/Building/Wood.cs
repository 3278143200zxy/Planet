using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wood : MonoBehaviour
{
    public Building building;
    public Task cutTreeTask;
    public GameObject cutTreeTip;
    [HideInInspector]
    public float treeCutProgress;
    public float totalTreeCutProgress;
    public Slider treeCutProcessSlider;
    public GameObject sliderTickMarkPrefab;

    public int minWoodItemNumber;
    public int maxWoodItemNumber;
    public float spawnItemWidthRange;
    public float spawnItemHeightRange;

    public GameObject destoryEffectPrefab;
    public Transform destoryEffectPos;
    public void Awake()
    {
        treeCutProgress = totalTreeCutProgress;

    }
    // Start is called before the first frame update
    public void Start()
    {
        building.actionTypeToEvent[ActionType.CutTree].AddListener(StartCutTreeTask);
        building.actionTypeToEvent[ActionType.CancelCutTree].AddListener(CancelCutTreeTask);

    }

    // Update is called once per frame
    public void Update()
    {

    }
    public void CutTree(float p)
    {
        treeCutProgress -= p;
        if (!treeCutProcessSlider.gameObject.activeInHierarchy) treeCutProcessSlider.gameObject.SetActive(true);
        treeCutProcessSlider.value = treeCutProgress / totalTreeCutProgress;
        if (treeCutProgress <= 0f) OnCuttedDown();
    }
    public void SetBuilding()
    {

    }
    public void OnCuttedDown()
    {
        int woodItemNumber = Random.Range(minWoodItemNumber, maxWoodItemNumber);
        for (int i = 0; i < woodItemNumber; i++)
        {
            Item woodItem = PoolManager.instance.InstantiateItem(ItemType.Wood);
            woodItem.transform.position = transform.position
                + Random.Range(-spawnItemWidthRange / 2, spawnItemWidthRange / 2) * transform.right
                + Random.Range(-spawnItemHeightRange / 2, spawnItemHeightRange / 2) * transform.up;

            woodItem.ChangeLayer(woodItem.gameObject, building.spriteRenderers[0].gameObject.layer);
        }

        //Instantiate(destoryEffectPrefab, destoryEffectPos.position, transform.rotation);

        CancelCutTreeTask();
        building.DestoryBaseUnit();

    }

    public void StartCutTreeTask()
    {
        cutTreeTip.SetActive(true);
        building.AddActionType(building, ActionType.CancelCutTree);
        building.RemoveActionType(building, ActionType.CutTree);
        cutTreeTask = new Task(TaskType.CutTree, new BaseUnit[] { building });
        TaskManager.instance.AddTask(cutTreeTask);

    }
    public void CancelCutTreeTask()
    {
        cutTreeTip.SetActive(false);
        building.AddActionType(building, ActionType.CutTree);
        building.RemoveActionType(building, ActionType.CancelCutTree);
        TaskManager.instance.RemoveTask(cutTreeTask);
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position - new Vector3(spawnItemWidthRange / 2, 0, 0), transform.position + new Vector3(spawnItemWidthRange / 2, 0, 0));
        Gizmos.DrawLine(transform.position - new Vector3(0, spawnItemHeightRange / 2, 0), transform.position + new Vector3(0, spawnItemHeightRange / 2, 0));
    }
}
