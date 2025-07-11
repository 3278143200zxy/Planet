using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private Building building;

    public float walkSpeed;
    public float climbSpeed;
    private void Awake()
    {
        building = GetComponent<Building>();
        building.SetBuildingEvent.AddListener(SetBuilding);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetBuilding()

    {
        foreach (Dot d in building.dots)
        {
            int radiusIdx = d.y + building.cell.radiusIdx, angleIdx = d.x + building.cell.angleIdx;
            if (radiusIdx >= building.cell.planet.innerRadius && radiusIdx < building.cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / building.cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = building.cell.planet.grid[radiusIdx, angleIdx];
                processingCell.AddAboveNeighbour(climbSpeed);
                Cell aboveCell = processingCell.neighbourCellNodes[0].cell;

                if (aboveCell != null)
                {
                    aboveCell.standNumber++;
                    aboveCell.AddBelowNeighbour(climbSpeed);
                    Cell aboveLeftCell = aboveCell.neighbourCellNodes[2].cell;
                    if (aboveLeftCell.canStand)
                    {
                        aboveCell.AddLeftNeighbour(walkSpeed);
                        aboveLeftCell.AddRightNeighbour(walkSpeed);
                    }
                    Cell aboveRightCell = aboveCell.neighbourCellNodes[3].cell;
                    if (aboveRightCell.canStand)
                    {
                        aboveCell.AddRightNeighbour(walkSpeed);
                        aboveRightCell.AddLeftNeighbour(walkSpeed);
                    }
                }
            }
        }

    }
}
