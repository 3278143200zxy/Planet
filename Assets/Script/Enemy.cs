using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BaseUnit
{
    [Header("Enemy")]
    public GameObject bloodEffectPrefab;
    public override void Awake()
    {
        base.Awake();
    }
    public override void Start()
    {
        base.Start();

        QtreeManager.instance.AddBaseUnit(this);
    }
    public override void Update()
    {
        base.Update();
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }
    public override void TakeDamage(float damage, Vector3 pos)
    {
        base.TakeDamage(damage, pos);

        GameObject bloodEffect = Instantiate(bloodEffectPrefab, transform.position, Quaternion.identity);
        Vector2 dir = transform.position - pos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bloodEffect.transform.rotation = Quaternion.Euler(0, 0, angle);


    }
    public override void DestoryBaseUnit()
    {
        base.DestoryBaseUnit();
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }

}
