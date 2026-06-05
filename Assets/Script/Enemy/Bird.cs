using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public Enemy enemy;
    public PlanetRigidbody rb;

    public Bird predecessor;
    public Bird successor;
    public Transform successorPos;

    public BaseUnit target;
    public float acceleration;
    public float maxSpeed;
    public float collisionRadius;

    public Vector3 lastFramePos;

    public float minCruiseHeight = 10f;
    public float maxCruiseHeight = 20f;
    public float minWaypointTolerance = 1.5f;
    public float toleranceIncreasePerSecond = 0.5f;
    public float maxHorizontalDistance = 30f;

    private Vector3 currentWaypoint;
    private bool hasWaypoint = false;
    private float lastSign = 1f;
    private float currentWaypointTolerance;


    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<PlanetRigidbody>();

    }

    private void Start()
    {
        currentWaypointTolerance = minWaypointTolerance;

        if (predecessor != null)
        {
            transform.position = predecessor.successorPos.position;
            rb.velocity = predecessor.rb.velocity;

            lastFramePos = transform.position;
        }

    }
    private void Update()
    {
        if (predecessor == null)
        {
            target = QtreeManager.instance.FindClosestTarget(transform.position, 100f, typeof(Creature), new List<BaseUnit>() { enemy });
            if (target != null)
            {
                hasWaypoint = false;
                rb.velocity += acceleration * (target.transform.position - transform.position).normalized;
                if (rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;

            }
            else
            {
                if (enemy != null && enemy.planet != null)
                {
                    Vector3 gravityUp = (transform.position - enemy.planet.transform.position).normalized;

                    currentWaypointTolerance += toleranceIncreasePerSecond * Time.deltaTime;

                    if (!hasWaypoint || Vector3.Distance(transform.position, currentWaypoint) < currentWaypointTolerance)
                    {
                        currentWaypointTolerance = minWaypointTolerance;

                        Vector3 forwardDir = Vector3.Cross(gravityUp, Vector3.forward).normalized;
                        if (forwardDir == Vector3.zero) forwardDir = Vector3.right;

                        lastSign = -lastSign;
                        float randomHorizontal = Random.Range(maxHorizontalDistance * 0.3f, maxHorizontalDistance) * lastSign;
                        float randomHeight = Random.Range(minCruiseHeight, maxCruiseHeight);

                        Vector3 basePos = enemy.planet.transform.position + gravityUp * (Vector3.Distance(transform.position, enemy.planet.transform.position) - Vector3.Dot(transform.position - enemy.planet.transform.position, gravityUp) + randomHeight);
                        currentWaypoint = basePos + forwardDir * randomHorizontal;
                        hasWaypoint = true;
                    }

                    rb.velocity += acceleration * (currentWaypoint - transform.position).normalized;
                    if (rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;
                }
            }
            List<BaseUnit> collidingBaseUnits = QtreeManager.instance.FindTargets(transform.position, collisionRadius);
            for (int i = collidingBaseUnits.Count - 1; i >= 0; i--) if (collidingBaseUnits[i].GetComponent<Enemy>() != null) collidingBaseUnits.RemoveAt(i);
            if (collidingBaseUnits.Count > 1)
            {
                hasWaypoint = false;
                rb.velocity *= -1;
                Vector3 v = rb.velocity;
                Vector3 perpendicular = (Random.value < 0.5f) ? new Vector2(v.y, -v.x) : new Vector2(-v.y, v.x);
                rb.velocity += perpendicular.normalized * 0.5f;
                transform.position += rb.velocity * Time.deltaTime;
            }

            Cell belowCell = null;
            if (enemy.currentCell != null) belowCell = enemy.currentCell.neighbourCellNodes[1].cell;
            if (enemy.currentCell != null && belowCell.canStand && Vector2.Dot(rb.velocity, transform.position - enemy.planet.transform.position) < 0
               && Vector2.Distance(transform.position, enemy.planet.transform.position) - enemy.planet.CellRadiusDistance(belowCell.radiusIdx) - enemy.planet.CellHeight(belowCell.radiusIdx) / 2f <= collisionRadius)
            {
                hasWaypoint = false;
                rb.velocity *= -1;
                Vector3 v = rb.velocity;
                Vector3 perpendicular = (Random.value < 0.5f) ? new Vector2(v.y, -v.x) : new Vector2(-v.y, v.x);
                rb.velocity += perpendicular.normalized * 0.5f;
                transform.position += rb.velocity * Time.deltaTime;
            }

            Vector2 dir = rb.velocity;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (successor != null) successor.AdjustAsSuccessor();
        }
    }
    public void SetPredecessor(Bird _predecessor)
    {
        this.predecessor = _predecessor;
        if (predecessor != null) predecessor.successor = this;
    }
    public void AdjustAsSuccessor()
    {
        transform.position = predecessor.successorPos.position;

        Vector2 dir = transform.position - lastFramePos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (successor != null) successor.AdjustAsSuccessor();
    }
    private void LateUpdate()
    {
        lastFramePos = transform.position;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
        if (hasWaypoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentWaypoint);
            Gizmos.DrawSphere(currentWaypoint, 0.5f);
        }
    }
}