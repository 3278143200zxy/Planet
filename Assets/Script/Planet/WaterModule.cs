using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water
{
    public WaterModule waterModule;
    public Cell cell;
    [Range(0, 1)] public float waterAmount = 0;
    public ObjectSlider waterSlider;

    public Water(Cell _cell)
    {
        waterModule = _cell.planet.waterModule;
        cell = _cell;
        waterSlider = cell.waterSlider;
        waterSlider.fillRect.layer = LayerMask.NameToLayer(cell.layerIdx.ToString());
        SetWaterAmount(0);
    }
    public void SetWaterAmount(float wa)
    {
        if (waterAmount > 0 && wa <= 0) waterModule.waterCells[cell.radiusIdx].Remove(cell);
        else if (waterAmount == 0 && wa > 0) waterModule.waterCells[cell.radiusIdx].Add(cell);

        waterAmount = wa;
        waterAmount = Mathf.Clamp01(waterAmount);

        //if (waterAmount == 0 && cell.planet.waterModule.waterCells.Contains(cell)) cell.planet.waterModule.waterCells.Remove(cell);
        //if (waterAmount > 0 && !cell.planet.waterModule.waterCells.Contains(cell)) cell.planet.waterModule.waterCells.Add(cell);

        RefreshDisplay();
    }
    public void AddWaterAmout(float wa)
    {
        if (waterAmount > 0 && waterAmount + wa <= 0) waterModule.waterCells[cell.radiusIdx].Remove(cell);
        else if (waterAmount == 0 && wa > 0) waterModule.waterCells[cell.radiusIdx].Add(cell);

        waterAmount += wa;
        if (waterAmount > 1) cell.neighbourCellNodes[0].cell.water.AddWaterAmout(waterAmount - 1);
        waterAmount = Mathf.Clamp01(waterAmount);


        //if (waterAmount == 0 && cell.planet.waterModule.waterCells.Contains(cell)) cell.planet.waterModule.waterCells.Remove(cell);
        //if (waterAmount > 0 && !cell.planet.waterModule.waterCells.Contains(cell)) cell.planet.waterModule.waterCells.Add(cell);

        RefreshDisplay();

    }
    public void ExcuteFrame()
    {
        Sink();
    }
    public void Sink()
    {

        Cell belowCell = cell.neighbourCellNodes[1].cell;
        if (waterAmount == 0 || cell.neighbourCellNodes[1].cell == null || belowCell.canStand) return;
        float waterAmountDelta = 1f - belowCell.water.waterAmount;
        if (waterAmountDelta > 0f)
        {
            float tempWaterDelta = Mathf.Min(waterAmountDelta, waterAmount);
            belowCell.water.AddWaterAmout(tempWaterDelta);
            AddWaterAmout(-tempWaterDelta);
            //if (waterAmountDelta == 1) cell.planet.waterModule.waterCells[belowCell.radiusIdx].Add(belowCell);
            //if (waterAmount == 0) cell.planet.waterModule.planet.waterModule.waterCells[cell.radiusIdx].Remove(cell);
        }
    }
    public void RefreshDisplay()
    {
        waterSlider.SetValue(waterAmount);
    }
}
public class WaterModule : MonoBehaviour
{
    public Planet planet;

    public Dictionary<int, List<Cell>> waterCells = new Dictionary<int, List<Cell>>();

    private float frameTimer;
    public float frameInterval;

    public float overflowThreshold = 0.1f;

