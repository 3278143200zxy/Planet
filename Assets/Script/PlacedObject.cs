using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public List<Dot> dots = new List<Dot>() { new Dot(0, 0) };

    public Building buildingPrefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetPlacedObject(Cell cell)
    {
        Building building = Instantiate(buildingPrefab, transform.position, transform.rotation);

        foreach (Dot d in dots)
        {
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = d.x + cell.angleIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx];
                processingCell.building = building;
                building.AddCell(processingCell);
            }
        }

        building.SetBuilding();
        Destroy(gameObject);
    }
}
