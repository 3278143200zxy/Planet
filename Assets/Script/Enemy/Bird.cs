using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    private Enemy enemy;
    private PlanetRigidbody rb;

    public BaseUnit target;
    public float acceleration;
    public float maxSpeed;
    public float collisionRadius;
    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<PlanetRigidbody>();
    }

    private void Start()
    {

    }
    private void Update()
    {
        // if (target == null || rb.velocity.magnitude < 0.05f) 
        target = QtreeManager.instance.FindClosestTarget(transform.position, 100f, typeof(Creature), new List<BaseUnit>() { enemy });
        if (target != null)
        {
            rb.velocity += acceleration * (target.transform.position - transform.position).normalized;
            if (rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;

            List<BaseUnit> collidingBaseUnits = QtreeManager.instance.FindTargets(transform.position, collisionRadius);
            if (collidingBaseUnits.Count > 1)
            {
                rb.velocity *= -1;
                Vector3 v = rb.velocity;
                Vector3 perpendicular = (Random.value < 0.5f) ? new Vector2(v.y, -v.x) : new Vector2(-v.y, v.x);
                rb.velocity += perpendicular.normalized * 0.5f;
                transform.position += rb.velocity * Time.deltaTime;
            }

            Cell belowCell = null;
            if (enemy.currentCell != null) belowCell = enemy.currentCell.neighbourCellNodes[1].cell;
            if (enemy.currentCell != null && belowCell.canStand && (enemy.currentCell.radiusIdx - 1f / 2f) * enemy.planet.cellHeight - Vector2.Distance(transform.position, enemy.planet.transform.position) >= -collisionRadius)
            {
                rb.velocity *= -1;
                Vector3 v = rb.velocity;
                Vector3 perpendicular = (Random.value < 0.5f) ? new Vector2(v.y, -v.x) : new Vector2(-v.y, v.x);
                rb.velocity += perpendicular.normalized * 0.5f;
                transform.position += rb.velocity * Time.deltaTime;
            }

            Vector2 dir = rb.velocity;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}
