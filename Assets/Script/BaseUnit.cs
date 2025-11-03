using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
[Serializable]
public struct Rectangle
{
    public float x, y;
    public float width, height;

    public Rectangle(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }
}
[Serializable]
public enum BaseUnitType
{
    Creature,
    Warehouse,
    Campfire,
    Ladder,
    Wood,
    Stone,
    ArbalestTower,
    Item,
}
public class BaseUnit : MonoBehaviour
{
    public BaseUnitType baseUnitType;

    public bool canClick = true;
    public List<Circle> clickCircles = new List<Circle>();
    public List<Rectangle> clickRectangles = new List<Rectangle>();

    public bool isRefreshQtree = true;

    public GameObject selectionRectangle;

    public BaseUnitInfo baseUnitInfo;

    public Dictionary<ActionType, UnityEvent> actionTypeToEvent = new Dictionary<ActionType, UnityEvent>();

    public Planet planet;

    public UnityEvent OnDestoryEvent = new UnityEvent();
    public Cell currentCell
    {
        get { return planet.PosToCell(transform.position); }
    }
    public virtual void Awake()
    {
        planet = MouseManager.instance.planets[0];

        foreach (ActionType type in Enum.GetValues(typeof(ActionType))) actionTypeToEvent[type] = new UnityEvent();
    }
    public virtual void Start()
    {
        //QtreeManager.instance.baseUnits.Add(this);
    }
    public virtual void Update()
    {
        if (canClick && Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            Vector3 mousePos = MouseManager.instance.mousePos;
            if (Vector2.Distance(transform.position, mousePos) < 15f)
            {
                bool isFinished = false;
                float offsetRad = MathEx.SignedAngleRad(transform.position, Vector2.up);
                mousePos = (Vector3)MathEx.RotateVector2(mousePos - transform.position, offsetRad) + transform.position;
                foreach (Circle c in clickCircles)
                {
                    if (isFinished) break;
                    float sqrDistance = Vector2.SqrMagnitude(transform.position + new Vector3(c.x, c.y) - mousePos);
                    if (sqrDistance < c.radius * c.radius)
                    {
                        MouseManager.instance.SelectBaseUnit(this);
                        isFinished = true;
                    }
                }
                foreach (Rectangle r in clickRectangles)
                {
                    if (isFinished) break;
                    Vector3 center = transform.position + new Vector3(r.x, r.y);
                    float halfWidth = r.width / 2f;
                    float halfHeight = r.height / 2f;
                    if (mousePos.x >= center.x - halfWidth && mousePos.x <= center.x + halfWidth && mousePos.y >= center.y - halfHeight && mousePos.y <= center.y + halfHeight)
                    {
                        isFinished = true;
                        MouseManager.instance.SelectBaseUnit(this);
                    }
                }
            }
        }
    }
    public virtual void LateUpdate()
    {

    }
    public virtual void OnDrawGizmos()
    {
        foreach (Circle c in clickCircles)
        {
            Gizmos.DrawWireSphere(transform.position + new Vector3(c.x, c.y), c.radius);
        }
        foreach (Rectangle r in clickRectangles)
        {
            Vector3 c = new Vector3(r.x, r.y, 0f);
            float halfW = r.width / 2f;
            float halfH = r.height / 2f;

            Vector3 topLeft = c + new Vector3(-halfW, halfH, 0);
            Vector3 topRight = c + new Vector3(halfW, halfH, 0);
            Vector3 bottomRight = c + new Vector3(halfW, -halfH, 0);
            Vector3 bottomLeft = c + new Vector3(-halfW, -halfH, 0);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
    public void AddActionType(BaseUnit baseUnit, ActionType type)
    {
        if (baseUnitInfo.actionTypes.Contains(type)) return;
        baseUnitInfo.actionTypes.Add(type);
        if (MouseManager.instance.baseUnit == baseUnit) MouseManager.instance.baseUnitInfoPanel.ActivateActionButton(type);
    }
    public void RemoveActionType(BaseUnit baseUnit, ActionType type)
    {
        if (!baseUnitInfo.actionTypes.Contains(type)) return;
        baseUnitInfo.actionTypes.Remove(type);
        if (MouseManager.instance.baseUnit == baseUnit) MouseManager.instance.baseUnitInfoPanel.DisableActionButton(type);
    }
    public virtual void TakeDamage(float damage)
    {

    }
    public virtual void TakeDamage(float damage, Vector3 pos)
    {

    }
    public List<BaseUnit> BaseUnitsInCollisionRange()
    {

        return null;
    }
    public virtual void DestoryBaseUnit()
    {

        //Destroy(gameObject);
        PoolManager.instance.DestoryBaseUnit(this);
    }
}
