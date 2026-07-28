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

    public bool isRotate;

    [Header("Explosion")]
    public bool isExplosion;
    public float explosionRadius;
    public GameObject explosionEffectPrefab;

    [EnumCondition(nameof(motionType), (int)MotionType.Homing)] public float homingRange;       // The radius to search for a target
    [EnumCondition(nameof(motionType), (int)MotionType.Homing)] public float turnSpeed;          // How fast the projectile rotates towards the target
    [EnumCondition(nameof(motionType), (int)MotionType.Homing)] private BaseUnit homingTarget;         // Stores the current tracked target
    [EnumCondition(nameof(motionType), (int)MotionType.Homing)] public float maxSpeed;
    [EnumCondition(nameof(motionType), (int)MotionType.Homing)] public float acceleration;
    private void Awake()
    {
        planet = MouseManager.instance.planets[0];

        if (motionType == MotionType.Linear || motionType == MotionType.Homing) planetRigidbody.gravity = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (motionType == MotionType.Homing) HomingMovement();

        //ColliderCheck
        if (isCollidingWithBlock)
        {
            currentCell = planet.PosToCell(transform.position);
            Cell belowCell = null;
            if (currentCell != null) belowCell = currentCell.neighbourCellNodes[1].cell;
            if (currentCell != null && belowCell != null && belowCell.canStand && planet.CellRadiusDistance(currentCell.radiusIdx) - Vector2.Distance(transform.position, planet.transform.position) >= -(collisionRadius + planetRigidbody.velocity.magnitude * TimeManager.deltaTime))
            {
                transform.position = (transform.position - planet.transform.position).normalized * ((currentCell.radiusIdx - 1f / 2f) * planet.cellHeight + collisionRadius);

                if (isExplosion) Explode();

                OnDestory();
            }
        }

        List<BaseUnit> collidingBaseUnits = QtreeManager.instance.FindTargets(transform.position, collisionRadius);
        foreach (var baseUnit in collidingBaseUnits)
        {
            if (lastFrameCollidingBaseUnits.Contains(baseUnit)) continue;
            lastFrameCollidingBaseUnits.Add(baseUnit);
            baseUnit.TakeDamage(0, transform.position);

            if (isExplosion) Explode();
        }
        lastFrameCollidingBaseUnits = collidingBaseUnits;

        if (isRotate)
        {
            float angle = Mathf.Atan2(planetRigidbody.velocity.y, planetRigidbody.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
    private void HomingMovement()
    {
        if (homingTarget == null)
        {
            List<BaseUnit> targetsInRange = QtreeManager.instance.FindTargets(transform.position, homingRange, typeof(Enemy));
            if (targetsInRange.Count > 0)
            {
                homingTarget = targetsInRange[0];
            }
        }

        Vector3 targetDirection;
        if (homingTarget != null)
        {
            targetDirection = (homingTarget.transform.position - transform.position).normalized;
        }
        else
        {
            targetDirection = planetRigidbody.velocity.normalized;
            if (targetDirection == Vector3.zero) targetDirection = Vector3.up;
        }

        float currentSpeed = planetRigidbody.velocity.magnitude;
        Vector3 currentDirection = currentSpeed > 0 ? planetRigidbody.velocity.normalized : transform.up;

        float maxAngleDelta = turnSpeed * Mathf.Deg2Rad * TimeManager.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(currentDirection, targetDirection, maxAngleDelta, 0f).normalized;

        currentSpeed += acceleration * TimeManager.deltaTime;
        if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;

        planetRigidbody.velocity = newDirection * currentSpeed;

        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
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
    public void Explode()
    {
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        explosionEffect.layer = gameObject.layer;

        OnDestory();
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        if (motionType == MotionType.Homing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, homingRange);
        }
    }
    public void OnDestory()
    {
        Vector2 dir = transform.position - planet.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        if (destoryEffect != null) Instantiate(destoryEffect, transform.position, Quaternion.Euler(0, 0, angle));

        Destroy(gameObject);
    }
}
