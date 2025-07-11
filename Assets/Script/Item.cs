using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ItemNode
{
    public ItemType itemType;
    public int number;
}
public enum ItemType
{
    Wood,
}
public class Item : BaseUnit
{
    public ItemType itemType;
    public float itemHeight;


    public PolarCoord polarCoord
    {
        get { return planet.PosToPolarCoord(transform.position); }
    }

    public bool isInAir;
    public bool isInAirLastFrame;

    public Vector3 velocity;

    public float pickUpSize;
    public bool isPickedUp;
    public Creature reserver;

    public override void Awake()
    {
        base.Awake();

        reserver = null;
    }
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        planet = MouseManager.instance.planets[0];

        if (currentCell == null) return;
        Cell belowCell = currentCell.neighbourCellNodes[1].cell;
        isInAir = !(belowCell != null && belowCell.canStand && (currentCell.radiusIdx - 1f / 2f) * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) >= -itemHeight / 2f);
        isInAirLastFrame = isInAir;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        Vector3 dir = transform.position - planet.transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        float angleRad = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
        transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);

        velocity = planet.transform.position - transform.position;
        velocity = velocity.normalized;

        if (currentCell != null && !isPickedUp)
        {

            Cell belowCell = currentCell.neighbourCellNodes[1].cell;
            isInAir = !(belowCell != null && belowCell.canStand && (currentCell.radiusIdx - 1f / 2f) * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) >= -itemHeight / 2f);

            if (isInAir) transform.position += velocity * Time.deltaTime;

            if (isInAir && !isInAirLastFrame) LeaveGround();
            if (!isInAir && isInAirLastFrame) HitGround();
        }
    }
    public override void LateUpdate()
    {
        base.LateUpdate();

        isInAirLastFrame = isInAir;

    }
    public void HitGround()
    {
        planet.items.Add(this);

    }
    public void LeaveGround()
    {

    }
    public void PickUp()
    {
        isPickedUp = true;
        transform.localScale = Vector3.one * pickUpSize;
    }
    public override void DestoryBaseUnit()
    {
        base.DestoryBaseUnit();

        planet.items.Remove(this);
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position - new Vector3(0, itemHeight / 2), transform.position + new Vector3(0, itemHeight / 2));
    }
}
