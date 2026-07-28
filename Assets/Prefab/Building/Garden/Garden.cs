using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garden : MonoBehaviour
{
    public Building building;

    public WarehouseModule warehouseModule;

    public Sapling saplingPrefab;

    public List<Sapling> saplings = new List<Sapling>();
    public List<Wood> woods = new List<Wood>();

    public List<Cell> availableCells = new List<Cell>();

    private void Start()
    {
        warehouseModule.SetNeededItemTypes(new List<ItemType>() { ItemType.Wood });
        warehouseModule.AddItemEvent.AddListener(OnWarehouseAddItem);

        Cell cell = building.cell;
        foreach (Dot d in building.dots)
        {
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = -d.x + cell.angleIdx, layerIdx = -d.z + cell.layerIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;

                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx, layerIdx];
                availableCells.Add(processingCell);
            }
        }
    }
    public void Plant(Cell cell)
    {
        availableCells.Remove(cell);

        Sapling sapling = Instantiate(saplingPrefab, cell.position, cell.rotation);
        sapling.building.SetBuilding(cell);
        sapling.building.ChangeLayer(sapling.gameObject, gameObject.layer);

        saplings.Add(sapling);
        sapling.OnGrowEvent.AddListener(OnSaplingGrow);
    }
    public void OnSaplingGrow(Sapling s, Wood w)
    {
        saplings.Remove(s);
        woods.Add(w);
        w.OnCuttedDownEvent.AddListener(OnTreeCuttedDown);
    }
    public void OnTreeCuttedDown(Wood w)
    {
        woods.Remove(w);

        availableCells.Add(w.building.cell);
        if (availableCells.Count == 1) warehouseModule.SetNeededItemTypes(new List<ItemType>() { ItemType.Wood });
    }
    public void OnWarehouseAddItem(ItemType itt)
    {
        warehouseModule.RemoveItemsDirectly(new Dictionary<ItemType, int>() { { itt, 1 } });

        int cellIdx = Random.Range(0, availableCells.Count);
        Plant(availableCells[cellIdx]);

        if (availableCells.Count == 0) warehouseModule.SetNeededItemTypes(new List<ItemType>());
    }
}
