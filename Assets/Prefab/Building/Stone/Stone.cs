using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MineralType
{
    Stone,
    Iron,
    Soil,
}
[Serializable]
public class MineralNode
{
    public Sprite sprite;
    public ItemType itemType;
}
[Serializable]
public class MineralDictionary : SerializableDictionary<MineralType, MineralNode> { }
public class Stone : MonoBehaviour
{
    public Building building;

    public SpriteRenderer spriteRenderer;
    public MineralDictionary mineralDictionary = new MineralDictionary();
    public MineralType mineralType;

    public GameObject mineStoneTip;
    public GameObject fog;

    public Task mineStoneTask;

    [HideInInspector]
    public float stoneMineProgress;
    public float totalStoneMineProgress;
    public Slider stoneMiningProgressSlider;
    public GameObject sliderTickMarkPrefab;

    public int minStoneItemNumber;
    public int maxStoneItemNumber;
    public float spawnItemHeight;
    public float spawnItemRange;

    void Start()
    {
        stoneMineProgress = totalStoneMineProgress;

        building.actionTypeToEvent[ActionType.MineStone].AddListener(StartMineStoneTask);
        building.actionTypeToEvent[ActionType.CancelMineStone].AddListener(CancelMineStoneTask);

        //CheckFog();
    }

    public void MineStone(float p)
    {
        stoneMineProgress -= p;
        //if (!stoneMiningProgressSlider.gameObject.activeInHierarchy) stoneMiningProgressSlider.gameObject.SetActive(true);
        //stoneMiningProgressSlider.value = stoneMineProgress / totalStoneMineProgress;
        if ((stoneMineProgress) <= 0f) OnMinedOut();
    }
    public void OnMinedOut()
    {
        int stoneItemNumber = UnityEngine.Random.Range(minStoneItemNumber, maxStoneItemNumber);
        for (int i = 0; i < stoneItemNumber; i++)
        {
            Item stoneItem = PoolManager.instance.InstantiateItem(mineralDictionary[mineralType].itemType);
            stoneItem.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(-spawnItemRange / 2, spawnItemRange / 2), spawnItemHeight, 0);

            stoneItem.ChangeLayer(stoneItem.gameObject, building.spriteRenderers[0].gameObject.layer);
        }


        building.ClearState();
        CancelMineStoneTask();
        /*
        for (int i = 0; i < 6; i++)
        {
            Cell neighbourCell = building.cell.neighbourCellNodes[i].cell;
            if (neighbourCell != null && neighbourCell.building != null && neighbourCell.building.isBlock)
            {
                Stone s = neighbourCell.building.GetComponent<Stone>();
                s.CheckFog();
            }
        }
        */
        building.OnlyDestory();
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
    public void SetMineralType(MineralType mt)
    {
        mineralType = mt;
        spriteRenderer.sprite = mineralDictionary[mineralType].sprite;
    }
    public void CheckFog()
    {
        for (int i = 0; i < 4; i++)
        {
            Cell neighbourCell = building.cell.neighbourCellNodes[i].cell;
            if (neighbourCell == null || !(neighbourCell.building != null && neighbourCell.building.isBlock))
            {
                fog.SetActive(false);
                return;
            }
        }
        for (int i = 5; i < 6; i++)
        {
            Cell neighbourCell = building.cell.neighbourCellNodes[i].cell;
            if (neighbourCell == null) continue;
            if (!(neighbourCell.building != null && neighbourCell.building.isBlock))
            {
                fog.SetActive(false);
                return;
            }
        }
        fog.SetActive(true);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 centerPos = transform.position + new Vector3(0, spawnItemHeight, 0);
        Gizmos.DrawLine(centerPos + new Vector3(spawnItemRange / 2, 0, 0), centerPos - new Vector3(spawnItemRange / 2, 0, 0));
    }
}
