using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stone : MonoBehaviour
{
    public Building building;

    public GameObject mineStoneTip;

    public Task mineStoneTask;

    public float stoneMiningProgress = 1f;
    public Slider stoneMiningProgressSlider;
    public GameObject sliderTickMarkPrefab;

    public int stoneItemNumber;
    public float spawnItemHeight;
    public float spawnItemRange;

    void Start()
    {
        building.actionTypeToEvent[ActionType.MineStone].AddListener(StartMineStoneTask);
        building.actionTypeToEvent[ActionType.CancelMineStone].AddListener(CancelMineStoneTask);
    }

    public void MineStone(float p)
    {
        stoneMiningProgress -= p;
        if (!stoneMiningProgressSlider.gameObject.activeInHierarchy) stoneMiningProgressSlider.gameObject.SetActive(true);
        stoneMiningProgressSlider.value = stoneMiningProgress / 1;
        if (stoneMiningProgress <= 0f) OnMinedOut();
    }
    public void OnMinedOut()
    {
        for (int i = 0; i < stoneItemNumber; i++)
        {
            Item stoneItem = PoolManager.instance.InstantiateItem(ItemType.Stone);
            stoneItem.transform.position = transform.position + new Vector3(Random.Range(-spawnItemRange / 2, spawnItemRange / 2), spawnItemHeight, 0);
        }

        CancelMineStoneTask();
        building.DestoryBaseUnit();
    }
    public void StartMineStoneTask()
    {
        mineStoneTip.SetActive(true);
        building.AddActionType(building, ActionType.CancelMineStone);
        building.RemoveActionType(building, ActionType.MineStone);
        mineStoneTask = new Task(TaskType.MineStone, new BaseUnit[] { building });
        TaskManager.instance.AddTask(mineStoneTask);

    }
    public void CancelMineStoneTask()
    {
        mineStoneTip.SetActive(false);
        building.AddActionType(building, ActionType.MineStone);
        building.RemoveActionType(building, ActionType.CancelMineStone);
        TaskManager.instance.RemoveTask(mineStoneTask);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 centerPos = transform.position + new Vector3(0, spawnItemHeight, 0);
        Gizmos.DrawLine(centerPos + new Vector3(spawnItemRange / 2, 0, 0), centerPos - new Vector3(spawnItemRange / 2, 0, 0));
    }
}
