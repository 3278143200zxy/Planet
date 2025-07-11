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
    private SortedList<float, Queue<T>> elements = new SortedList<float, Queue<T>>();

    // Enqueue an item with a priority
    public void Enqueue(T item, float priority)
    {
        if (!elements.ContainsKey(priority))
        {
            elements[priority] = new Queue<T>();
        }
        elements[priority].Enqueue(item);
    }

    // Dequeue the item with the highest priority (smallest key)
    public T Dequeue()
    {
        // Get the queue with the smallest priority key (highest priority)
        var firstPriority = elements.Keys[0];
        var queue = elements[firstPriority];
        var item = queue.Dequeue();

        // Remove the queue if it's empty
        if (queue.Count == 0)
        {
            elements.Remove(firstPriority);
        }
        return item;
    }

    // Check if the queue contains an item
    public bool Contains(T item)
    {
        foreach (var queue in elements.Values)
        {
            if (queue.Contains(item))
                return true;
        }
        return false;
    }

    // Update the priority of an item
    public bool UpdatePriority(T item, float newPriority)
    {
        // Remove the item from its current position
        foreach (var priority in elements.Keys)
        {
            var queue = elements[priority];
            if (queue.Contains(item))
            {
                queue = new Queue<T>(queue.Where(x => !EqualityComparer<T>.Default.Equals(x, item))); // Remove item
                elements[priority] = queue;

                // Enqueue the item with the new priority
                Enqueue(item, newPriority);
                return true;
            }
        }
        return false;
    }

    // Count of elements in the queue
    public int Count => elements.Values.Sum(queue => queue.Count);
}

public class Planet : MonoBehaviour
{
    public Cell[,] grid = new Cell[200, 2000];

    public Cell cellPrefab;
    public int innerRadius, outerRadius, surfaceRadius;
    public float cellSizeCorrection;
    public float cellHeight, cellIntervalAngle;

    public List<Item> items = new List<Item>();

    public float gravity;

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

                woodBuilding.SetBuilding(grid[tempIdx, i]);


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
        var gScore = new Dictionary<Cell, float>();
        var fScore = new Dictionary<Cell, float>();
        var closedSet = new HashSet<Cell>();

        int Heuristic(Cell a, Cell b) =>
            (int)(Mathf.Abs(a.transform.position.x - b.transform.position.x) +
                  Mathf.Abs(a.transform.position.y - b.transform.position.y));

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == end)
            {
                // 计算路径总代价
                float pathLength = gScore[end]; // 总路径代价
                var path = new List<Cell> { current };
                while (cameFrom.TryGetValue(current, out var prev))
                {
                    current = prev;
                    path.Add(current);
                }
                path.Reverse();
                return path;
            }

            closedSet.Add(current);

            foreach (var neighbor in current.GetNeighbours())
            {
                if (/*!neighbor.canStand || */closedSet.Contains(neighbor))
                    continue;

                float currentG = gScore.TryGetValue(current, out var g) ? g : float.MaxValue;
                float neighborG = gScore.TryGetValue(neighbor, out var ng) ? ng : float.MaxValue;

                float moveCost = current.GetMoveCostTo(neighbor);
                float tentativeG = currentG + moveCost;

                if (tentativeG < neighborG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, end);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    else
                        openSet.UpdatePriority(neighbor, fScore[neighbor]); // 如果支持
                }
            }
        }

        return null; // 无路径
    }
    public List<Cell> FindPathWithMaxDistance(Cell start, Cell end, float maxDistance)
    {
        var openSet = new PriorityQueue<Cell>();
        var cameFrom = new Dictionary<Cell, Cell>();
        var gScore = new Dictionary<Cell, float>();
        var fScore = new Dictionary<Cell, float>();

        float Heuristic(Cell a, Cell b) =>
            Mathf.Abs(a.transform.position.x - b.transform.position.x) +
            Mathf.Abs(a.transform.position.y - b.transform.position.y);

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            // ✅ 超过最大距离，提前终止
            if (gScore[current] > maxDistance)
                return null;

            if (current == end)
            {
                float totalPathLength = gScore[end];
                if (totalPathLength > maxDistance)
                    return null;

                var path = new List<Cell> { current };
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Add(current);
                }
                path.Reverse();
                return path;
            }

            foreach (var neighbor in current.GetNeighbours())
            {
                if (!neighbor.canStand) continue;

                float tentativeG = gScore.TryGetValue(current, out var g) ? g : float.MaxValue;
                tentativeG += current.GetMoveCostTo(neighbor);

                if (tentativeG < (gScore.TryGetValue(neighbor, out var ng) ? ng : float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, end);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    else
                        openSet.UpdatePriority(neighbor, fScore[neighbor]);
                }
            }
        }

        return null; // 无法找到路径或超出最大距离
    }

    public float GetPathLength(List<Cell> path)
    {
        float totalLength = 0f;

        for (int i = 0; i < path.Count - 1; i++)
        {
            // 获取当前格子和下一个格子之间的距离
            totalLength += path[i].GetMoveCostTo(path[i + 1]);
        }

        return totalLength;
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
