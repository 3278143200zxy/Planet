using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetingType
{
    Aim,
    Still,

}
public class Trebuchet : MonoBehaviour
{
    public Animator animator;

    public Transform center;
    public float attackRange;

    public BaseUnit target;
    public float fireInterval;
    private float fireIntervalTimer;

    public float maxProjectSpeed;
    public Projectile projectilePrefab;
    public Transform firePos;

    public TargetingType targetingType;
    // Update is called once per frame
    void Update()
    {
        if (target == null) target = QtreeManager.instance.FindClosestTarget(center.position, attackRange, typeof(Enemy));
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
        Vector2 dir = Vector2.up * maxProjectSpeed;
        if (targetingType == TargetingType.Aim)
        {
            float x = target.transform.position.x - firePos.position.x, y = target.transform.position.y - firePos.position.y, v = maxProjectSpeed, g = projectilePrefab.planetRigidbody.gravity;
            float a = Mathf.Atan2(v * v + Mathf.Sqrt(v * v * v * v - g * (g * x * x + 2f * v * v * y)), g * x);
            float offsetAngle = MathEx.SignedAngleRad(transform.position, Vector2.up);
            //Debug.Log(x + " " + y + " " + v + " " + g + " " + a);
            a -= offsetAngle;
            dir = MathEx.RotateVector2(Vector2.right, a) * maxProjectSpeed;
        }
        //Debug.Log(offsetAngle);
        Debug.Log(1 + " " + Time.time);
        Projectile projectile = Instantiate(projectilePrefab, firePos.position, Quaternion.identity);
        projectile.SetVelocity(dir);
        projectile.ChangeLayer(projectile.gameObject, gameObject.layer);

        animator.Play("Restore", 0, 0);

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, attackRange);
    }
}
