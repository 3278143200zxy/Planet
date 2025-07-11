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
public class BaseUnit : MonoBehaviour
{
    public bool canClick = true;
    public List<Circle> clickCircles = new List<Circle>();
    public GameObject selectionRectangle;

    public BaseUnitInfo baseUnitInfo;

    public Dictionary<ActionType, UnityEvent> actionTypeToEvent = new Dictionary<ActionType, UnityEvent>();

    public Planet planet;
    public Cell currentCell
    {
        get { return planet.PosToCell(transform.position); }
    }
    public virtual void Awake()
    {
        planet = MouseManager.instance.planets[0];

        actionTypeToEvent[ActionType.CutTree] = new UnityEvent();
        actionTypeToEvent[ActionType.CancelCuttingTree] = new UnityEvent();
    }
    public virtual void Start()
    {

    }
    public virtual void Update()
    {
        if (canClick && Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            foreach (Circle c in clickCircles)
            {
                float sqrDistance = Vector2.SqrMagnitude(transform.position + new Vector3(c.x, c.y) - MouseManager.instance.mousePos);
                if (sqrDistance < c.radius * c.radius) MouseManager.instance.SelectBaseUnit(this);
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
    }
    public void AddActionType(ActionType type)
    {
        if (baseUnitInfo.actionTypes.Contains(type)) return;
        baseUnitInfo.actionTypes.Add(type);
        MouseManager.instance.baseUnitInfoPanel.ActivateActionButton(type);
    }
    public void RemoveActionType(ActionType type)
    {
        if (!baseUnitInfo.actionTypes.Contains(type)) return;
        baseUnitInfo.actionTypes.Remove(type);
        MouseManager.instance.baseUnitInfoPanel.DisableActionButton(type);
    }
    public virtual void DestoryBaseUnit()
    {
        if (MouseManager.instance.baseUnit == this) MouseManager.instance.DeselectBaseUnit();

        Destroy(gameObject);
    }
}
