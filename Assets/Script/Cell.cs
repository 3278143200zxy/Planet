using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NeighbourCellNode
{
    public Cell cell;
    public int number;
    public NeighbourCellNode(Cell c)
    {
        cell = c;
        number = 0;
    }
    public void AddCellNumber()
    {
        number++;
    }
}
public class Cell : MonoBehaviour
{
    public Planet planet;
    public int radiusIdx, angleIdx;

    public Building building;
    public bool canStand = false;
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
        if (radiusIdx == planet.surfaceRadius + 1)
        {
            AddCircleNeighbours();
            SetCanStand(true);
        }

    }
    public void SetCell(Planet p, int ri, int ai)
    {
        planet = p;
        radiusIdx = ri;
        angleIdx = ai;
    }
    public void SetCanStand(bool cs)
    {
        canStand = cs;
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
    public void AddCircleNeighbours()
    {
        neighbourCellNodes[2].AddCellNumber();
        neighbourCellNodes[3].AddCellNumber();
    }
    public void AddAboveNeighbour()
    {
        neighbourCellNodes[0].AddCellNumber();
    }
    public void AddBelowNeighbour()
    {
        neighbourCellNodes[1].AddCellNumber();
    }public void AddLeftNeighbour()
    {
        neighbourCellNodes[2].AddCellNumber();
    }
    public void AddRightNeighbour()
    {
        neighbourCellNodes[3].AddCellNumber();
    }
}
