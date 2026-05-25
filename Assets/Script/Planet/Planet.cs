using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct PolarCoord
{
    public float r;
    public float a;
    public float l;
    public PolarCoord(float r, float a, float l)
    {
        this.r = r;
        this.a = a;
        this.l = l;
    }
}


public class Planet : MonoBehaviour
{
    public Cell[,,] grid = new Cell[2000, 2000, 10];//[radius,angle,layer]

    public Cell cellPrefab;
    public int innerRadius, outerRadius, surfaceRadius;
    public float cellSizeCorrection;
    public float cellHeight, cellIntervalAngle;

    [Header("Territory")]
    public float noiseScale;
    public float noiseAmplitude;
    public float minSurfaceNoiseThreshold;
    public float maxSurfaceNoiseThreshold;
    public int maxHeight;
    public int maxDepth;


    public List<Item> items = new List<Item>();
    public List<WarehouseModule> warehouseModules = new List<WarehouseModule>();

    public List<RestPoint> restPoints = new List<RestPoint>();

    public float gravity;

    public Building woodBuildingPrefab;
    public float woodPossibility;
    public Building stoneBuildingPrefab;
    public float mineralThreshold;
    public float xMineralNoiseScale;
    public float yMineralNoiseScale;

    public UnityEvent<ItemType> ItemHitGroundEvent = new UnityEvent<ItemType>();

    [Header("Mountain")]
    public int mountainNumber;
    public int minMountainHeight;
    public int maxMountainHeight;
    public float risePossibility;
    public int maxRiseHeight;
    //public float riseParameterCorrection;

    public int currentLayer = 0;
    public List<Transform> layers = new List<Transform>();
    public Camera frontCamera;
    public Camera backCamera;
    public GameObject layerMask;

    public WaterModule waterModule;

    public int seed;

