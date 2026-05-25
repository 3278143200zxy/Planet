using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MotionType
{
    Homing,
    Linear,
    Projectile
}
public class Projectile : MonoBehaviour
{
    [HideInInspector] public Planet planet;
    [HideInInspector] public Cell currentCell;

    public PlanetRigidbody planetRigidbody;

    public float collisionRadius;

    public GameObject destoryEffect;

    public List<BaseUnit> lastFrameCollidingBaseUnits = new List<BaseUnit>();

    public MotionType motionType;

    public bool isCollidingWithBlock = true;

    private void Awake()
    {
        planet = MouseManager.instance.planets[0];

        if (motionType == MotionType.Linear) planetRigidbody.gravity = 0f;
        Debug.Log(2 + " " + Time.time);
    }

    // Update is called once per frame
    void Update()
    {
        if (isCollidingWithBlock)
        {
            currentCell = planet.PosToCell(transform.position);
            Cell belowCell = null;
            if (currentCell != null) belowCell = currentCell.neighbourCellNodes[1].cell;
            if (currentCell != null && belowCell.canStand && planet.CellRadiusDistance(currentCell.radiusIdx) - Vector2.Distance(transform.position, planet.transform.position) >= -(collisionRadius + planetRigidbody.velocity.magnitude * TimeManager.deltaTime))
            {
                transform.position = (transform.position - planet.transform.position).normalized * ((currentCell.radiusIdx - 1f / 2f) * planet.cellHeight + collisionRadius);
                OnDestory();
            }
        }

        List<BaseUnit> collidingBaseUnits = QtreeManager.instance.FindTargets(transform.position, collisionRadius);
        foreach (var baseUnit in collidingBaseUnits)
        {
            if (lastFrameCollidingBaseUnits.Contains(baseUnit)) continue;
            lastFrameCollidingBaseUnits.Add(baseUnit);
            baseUnit.TakeDamage(0, transform.position);
        }
        lastFrameCollidingBaseUnits = collidingBaseUnits;
    }
    public void SetVelocity(Vector3 v)
    {
        planetRigidbody.velocity = v;
    }
    public void ChangeLayer(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;

        foreach (Transform child in go.transform)
        {
            ChangeLayer(child.gameObject, layer);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
    public void OnDestory()
    {
        Vector2 dir = transform.position - planet.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        if (destoryEffect != null) Instantiate(destoryEffect, transform.position, Quaternion.Euler(0, 0, angle));

        Destroy(gameObject);
    }
}
