using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public enum BuildingType
{
    Tree,
    Ladder,
    Campfire,
}
public class Building : BaseUnit
{
    [Header("Building")]
    public BuildingType buildingType;

    public Cell cell;
    public List<Dot> dots = new List<Dot>();
    public List<Dot> standDots = new List<Dot>();
    public UnityEvent SetBuildingEvent = new UnityEvent();

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
    }
    public override void Start()
    {
        base.Start();

        //QtreeManager.instance.AddBaseUnit(this);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    public virtual void SetBuilding(Cell c)
    {
        cell = c;
        foreach (Dot d in dots)
        {
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = -d.x + cell.angleIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx];
                processingCell.building = this;
            }
        }
        foreach (Dot d in standDots)
        {
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = -d.x + cell.angleIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx];
                processingCell.AddStandNumber(1);

            }
        }
        SetBuildingEvent.Invoke();
    }
    public override void DestoryBaseUnit()
    {
        foreach (Dot d in dots)
        {
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = -d.x + cell.angleIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx];
                processingCell.building = null;
            }
        }
        foreach (Dot d in standDots)
        {
            int radiusIdx = d.y + cell.radiusIdx, angleIdx = -d.x + cell.angleIdx;
            if (radiusIdx >= cell.planet.innerRadius && radiusIdx < cell.planet.outerRadius)
            {
                int temp = Mathf.RoundToInt(360f / cell.planet.cellIntervalAngle);
                if (angleIdx < 0) angleIdx += temp;
                if (angleIdx >= temp) angleIdx -= temp;
                Cell processingCell = cell.planet.grid[radiusIdx, angleIdx];
                processingCell.AddStandNumber(-1);
            }
        }
        base.DestoryBaseUnit();
    }
}