    public int circleCellNumber
    {
        get { return Mathf.RoundToInt(360f / cellIntervalAngle); }
    }

    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < 200; i++) for (int j = 0; j < 2000; j++) grid[i, j] = new Cell();
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
    }
    public static float Sum(int N, int s, float k, float d)
    {
        int M = N - s; // 项数差
        return (M + 1) * (k + d * M / 2f);
    }
    public float CellRadiusDistance(int i)
    {
        return Sum(i, surfaceRadius, cellHeight, cellSizeCorrection) + surfaceRadius * cellHeight;
    }
    public float CellHeight(int i)
    {
        return cellHeight + (i - surfaceRadius) * cellSizeCorrection;
    }
    private void GenerateMap()
    {
        UnityEngine.Random.InitState(seed);
        List<Cell> tempCells = new List<Cell>();
        for (int l = 0; l < 2; l++)
        {

            for (int i = innerRadius; i < outerRadius; i++)
            {
                for (int j = 0; j < Mathf.RoundToInt(360f / cellIntervalAngle); j++)
                {
                    Vector3 dir = Vector2.right;
                    dir = Quaternion.Euler(0, 0, cellIntervalAngle * j) * dir;
                    /*
                    Cell cell = Instantiate(cellPrefab, transform.position + dir.normalized * i * cellHeight, Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                    cell.transform.localScale = new Vector3(1 + (i - surfaceRadius) * cellSizeCorrection, 1, 1);
                    */
                    Cell cell = Instantiate(cellPrefab, transform.position + dir.normalized * CellRadiusDistance(i) + new Vector3(0, 0, l), Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                    cell.transform.localScale = Vector3.one * (1 + (i - surfaceRadius) * cellSizeCorrection);
                    grid[i, j, l] = cell;// new Cell(i, j);
                    cell.SetCell(this, i, j, l);
                    tempCells.Add(cell);
                    //if (i == surfaceRadius + 1) cell.AddCircleNeighbours();
                    //cell.SetCanReach(true);
                }
            }

        }
        for (int l = 0; l < 2; l++)
            foreach (var cell in tempCells)
            {
                cell.SetCellNeighbours();
                cell.transform.SetParent(layers[l]);
            }

        for (int l = 0; l < 2; l++)
        {
            int layer = LayerMask.NameToLayer(l.ToString());
            //instantiate stone
            /*
            for (int i = innerRadius; i < surfaceRadius; i++)
            {
                for (int j = 0; j < Mathf.RoundToInt(360f / cellIntervalAngle); j++)
                {
                    Vector3 dir = Vector2.right;
                    dir = Quaternion.Euler(0, 0, cellIntervalAngle * j) * dir;

                    Building stoneBuilding = Instantiate(stoneBuildingPrefab, transform.position + dir.normalized * CellRadiusDistance(i) + new Vector3(0, 0, l), Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                    stoneBuilding.SetBuilding(grid[i, j, l]);
                    stoneBuilding.transform.SetParent(layers[l]);
                    stoneBuilding.ChangeLayer(stoneBuilding.gameObject, layer);
                }
            }
            */
            //instantiate mountain
            /*
            for (int m = 0; m < mountainNumber; m++)
            {
                int currentAngleIdx = UnityEngine.Random.Range(0, Mathf.RoundToInt(360f / cellIntervalAngle));
                bool isRising = true;
                int h = 0;
                int maxHeight = UnityEngine.Random.Range(minMountainHeight, maxMountainHeight);
                while (isRising || h != 0)
                {
                    for (int k = 0; k < h; k++)
                    {
                        Vector3 dir = Vector2.right;
                        dir = Quaternion.Euler(0, 0, cellIntervalAngle * currentAngleIdx) * dir;
                        for (int i = surfaceRadius; i < surfaceRadius + h; i++)
                        {
                            if (grid[i, currentAngleIdx, l].building != null) continue;

                            Building stoneBuilding = Instantiate(stoneBuildingPrefab, transform.position + dir.normalized * CellRadiusDistance(i) + new Vector3(0, 0, l), Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                            stoneBuilding.SetBuilding(grid[i, currentAngleIdx, l]);
                            stoneBuilding.transform.SetParent(layers[l]);
                            stoneBuilding.spriteRenderers[0].gameObject.layer = layer;
                        }
                    }
                    int heightOffset = UnityEngine.Random.Range(0, maxRiseHeight);
                    if (isRising)
                    {
                        h += heightOffset;
                        h = Mathf.Min(h, maxHeight);
                        if (h == maxHeight) isRising = false;
                    }
                    else
                    {
                        h -= heightOffset;
                        h = Math.Max(h, 0);
                        if (h == 0) break;
                    }
                    currentAngleIdx += 1;
                    int parameter = Mathf.RoundToInt(360f / cellIntervalAngle);
                    currentAngleIdx = (currentAngleIdx + parameter) % parameter;
                }

            }
            */
            //instantiate soil
            /*
            for (int j = 0; j < Mathf.RoundToInt(360f / cellIntervalAngle); j++)
            {
                Vector3 dir = Vector2.right;
                if (grid[surfaceRadius, j, l].building != null) continue;
                dir = Quaternion.Euler(0, 0, cellIntervalAngle * j) * dir;

                Building soilBuilding = Instantiate(soilBuildingPrefab, transform.position + dir.normalized * CellRadiusDistance(surfaceRadius) + new Vector3(0, 0, l), Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                soilBuilding.SetBuilding(grid[surfaceRadius, j, l]);
                soilBuilding.transform.SetParent(layers[l]);
                soilBuilding.ChangeLayer(soilBuilding.gameObject, layer);
                //soilBuilding.spriteRenderers[0].gameObject.layer = layer;
            }
            */
            //instantiate tree


            float offset = UnityEngine.Random.Range(0, 10000f);
            Dictionary<int, int> angleIdxToMaxRadiusIdx = new Dictionary<int, int>();
            for (int a = 0; a < circleCellNumber; a++)
            {
                float h = MathEx.CircularNoise(a, circleCellNumber, noiseScale, offset + 10 * l);
                float h1 = MathEx.CircularNoise(a, circleCellNumber, noiseScale, offset + 10 * l - 1000);

                h = (h * 2 - 1) * noiseAmplitude;
                if (h < 0) h /= 1.2f;
                //h = Mathf.Clamp01((h + 1) / 2);

                h1 = (h1 * 2 - 1) * noiseAmplitude;
                if (h1 < 0) h1 /= 1.2f;
                //h1 = Mathf.Clamp01((h1 + 1) / 2);

                //h += 0.1f * h1 + h;
                h = Mathf.Clamp01((h + 1) / 2);
                //Debug.Log(h);

                int r = surfaceRadius;
                float parameter1 = 0;
                int parameter2 = 0;
                if (h > maxSurfaceNoiseThreshold)
                {
                    parameter1 = h - maxSurfaceNoiseThreshold;
                    parameter2 = maxHeight * 2;
                }
                else if (h < minSurfaceNoiseThreshold)
                {
                    parameter1 = h - minSurfaceNoiseThreshold;
                    parameter2 = maxDepth * 2;
                }
                r += Mathf.RoundToInt(parameter1 * parameter2);
                angleIdxToMaxRadiusIdx.Add(a, r);

                Vector3 dir = Vector2.right;
                dir = Quaternion.Euler(0, 0, cellIntervalAngle * a) * dir;

                Building stoneBuilding = Instantiate(stoneBuildingPrefab, transform.position + dir.normalized * CellRadiusDistance(r) + new Vector3(0, 0, l), Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                stoneBuilding.SetBuilding(grid[r, a, l]);
                stoneBuilding.transform.SetParent(layers[l]);
                stoneBuilding.ChangeLayer(stoneBuilding.gameObject, layer);
                Stone stone = stoneBuilding.GetComponent<Stone>();

                if (r != surfaceRadius) stone.SetMineralType(MineralType.Stone);
                else stone.SetMineralType(MineralType.Soil);

            }

            for (int a = 0; a < circleCellNumber; a++)
            {
                for (int r = innerRadius; r < angleIdxToMaxRadiusIdx[a]; r++)
                {
                    Vector3 dir = Vector2.right;
                    dir = Quaternion.Euler(0, 0, cellIntervalAngle * a) * dir;

                    Building stoneBuilding = Instantiate(stoneBuildingPrefab, transform.position + dir.normalized * CellRadiusDistance(r) + new Vector3(0, 0, l), Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                    stoneBuilding.SetBuilding(grid[r, a, l]);
                    stoneBuilding.transform.SetParent(layers[l]);
                    stoneBuilding.ChangeLayer(stoneBuilding.gameObject, layer);

                    float possibility = MathEx.SimpleNoise(a * xMineralNoiseScale, r * yMineralNoiseScale);
                    Stone stone = stoneBuilding.GetComponent<Stone>();
                    if (possibility > mineralThreshold) stone.SetMineralType(MineralType.Iron);
                    else stone.SetMineralType(MineralType.Stone);

                    stone.totalStoneMineProgress += (r - innerRadius) / 2;
                }
                if (angleIdxToMaxRadiusIdx[a] < surfaceRadius)
                {
                    for (int r = angleIdxToMaxRadiusIdx[a] + 1; r <= surfaceRadius; r++) grid[r, a, l].water.SetWaterAmount(1f);
                }
            }

            for (int a = 0; a < circleCellNumber; a++)
            {
                if (angleIdxToMaxRadiusIdx[a] != surfaceRadius) continue;
                int tempIdx = surfaceRadius + 1;
                //if (grid[tempIdx, j, l].building != null) continue;
                Vector3 dir = Vector2.right;
                dir = Quaternion.Euler(0, 0, cellIntervalAngle * a) * dir;
                if (UnityEngine.Random.Range(0f, 1f) < woodPossibility)
                {
                    Building woodBuilding = Instantiate(woodBuildingPrefab, transform.position + dir.normalized * CellRadiusDistance(tempIdx) + new Vector3(0, 0, l), Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg));
                    woodBuilding.SetBuilding(grid[tempIdx, a, l]);
                    woodBuilding.transform.SetParent(layers[l]);
                    woodBuilding.ChangeLayer(woodBuilding.gameObject, layer);
                    //woodBuilding.spriteRenderers[0].gameObject.layer = layer;

                }
            }
        }

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

            // 1. 先检查是否到达终点（且距离在限制内才返回）
            if (current == end)
            {
                if (gScore[current] <= maxDistance)
                {
                    var path = new List<Cell> { current };
                    while (cameFrom.ContainsKey(current))
                    {
                        current = cameFrom[current];
                        path.Add(current);
                    }
                    path.Reverse();
                    return path;
                }
                // 如果终点超出距离，继续搜索其他节点（不返回）
                continue;
            }

            // 2. 如果当前节点的路径长度已经 >= maxDistance，则跳过它，不扩展邻居
            if (gScore[current] >= maxDistance)
                continue;   // 原代码为 return null，现已改为跳过该节点

            // 3. 扩展邻居（原逻辑不变）
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

        return null; // 无法找到满足距离限制的路径
    }

    public float GetPathLength(List<Cell> path)
    {
        if (path == null || path.Count == 0) return float.MaxValue;
        float totalLength = 0f;

        for (int i = 0; i < path.Count - 1; i++) totalLength += path[i].GetMoveCostTo(path[i + 1]);
        return totalLength;
    }
    public void ItemHitGround(Item item)
    {
        items.Add(item);
        ItemHitGroundEvent.Invoke(item.itemType);
    }
    public void ItemHitGround(ItemType itemType)
    {
        ItemHitGroundEvent.Invoke(itemType);
    }
    public Cell IndexToCell(int id)
    {
        int sizeA = circleCellNumber + 1;
        int r = id / (2 * sizeA);
        int a = (id / 2) % sizeA;
        int l = id % 2;
        return grid[r, a, l];
    }
    public Cell PosToCell(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        if (dir.magnitude < (innerRadius - 1f / 2f) * cellHeight && dir.magnitude > (outerRadius - 1f / 2f) * cellHeight) return null;
        float angle = Vector2.SignedAngle(Vector2.right, dir) + cellIntervalAngle / 2;
        if (angle < 0) angle += 360f;
        //angle += 360f;
        int ri = CellRadiusFromDistance(Vector2.Distance(transform.position, pos));
        int li = Mathf.RoundToInt(pos.z);
        //Debug.Log((int)(distance / cellHeight) + " " + (int)(angle / cellIntervalAngle));
        return grid[ri, (int)(angle / cellIntervalAngle), li];
    }
    public PolarCoord PosToPolarCoord(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, dir) + cellIntervalAngle / 2;
        if (angle < 0) angle += 360f;
        int ri = CellRadiusFromDistance(Vector2.Distance(transform.position, pos));
        int li = Mathf.RoundToInt(pos.z);
        return new PolarCoord(ri, (int)(angle / cellIntervalAngle), li);
    }
    public void ChangeLayer()
    {
        backCamera.cullingMask = LayerMask.GetMask(currentLayer.ToString(), "UI");
        layerMask.layer = LayerMask.NameToLayer(currentLayer.ToString());
        currentLayer = Mathf.Abs(currentLayer - 1);
        frontCamera.cullingMask = LayerMask.GetMask(currentLayer.ToString(), "UI");


    }
    public int CellRadiusFromDistance(float S)
    {
        S -= surfaceRadius * cellHeight;

        float s = surfaceRadius;
        float k = cellHeight;
        float d = cellSizeCorrection;

        float M;

        if (Mathf.Abs(d) < 1e-6f)
        {
            M = S / k - 1f;
        }
        else
        {
            float a = d;
            float b = d + 2f * k;
            float c = 2f * k - 2f * S;

            float disc = b * b - 4f * a * c;
            float sqrtDisc = Mathf.Sqrt(Mathf.Max(disc, 0f)); // 防止负数开根号

            float M1 = (-b + sqrtDisc) / (2f * a);
            float M2 = (-b - sqrtDisc) / (2f * a);

            M = Mathf.Max(M1, M2);
        }

        float nReal = s + M;
        int nInt = Mathf.RoundToInt(nReal);

        return Mathf.Max(nInt, 1); // 保证返回值至少是1
    }
    public static float CalcSum_Float(float N, float s, float k, float d)
    {
        float M = N - s;
        return (M + 1f) * (k + d * M * 0.5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + new Vector3(surfaceRadius * cellHeight, 0, 0), 0.2f);
    }
}
