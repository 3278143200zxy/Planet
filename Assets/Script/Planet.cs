using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct PolarCoord
{
    public float r;
    public float a;
    public PolarCoord(float r, float a)
    {
        this.r = r;
        this.a = a;
    }
}
class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new List<(T item, int priority)>();


    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
        elements.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public T Dequeue()
    {
        var item = elements[0].item;
        elements.RemoveAt(0);
        return item;
    }

    public bool Contains(T item) => elements.Any(e => EqualityComparer<T>.Default.Equals(e.item, item));
    public int Count => elements.Count;
}

public class Planet : MonoBehaviour
{
    public Cell[,] grid = new Cell[200, 2000];

    public Cell cellPrefab;
    public int innerRadius, outerRadius, surfaceRadius;
    public float cellSizeCorrection;
    public float cellHeight, cellIntervalAngle;

    public Building woodBuildingPrefab;
    public float woodPossibility;
    public int circleCellNumber
    {
        get { return Mathf.RoundToInt(360f / cellIntervalAngle); }
    }

    private void Awake()
    {
        GenerateMap();
    }
    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < 200; i++) for (int j = 0; j < 2000; j++) grid[i, j] = new Cell();
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void GenerateMap()
    {
        for (int i = innerRadius; i < outerRadius; i++)
        {
            for (int j = 0; j < Mathf.RoundToInt(360f / cellIntervalAngle); j++)
            {
                Vector3 dir = Vector2.right;
                dir = Quaternion.Euler(0, 0, cellIntervalAngle * j) * dir;
                Cell cell = Instantiate(cellPrefab, transform.position + dir.normalized * i * cellHeight, Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                cell.transform.localScale = new Vector3(1 + (i - surfaceRadius) * cellSizeCorrection, 1, 1);
                grid[i, j] = cell;// new Cell(i, j);
                cell.SetCell(this, i, j);
                //if (i == surfaceRadius + 1) cell.AddCircleNeighbours();
                //cell.SetCanReach(true);
            }
        }
        for (int i = 0; i < Mathf.RoundToInt(360f / cellIntervalAngle); i++)
        {
            int tempIdx = surfaceRadius + 1;
            Vector3 dir = Vector2.right;
            dir = Quaternion.Euler(0, 0, cellIntervalAngle * i) * dir;

            if (UnityEngine.Random.Range(0f, 1f) < woodPossibility)
            {
                Building woodBuilding = Instantiate(woodBuildingPrefab, transform.position + dir.normalized * tempIdx * cellHeight, Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));

                Cell cell1 = grid[tempIdx, i], cell2 = grid[tempIdx + 1, i];
                cell1.building = woodBuilding; cell2.building = woodBuilding;
                woodBuilding.AddCell(cell1); woodBuilding.AddCell(cell2);


            }
            //if (i == surfaceRadius + 1) cell.AddCircleNeighbours();
            //cell.SetCanReach(true);
        }
        //Debug.Log((int)(360f / cellIntervalAngle) + " " + (360f / cellIntervalAngle));
    }
    public List<Cell> FindPath(Cell start, Cell end)
    {
        var openSet = new PriorityQueue<Cell>();
        var cameFrom = new Dictionary<Cell, Cell>();
        var gScore = new Dictionary<Cell, int>();
        var fScore = new Dictionary<Cell, int>();
        var closedSet = new HashSet<Cell>();

        int Heuristic(Cell a, Cell b) => (int)(Mathf.Abs(a.transform.position.x - b.transform.position.x) + Mathf.Abs(a.transform.position.y - b.transform.position.y));


        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == end)
            {
                var path = new List<Cell>();
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            closedSet.Add(current);

            foreach (var neighbor in current.GetNeighbours())
            {
                if (/*!neighbor.canStand || */closedSet.Contains(neighbor))
                    continue;

                int currentG = gScore.ContainsKey(current) ? gScore[current] : int.MaxValue;
                int neighborG = gScore.ContainsKey(neighbor) ? gScore[neighbor] : int.MaxValue;
                int tentativeG = currentG + 1;

                if (tentativeG < neighborG)

                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, end);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return null; // 无路径
    }

    public Cell PosToCell(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        if (dir.magnitude < (innerRadius - 1f / 2f) * cellHeight && dir.magnitude > (outerRadius - 1f / 2f) * cellHeight) return null;
        float angle = Vector2.SignedAngle(Vector2.right, dir) + cellIntervalAngle / 2;
        if (angle < 0) angle += 360f;
        //angle += 360f;
        float distance = dir.magnitude + cellHeight / 2;
        //Debug.Log((int)(distance / cellHeight) + " " + (int)(angle / cellIntervalAngle));
        return grid[(int)(distance / cellHeight), (int)(angle / cellIntervalAngle)];
    }
    public PolarCoord PosToPolarCoord(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, dir) + cellIntervalAngle / 2;
        if (angle < 0) angle += 360f;
        float distance = dir.magnitude + cellHeight / 2;
        return new PolarCoord((int)(distance / cellHeight), (int)(angle / cellIntervalAngle));
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + new Vector3(surfaceRadius * cellHeight, 0, 0), 0.2f);
    }
}
