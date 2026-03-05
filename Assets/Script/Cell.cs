using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class NeighbourCellNode
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
    public void SubtractCellNumber()
    {
        number--;
    }
    public void SetDistance(float d)
    {
        distance = 1;
    }
}
public class Cell : MonoBehaviour
{
    public Planet planet;
    public int radiusIdx, angleIdx, layerIdx;

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
    private int[] ld = new int[2] { -1, 1 };

    private int[] ad2 = new int[4] { 1, 1, -1, -1 };
    private int[] rd2 = new int[4] { 1, -1, -1, 1 };
    public NeighbourCellNode[] neighbourCellNodes = new NeighbourCellNode[6];
    public NeighbourCellNode[] diagonalNeighbourCellNodes = new NeighbourCellNode[4];


    public GameObject noPlacingSign;
    public void SetCell(Planet p, int _ri, int _ai, int _li)
    {
        planet = p;
        radiusIdx = _ri;
        angleIdx = _ai;
        layerIdx = _li;
        neighbourCellNodes = new NeighbourCellNode[6];
    }
    //Find cell's neighbours
    public void SetCellNeighbours()
    {
        for (int i = 0; i < 4; i++)
        {
            int ri = radiusIdx + rd[i], ai = angleIdx + ad[i];
            if (ri >= planet.outerRadius || ri < planet.innerRadius)
            {
                neighbourCellNodes[i] = new NeighbourCellNode(null);
                continue;
            }
            if (ai >= planet.circleCellNumber) ai -= planet.circleCellNumber;
            if (ai < 0) ai += planet.circleCellNumber;
            neighbourCellNodes[i] = new NeighbourCellNode(planet.grid[ri, ai, layerIdx]);
        }
        for (int i = 0; i < 2; i++)
        {
            int li = layerIdx + ld[i];
            if (li < 0 || li > 1)
            {
                //Debug.Log("yes");
                neighbourCellNodes[i + 4] = new NeighbourCellNode(null);
                continue;
            }
            neighbourCellNodes[i + 4] = new NeighbourCellNode(planet.grid[radiusIdx, angleIdx, li]);
        }

        for (int i = 0; i < 4; i++)
        {
            int ri = radiusIdx + rd2[i], ai = angleIdx + ad2[i];
            if (ri >= planet.outerRadius || ri < planet.innerRadius)
            {
                diagonalNeighbourCellNodes[i] = new NeighbourCellNode(null);
                continue;
            }
            if (ai >= planet.circleCellNumber) ai -= planet.circleCellNumber;
            if (ai < 0) ai += planet.circleCellNumber;
            diagonalNeighbourCellNodes[i] = new NeighbourCellNode(planet.grid[ri, ai, layerIdx]);
        }

    }
    //FInd neighbours that can be reached from this cell
    public List<Cell> GetNeighbours()
    {
        List<Cell> temp = new List<Cell>();

        for (int i = 0; i < 6; i++)
        {
            NeighbourCellNode node = neighbourCellNodes[i];
            if (node.cell != null && node.number > 0 && node.cell.neighbourCellNodes[1].cell.canStand && (node.cell.building == null || !node.cell.building.isBlock)) temp.Add(node.cell);
        }
        //Debug.Log(angleIdx + " " + radiusIdx + " " + layerIdx + " " + "general");
        for (int i = 2; i < 4; i++)
        {
            NeighbourCellNode node = neighbourCellNodes[i];
            if (node.cell != null && node.cell.canStand)
            {
                NeighbourCellNode node2 = neighbourCellNodes[0];
                NeighbourCellNode node3 = node.cell.neighbourCellNodes[0];
                if (node2.cell != null && (node2.cell.building == null || !node2.cell.building.isBlock)
                    && node3.cell != null && (node3.cell.building == null || !node3.cell.building.isBlock)) temp.Add(node3.cell);
            }
        }
        for (int i = 2; i < 4; i++)
        {
            NeighbourCellNode node = neighbourCellNodes[i];
            NeighbourCellNode node2 = node.cell.neighbourCellNodes[1];
            NeighbourCellNode node3 = node2.cell.neighbourCellNodes[1];
            if (node3.cell != null && node3.cell.canStand)
            {
                if (node2.cell != null && (node2.cell.building == null || !node2.cell.building.isBlock)
                    && node.cell != null && (node.cell.building == null || !node.cell.building.isBlock)) temp.Add(node2.cell);
            }
        }
        //Debug.Log(temp.Count);
        return temp;
    }
    public float GetMoveCostTo(Cell neighbour)
    {
        for (int i = 0; i < 6; i++)
        {
            if (neighbourCellNodes[i].cell == neighbour)
            {
                return 1;
                //return neighbourCellNodes[i].distance;
                // 返回邻居的移动代价
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (diagonalNeighbourCellNodes[i].cell == neighbour)
            {
                return 1;
            }
        }
        return float.MaxValue; // 如果没有找到邻居，返回一个非常大的值，表示无法到达
    }
    public void AddStandNumber(int number)
    {
        if (standNumber == 0 && number > 0)
        {
            Cell aboveCell = neighbourCellNodes[0].cell;
            if (aboveCell != null)
            {
                aboveCell.AddCircleNeighbours(1);
                aboveCell.AddLayerNeighbours(1);
            }
        }
        standNumber += number;
        if (standNumber == 0 && number != 0)
        {
            Cell aboveCell = neighbourCellNodes[0].cell;
            if (aboveCell != null)
            {
                aboveCell.RemoveCircleNeighbours(0);
                aboveCell.RemoveLayerNeighbours(0);
            }
        }
    }
    public void AddCircleNeighbours(float d)
    {
        AddRightNeighbour(d);
        AddLeftNeighbour(d);
    }
    public void RemoveCircleNeighbours(float d)
    {
        RemoveRightNeighbour(d);
        RemoveLeftNeighbour(d);
    }
    public void AddLayerNeighbours(float d)
    {
        AddFrontNeighbour(d);
        AddBackNeighbour(d);
    }
    public void RemoveLayerNeighbours(float d)
    {
        RemoveFrontNeighbour(d);
        RemoveBackNeighbour(d);
    }
    public void AddAboveNeighbour(float d)
    {
        neighbourCellNodes[0].AddCellNumber();
        neighbourCellNodes[0].SetDistance(d);
    }
    public void RemoveAboveNeighbour(float d)
    {
        neighbourCellNodes[0].SubtractCellNumber();
    }
    public void AddBelowNeighbour(float d)
    {
        neighbourCellNodes[1].AddCellNumber();
        neighbourCellNodes[1].SetDistance(d);
    }
    public void RemoveBelowNeighbour(float d)
    {
        neighbourCellNodes[1].SubtractCellNumber();
    }
    public void AddLeftNeighbour(float d)
    {
        neighbourCellNodes[2].AddCellNumber();
        neighbourCellNodes[2].SetDistance(d);
    }
    public void RemoveLeftNeighbour(float d)
    {
        neighbourCellNodes[2].SubtractCellNumber();
    }
    public void AddRightNeighbour(float d)
    {
        neighbourCellNodes[3].AddCellNumber();
        neighbourCellNodes[3].SetDistance(d);
    }
    public void RemoveRightNeighbour(float d)
    {
        neighbourCellNodes[3].SubtractCellNumber();
    }
    public void AddFrontNeighbour(float d)
    {
        if (neighbourCellNodes[4].cell == null) return;
        neighbourCellNodes[4].AddCellNumber();
        neighbourCellNodes[4].SetDistance(d);
    }
    public void RemoveFrontNeighbour(float d)
    {
        if (neighbourCellNodes[4].cell == null) return;
        neighbourCellNodes[4].SubtractCellNumber();
    }
    public void AddBackNeighbour(float d)
    {
        if (neighbourCellNodes[5].cell == null) return;
        neighbourCellNodes[5].AddCellNumber();
        neighbourCellNodes[5].SetDistance(d);
    }
    public void RemoveBackNeighbour(float d)
    {
        if (neighbourCellNodes[5].cell == null) return;
        neighbourCellNodes[5].SubtractCellNumber();
    }
}
