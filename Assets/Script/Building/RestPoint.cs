using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestPoint : MonoBehaviour
{
    public Building building;

    public float energyRestoreBonus = 1f;
    public int maximumCapacity;

    public List<Creature> creatures = new List<Creature>();
    public void Awake()
    {
        building.SetBuildingEvent.AddListener(SetRestPoint);
        building.OnDestoryEvent.AddListener(OnDestoryFunction);
    }
    public void SetRestPoint()
    {
        building.planet.restPoints.Add(this);
    }
    public void OnDestoryFunction()
    {
        building.planet.restPoints.Remove(this);
    }
    public bool IsAvailable()
    {
        return creatures.Count <= maximumCapacity;
    }
    public void AddCreature(Creature c)
    {
        if (!creatures.Contains(c)) creatures.Add(c);

    }
    public void RemoveCreature(Creature c)
    {
        if (creatures.Contains(c)) creatures.Remove(c);
    }
}
