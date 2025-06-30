using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

[Serializable]
public struct Circle
{
    public float x, y;
    public float radius;
    public Circle(float x, float y, float radius)
    {
        this.x = x;
        this.y = y;
        this.radius = radius;
    }
}
public enum CreatureState
{
    Idle,
    Walk,
}
public class Creature : MonoBehaviour
{
    public CreatureState creatureState;
    private Animator animator;

    public Planet planet;
    public PolarCoord polarCoord
    {
        get { return planet.PosToPolarCoord(transform.position); }
    }
    public Cell currentCell
    {
        get { return planet.PosToCell(transform.position); }
    }
    public Cell lastCurrentCell;
    public float currentAngle
    {
        get { return Vector2.SignedAngle(Vector2.right, transform.position - planet.transform.position); }
    }

    public List<Circle> clickCircles = new List<Circle>();

    public float idleWalkSpeed;
    public float minIdleWalkInterval, maxIdleWalkInterval;
    private float idleWalkTimer, idleWalkInterval;
    public float idleWalkAngleOffset;
    public bool isIdleWalking;

    public List<Cell> path = new List<Cell>();
    public float walkSpeed;
    public float climbSpeed;

    public GameObject outline;
    public int rangeStartAngleIdx, rangeEndAngleIdx;
    private void Awake()
    {

        animator = GetComponent<Animator>();

    }
    // Start is called before the first  update
    void Start()
    {
        planet = MouseManager.instance.planets[0];
        lastCurrentCell = currentCell;
        //ChangeCreatureState(CreatureState.Idle);

        idleWalkInterval = UnityEngine.Random.Range(minIdleWalkInterval, maxIdleWalkInterval);
        idleWalkAngleOffset = UnityEngine.Random.Range(-planet.cellIntervalAngle / 2, planet.cellIntervalAngle / 2);
    }

    // Update is called once per 
    void Update()
    {

        Vector3 dir = transform.position - planet.transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        float angleRad = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
        transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);

        if (Input.GetMouseButtonDown(0) && !MouseManager.instance.isChoosingCreature)
        {
            foreach (Circle c in clickCircles)
            {
                float sqrDistance = Vector2.SqrMagnitude(transform.position + new Vector3(c.x, c.y) - MouseManager.instance.mousePos);
                if (sqrDistance < c.radius * c.radius) MouseManager.instance.SelectCreature(this);
            }
        }
        switch (creatureState)
        {
            case CreatureState.Idle:
                /*
                if (isIdleWalking)
                {
                    float step = idleWalkSpeed / (polarCoord.r * planet.cellHeight) * Time.deltaTime * Mathf.Rad2Deg;
                    float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                    float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                    Debug.Log(targetAngle + " " + currentAngle);
                    Debug.Log(angleDiff);
                    if (Mathf.Abs(angleDiff) <= step)
                    {
                        isIdleWalking = false;
                        transform.RotateAround(planet.transform.position, Vector3.forward, angleDiff);

                        animator.Play("Idle", 0, 0f);
                        animator.Update(0f);
                    }
                    else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                }
                else
                {
                    idleWalkTimer += Time.deltaTime;
                    if (idleWalkTimer > idleWalkInterval)
                    {
                        isIdleWalking = true;
                        idleWalkTimer = 0;
                        idleWalkInterval = Random.Range(minIdleWalkInterval, maxIdleWalkInterval);
                        idleWalkAngleOffset = Random.Range(-planet.cellIntervalAngle / 2, planet.cellIntervalAngle / 2);

                        animator.Play("Walk", 0, 0f);
                        animator.Update(0f);
                    }
                }
                */
                break;
            case CreatureState.Walk:
                if (path.Count > 0)
                {
                    if (path[0] != lastCurrentCell)
                    {
                        if (path[0].radiusIdx == lastCurrentCell.radiusIdx)
                        {
                            float step = walkSpeed / (polarCoord.r * planet.cellHeight) * Time.deltaTime * Mathf.Rad2Deg;
                            //float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                            float targetAngle = path[0].angleIdx * planet.cellIntervalAngle;
                            float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                            if (Mathf.Abs(angleDiff) <= step)
                            {
                                transform.RotateAround(planet.transform.position, Vector3.forward, angleDiff);
                                lastCurrentCell = path[0];
                                path.RemoveAt(0);
                            }
                            else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                        }
                        else
                        {
                            float step = climbSpeed * Time.deltaTime;
                            float distanceDiff = Vector2.Distance(path[0].transform.position, planet.transform.position) - Vector2.Distance(transform.position, planet.transform.position);
                            //Debug.Log(distanceDiff + " " + step);
                            if (Mathf.Abs(distanceDiff) <= step)
                            {
                                transform.position += transform.up * distanceDiff;
                                lastCurrentCell = path[0];
                                path.RemoveAt(0);
                            }
                            else transform.position += transform.up * step * Mathf.Sign(distanceDiff);
                        }
                    }
                    else
                    {
                        if (currentCell.radiusIdx * planet.cellHeight - Vector2.Distance(transform.position, planet.transform.position) <= 0.05f)
                        {
                            float step = walkSpeed / (polarCoord.r * planet.cellHeight) * Time.deltaTime * Mathf.Rad2Deg;
                            //float targetAngle = polarCoord.a * planet.cellIntervalAngle + idleWalkAngleOffset;
                            float targetAngle = path[0].angleIdx * planet.cellIntervalAngle;
                            float angleDiff = Mathf.DeltaAngle(targetAngle, currentAngle);
                            if (Mathf.Abs(angleDiff) <= step)
                            {
                                transform.RotateAround(planet.transform.position, Vector3.forward, angleDiff);
                                lastCurrentCell = path[0];
                                path.RemoveAt(0);
                            }
                            else transform.RotateAround(planet.transform.position, Vector3.forward, step * -Mathf.Sign(angleDiff));
                        }
                        else
                        {

                            float step = climbSpeed * Time.deltaTime;
                            float distanceDiff = Vector2.Distance(path[0].transform.position, planet.transform.position) - Vector2.Distance(transform.position, planet.transform.position);
                            //Debug.Log(distanceDiff + " " + step);
                            if (Mathf.Abs(distanceDiff) <= step)
                            {
                                transform.position += transform.up * distanceDiff;
                                lastCurrentCell = path[0];
                                path.RemoveAt(0);
                            }
                            else transform.position += transform.up * step * Mathf.Sign(distanceDiff);
                        }
                    }

                }
                if (path.Count == 0) ChangeCreatureState(CreatureState.Idle);
                break;

        }
    }
    public void ChangeCreatureState(CreatureState cs)
    {
        creatureState = cs;
        switch (cs)
        {
            case CreatureState.Idle:
                animator.Play("Idle", 0, 0f);
                animator.Update(0f);
                break;
            case CreatureState.Walk:
                animator.Play("Walk", 0, 0f);
                animator.Update(0f);
                break;
        }
    }
    public void OnDrawGizmos()
    {
        foreach (Circle c in clickCircles)
        {
            Gizmos.DrawWireSphere(transform.position + new Vector3(c.x, c.y), c.radius);
        }
    }
    public void SetTargetCell(Cell tc)
    {

    }
}
