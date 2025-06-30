using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private Building building;
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
        foreach (Cell cell in building.cells)
        {
            cell.AddAboveNeighbour();
            Cell aboveCell = cell.neighbourCellNodes[0].cell;
            if (aboveCell != null)
            {
                aboveCell.SetCanStand(true);
                aboveCell.AddBelowNeighbour();
                Cell aboveLeftCell = aboveCell.neighbourCellNodes[2].cell;
                if (aboveLeftCell.canStand)
                {
                    aboveCell.AddLeftNeighbour();
                    aboveLeftCell.AddRightNeighbour();
                }
                Cell aboveRightCell = aboveCell.neighbourCellNodes[3].cell;
                if (aboveRightCell.canStand)
                {
                    aboveCell.AddRightNeighbour();
                    aboveRightCell.AddLeftNeighbour();
                }
            }
        }
    }
}
