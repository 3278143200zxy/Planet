using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Planet planet;
    public Cell currentCell;

    public PlanetRigidbody planetRigidbody;

    public float collisionRadius;

    public GameObject destoryEffect;

    public List<BaseUnit> lastFrameCollidingBaseUnits = new List<BaseUnit>();
    private void Awake()
    {
        planet = MouseManager.instance.planets[0];
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        currentCell = planet.PosToCell(transform.position);
        Cell belowCell = null;
        if (currentCell != null) belowCell = currentCell.neighbourCellNodes[1].cell;
        if (currentCell != null && belowCell.canStand && (currentCell.radiusIdx - 1f / 2f) * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) >= -(collisionRadius + planetRigidbody.velocity.magnitude * Time.deltaTime))
        {
            transform.position = (transform.position - planet.transform.position).normalized * ((currentCell.radiusIdx - 1f / 2f) * planet.cellHeight + collisionRadius);
            OnDestory();
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
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
    public void OnDestory()
    {
        Vector2 dir = transform.position - planet.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        Instantiate(destoryEffect, transform.position, Quaternion.Euler(0, 0, angle));

        Destroy(gameObject);
    }
}
