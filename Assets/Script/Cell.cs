using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NeighbourCellNode
{
    public Cell cell;
    public int number;
    public float distance;
    public NeighbourCellNode(Cell c)
    {
        cell = c;
        number = 0;
        distance = 0;
    }
    public void AddCellNumber()
    {
        number++;
    }
    public void SetDistance(float d)
    {
        distance = d;
    }
}
public class Cell : MonoBehaviour
{
    public Planet planet;
    public int radiusIdx, angleIdx;

    public Building building;
    public PlacedObject placedObject;
    public bool canPlace
    {
        get { return building == null && placedObject == null; }
    }
    public bool canStand
    {
        get { return standNumber > 0; }
    }
    public int standNumber;
    private int[] rd = new int[4] { 1, -1, 0, 0 };
    private int[] ad = new int[4] { 0, 0, 1, -1 };
    public NeighbourCellNode[] neighbourCellNodes = new NeighbourCellNode[4];


    public GameObject noPlacingSign;

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            int ri = radiusIdx + rd[i], ai = angleIdx + ad[i];
            if (ri >= planet.outerRadius || ri < planet.innerRadius)
            {
                neighbourCellNodes[i] = new NeighbourCellNode(null);
            }
            if (ai >= planet.circleCellNumber) ai -= planet.circleCellNumber;
            if (ai < 0) ai += planet.circleCellNumber;
            neighbourCellNodes[i] = new NeighbourCellNode(planet.grid[ri, ai]);
        }
        if (radiusIdx == planet.surfaceRadius) standNumber++;
        if (radiusIdx == planet.surfaceRadius + 1)
        {
            AddCircleNeighbours(1);
        }

    }
    public void SetCell(Planet p, int ri, int ai)
    {
        planet = p;
        radiusIdx = ri;
        angleIdx = ai;
    }
    public List<Cell> GetNeighbours()
    {
        List<Cell> temp = new List<Cell>();

        for (int i = 0; i < 4; i++)
        {
            if (neighbourCellNodes[i].number > 0 && neighbourCellNodes[i].cell != null) temp.Add(neighbourCellNodes[i].cell);
        }
        return temp;
    }
    public float GetMoveCostTo(Cell neighbor)
    {
        for (int i = 0; i < 4; i++)
        {
            if (neighbourCellNodes[i].cell == neighbor)
            {
                return neighbourCellNodes[i].distance; // 返回邻居的移动代价
            }
        }
        return float.MaxValue; // 如果没有找到邻居，返回一个非常大的值，表示无法到达
    }
    public void AddCircleNeighbours(float d)
    {
        AddRightNeighbour(d);
        AddLeftNeighbour(d);
    }
    public void AddAboveNeighbour(float d)
    {
        neighbourCellNodes[0].AddCellNumber();
        neighbourCellNodes[0].SetDistance(d);
    }
    public void AddBelowNeighbour(float d)
    {
        neighbourCellNodes[1].AddCellNumber();
        neighbourCellNodes[1].SetDistance(d);
    }
    public void AddLeftNeighbour(float d)
    {
        neighbourCellNodes[2].AddCellNumber();
        neighbourCellNodes[2].SetDistance(d);
    }
    public void AddRightNeighbour(float d)
    {
        neighbourCellNodes[3].AddCellNumber();
        neighbourCellNodes[3].SetDistance(d);
    }
}
