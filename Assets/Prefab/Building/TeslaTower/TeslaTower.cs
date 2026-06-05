using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaTower : MonoBehaviour
{
    public Building building;

    public Transform center;
    public float attackRange;

    public BaseUnit target;
    public float attackInterval;
    private float attackIntervalTimer;
    public float chainAttackRange;
    public int chainAttackNumber;

    public Lighting lightingPrefab;

    public float damage;

    void Update()
    {
        target = QtreeManager.instance.FindClosestTarget(center.position, attackRange, typeof(Enemy));
        if (target != null)
        {
            attackIntervalTimer += Time.deltaTime;
            if (attackIntervalTimer > attackInterval)
            {
                Fire();
                attackIntervalTimer = 0;
            }
        }
    }
    public void Fire()
    {
        Lighting lighting = Instantiate(lightingPrefab);
        lighting.SetPoint(center.position, target.transform.position);
        building.ChangeLayer(lighting.gameObject, gameObject.layer);
        target.TakeDamage(damage);

        List<BaseUnit> chainTargets = QtreeManager.instance.FindTargets(target.transform.position, chainAttackRange, typeof(Enemy), new List<BaseUnit>() { target });
        if (chainTargets.Count > 0)
        {
            lighting = Instantiate(lightingPrefab);
            lighting.SetPoint(target.transform.position, chainTargets[0].transform.position);
            building.ChangeLayer(lighting.gameObject, gameObject.layer);
            target.TakeDamage(damage);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center.position, attackRange);
        Gizmos.DrawWireSphere(center.position, chainAttackRange);
    }
}