    private void Awake()
    {
        for (int i = planet.innerRadius; i < planet.outerRadius; i++) waterCells.Add(i, new List<Cell>());

    }
    private void Start()
    {
        //for (int l = 0; l < 2; l++)
        //{
        /*
        for (int r = planet.innerRadius; r < planet.outerRadius; r++)
        {
            for (int a = 100; a < 120; a++)
            {
                Cell cell = planet.grid[r, a, 0];
                if (cell.building != null && cell.building.isBlock) cell.water.SetWaterAmount(0);
                else
                {
                    cell.water.SetWaterAmount(0.8f);
                    //waterCells[r].Add(cell);
                }
            }
        }
        */
        //}
        //ExcuteFrame();
    }
    void Update()
    {
        frameTimer += TimeManager.deltaTime;
        if (frameTimer > frameInterval)
        {
            ExcuteFrame();
            frameTimer -= frameInterval;
        }
    }
    public void ExcuteFrame()
    {
        List<int> tempKeys = new List<int>(waterCells.Keys);
        tempKeys.Sort();
        foreach (var key in tempKeys)
        {
            List<Cell> tempWaterCells = new List<Cell>(waterCells[key]);
            //Dictionary<Cell, List<Cell>> processedCell = new Dictionary<Cell, List<Cell>>();
            UnionFind<Cell> unionFindCells = new UnionFind<Cell>();
            Dictionary<Cell, HashSet<Cell>> unionFindNeighbourEmptyCells = new Dictionary<Cell, HashSet<Cell>>();
            //Dictionary<Cell, float> unionFindTotalWaterAmount = new Dictionary<Cell, float>();
            foreach (var cell in tempWaterCells)
            {
                cell.water.Sink();
                if (cell.water.waterAmount != 0)
                {
                    //cell.DebugCoord();
                    //processedCell[cell] = new List<Cell>();
                    unionFindCells.Add(cell);
                    unionFindNeighbourEmptyCells[cell] = new HashSet<Cell>();
                    //unionFindTotalWaterAmount.Add(cell, cell.water.waterAmount);
                }
            }
            tempWaterCells = new List<Cell>(waterCells[key]);


            foreach (var cell in tempWaterCells)
            {
                for (int i = 2; i < 6; i++)
                {
                    Cell neighbourCell = cell.neighbourCellNodes[i].cell;
                    //if (!processedCell[cell].Contains(neighbourCell))
                    //{
                    if (neighbourCell != null && neighbourCell.water.waterAmount > 0)
                    {
                        //processedCell[neighbourCell].Add(cell);
                        unionFindCells.Union(cell, neighbourCell);
                        //processedCell[cell].Add(neighbourCell);
                        //unionFindTotalWaterAmount[cell] += neighbourCell.water.waterAmount;
                        //unionFindTotalWaterAmount[neighbourCell] = unionFindTotalWaterAmount[cell];
                    }
                    //cell.DebugCoord();
                    if (neighbourCell != null && neighbourCell.water.waterAmount == 0 && (neighbourCell.building == null || !neighbourCell.building.isBlock)) unionFindNeighbourEmptyCells[cell].Add(neighbourCell);
                    // }
                }
                unionFindNeighbourEmptyCells[unionFindCells.Find(cell)].UnionWith(unionFindNeighbourEmptyCells[cell]);
            }

            Dictionary<Cell, List<Cell>> unionFindGroups = new Dictionary<Cell, List<Cell>>(unionFindCells.GetGroups());
            foreach (var root in unionFindGroups.Keys)
            {
                float totalWaterAmount = 0;
                foreach (var cell in unionFindGroups[root]) totalWaterAmount += cell.water.waterAmount;
                //Debug.Log(totalWaterAmount + " " + Time.time);
                float averagedWaterAmount = totalWaterAmount / unionFindGroups[root].Count;
                bool isOverFlow = averagedWaterAmount > overflowThreshold;
                if (isOverFlow) averagedWaterAmount = totalWaterAmount / (unionFindGroups[root].Count + unionFindNeighbourEmptyCells[root].Count);
                foreach (var cell in unionFindGroups[root]) cell.water.SetWaterAmount(averagedWaterAmount);
                if (isOverFlow) foreach (var cell in unionFindNeighbourEmptyCells[root]) cell.water.SetWaterAmount(averagedWaterAmount);
            }
        }
        //Debug.Log("End");
    }
}
