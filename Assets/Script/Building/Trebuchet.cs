using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trebuchet : MonoBehaviour
{
    private Animator animator;

    public Transform center;
    public float attackRange;

    public BaseUnit target;
    public float fireInterval;
    private float fireIntervalTimer;

    public float maxProjectSpeed;
    public Projectile projectilePrefab;
    public Transform firePos;
    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) target = QtreeManager.instance.FindClosestTarget(center.position, attackRange);
        if (target != null && Vector3.Distance(target.transform.position, center.position) > attackRange) target = null;

        if (target != null)
        {

            fireIntervalTimer += Time.deltaTime;
            if (fireIntervalTimer >= fireInterval)
            {
                fireIntervalTimer = 0f;
                animator.Play("Fire", 0, 0);
            }
        }
    }
    public void Fire()
    {
        float x = target.transform.position.x - firePos.position.x, y = target.transform.position.y - firePos.position.y, v = maxProjectSpeed, g = projectilePrefab.planetRigidbody.gravity;
        float a = Mathf.Atan2(v * v + Mathf.Sqrt(v * v * v * v - g * (g * x * x + 2f * v * v * y)), g * x);
        float offsetAngle = MathEx.SignedAngleRad(transform.position, Vector2.up);
        //Debug.Log(x + " " + y + " " + v + " " + g + " " + a);
        a -= offsetAngle;
        //Debug.Log(offsetAngle);
        Projectile projectile = Instantiate(projectilePrefab, firePos.position, Quaternion.identity);
        projectile.SetVelocity(MathEx.RotateVector2(Vector2.right, a) * maxProjectSpeed);

        animator.Play("Restore", 0, 0);

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, attackRange);
    }
}
