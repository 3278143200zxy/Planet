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


    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<PlanetRigidbody>();

    }

    private void Start()
    {
        if (predecessor != null)
        {
            transform.position = predecessor.successorPos.position;
            rb.velocity = predecessor.rb.velocity;

            lastFramePos = transform.position;
        }

    }
    private void Update()
    {
        // if (target == null || rb.velocity.magnitude < 0.05f) 
        if (predecessor == null)
        {
            target = QtreeManager.instance.FindClosestTarget(transform.position, 100f, typeof(Creature), new List<BaseUnit>() { enemy });
            if (target != null)
            {
                rb.velocity += acceleration * (target.transform.position - transform.position).normalized;
                if (rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;

                List<BaseUnit> collidingBaseUnits = QtreeManager.instance.FindTargets(transform.position, collisionRadius);
                for (int i = collidingBaseUnits.Count - 1; i >= 0; i--) if (collidingBaseUnits[i].GetComponent<Enemy>() != null) collidingBaseUnits.RemoveAt(i);
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
        //rb.velocity = predecessor.rb.velocity;

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
    }
}
