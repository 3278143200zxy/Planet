using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sapling : MonoBehaviour
{
    public Building building;
    private Planet planet;

    public Wood treePrefab;

    public float growthProgress;
    public float growthRate;

    public float minGrowthThreshold, maxGrowthThreshold;
    public float growthThreshold;

    public UnityEvent<Sapling, Wood> OnGrowEvent = new UnityEvent<Sapling, Wood>();
    private void Awake()
    {
        planet = building.planet;

        growthThreshold = Random.Range(minGrowthThreshold, maxGrowthThreshold);
    }
    void Update()
    {
        if ((building.cell.angleIdx - planet.sun.startAngleIdx) % planet.circleCellNumber <= (planet.sun.endAngleIdx - planet.sun.startAngleIdx) % planet.circleCellNumber && building.cell.neighbourCellNodes[0].cell.canPlace)
        {
            growthProgress += growthRate * TimeManager.deltaTime;

            if (growthProgress > growthThreshold)
            {

                building.ClearState();

                Wood wood = Instantiate(treePrefab, transform.position, transform.rotation);
                wood.building.SetBuilding(building.cell);
                wood.building.ChangeLayer(wood.gameObject, gameObject.layer);

                OnGrowEvent.Invoke(this, wood);

                building.OnlyDestory();
            }
        }
    }
}
