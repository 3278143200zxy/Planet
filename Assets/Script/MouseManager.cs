using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public struct Dot
{
    public int x, y;
    public Dot(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
public enum MouseState
{
    Default,
    PlacedObject,
    Creature,
    CreatureRange,
}
public class MouseManager : MonoBehaviour
{
    public static MouseManager instance;

    public MouseState mouseState = MouseState.Default;

    public Vector3 mousePos
    {
        get
        {
            Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return mp - new Vector3(0, 0, mp.z);
        }
    }
    public GameObject mouseTip;
    public PlacedObject placedObject;
    public List<GameObject> lastFrameNoPlacingSigns = new List<GameObject>();

    public Creature creature;
    public bool isChoosingCreature = false;

    public Color placedObjectColor;


    public List<Planet> planets = new List<Planet>();
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (var sign in lastFrameNoPlacingSigns) sign.SetActive(false);
        lastFrameNoPlacingSigns.Clear();


        foreach (Planet planet in planets)
        {
            int innerRadius = planet.innerRadius, outerRadius = planet.outerRadius;
            float cellHeight = planet.cellHeight, cellIntervalAngle = planet.cellIntervalAngle;
            Vector3 dir = mousePos - planet.transform.position;
            if (dir.magnitude >= (innerRadius - 1f / 2f) * cellHeight && dir.magnitude <= (outerRadius - 1f / 2f) * cellHeight)
            {
                Cell mouseCell = planet.PosToCell(mousePos);
                //if (mouseCell != null) Debug.Log(mouseCell.radiusIdx + " " + mouseCell.angleIdx);
                //if (mouseCell != null) mouseCell.noPlacingSign.SetActive(true);

                mouseTip.SetActive(true);
                //mouseTip.transform.position = dir + transform.position;
                float angle = Vector2.SignedAngle(Vector2.right, dir) + cellIntervalAngle / 2;
                if (angle < 0) angle -= cellIntervalAngle;
                angle = (int)(angle / cellIntervalAngle) * cellIntervalAngle;
                //Debug.Log(angle);
                float angleRad = angle * Mathf.Deg2Rad;
                float distance = dir.magnitude + cellHeight / 2;
                distance = (int)(distance / cellHeight) * cellHeight;
                Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * distance;
                mouseTip.transform.position = planet.transform.position + direction;
                mouseTip.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);


                if (placedObject != null)
                {
                    placedObject.transform.position = planet.transform.position + direction;
                    placedObject.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);

                    bool canMousePlace = true;
                    foreach (Dot d in placedObject.dots)
                    {
                        int radiusIdx = d.y + mouseCell.radiusIdx, angleIdx = d.x + mouseCell.angleIdx;
                        if (radiusIdx >= planet.innerRadius && radiusIdx < planet.outerRadius)
                        {
                            int temp = Mathf.RoundToInt(360f / planet.cellIntervalAngle);
                            if (angleIdx < 0) angle += temp;
                            if (angleIdx >= temp) angleIdx -= temp;
                            Cell processingCell = planet.grid[radiusIdx, angleIdx];
                            if (processingCell.building != null)
                            {
                                lastFrameNoPlacingSigns.Add(processingCell.noPlacingSign);
                                processingCell.noPlacingSign.SetActive(true);
                                canMousePlace = false;
                            }
                        }
                    }
                    if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()) && canMousePlace)
                    {
                        placedObject.SetPlacedObject(mouseCell);
                        placedObject = null;
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        Destroy(placedObject.gameObject);
                    }

                }

                else mouseTip.SetActive(false);

                if (creature != null)
                {
                    if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
                    {
                        Cell cell = planet.PosToCell(mousePos);
                        if (cell != null)
                        {
                            List<Cell> temp = planet.FindPath(planet.PosToCell(creature.transform.position), cell);
                            if (temp != null)
                            {
                                creature.path = temp;
                                creature.ChangeCreatureState(CreatureState.Walk);
                            }
                        }
                    }
                    if (Input.GetMouseButtonDown(1)) DeselectCreature();
                }

            }
        }
    }
    public void SelectCreature(Creature c)
    {
        if (placedObject == null)
        {
            creature = c;
            isChoosingCreature = true;
            creature.outline.SetActive(true);
        }
    }
    public void DeselectCreature()
    {
        if (creature == null) return;
        creature.outline.SetActive(false);
        creature = null;
    }
    private void LateUpdate()
    {
        isChoosingCreature = false;
    }
}
