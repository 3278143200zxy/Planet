using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//left,up,back(1,1,1)
[Serializable]
public struct Dot
{
    public int x, y, z;
    public Dot(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
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

    public Text mouseCellRadiusIdxText;
    public Text mouseCellAngleIdxText;
    public Text mouseCellLayerIdxText;
    public Text mouseCellWaterText;

    public PlacedObject placedObject;
    //public List<GameObject> lastFrameNoPlacingSigns = new List<GameObject>();

    public BaseUnit baseUnit;
    public BaseUnit lastBaseUnit;
    private bool isChoosingBaseUnitFrame = false;

    public BaseUnitInfoPanel baseUnitInfoPanel;
    public BillInfoPanel billInfoPanel;

    public Color placedObjectColor;


    public List<Planet> planets = new List<Planet>();

    public bool isPushIn = false;
    private void Awake()
    {
        instance = this;

        //billInfoPanel.dropDown.onValueChanged.AddListener(OnBillInfoValueChanged);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*
        foreach (var sign in lastFrameNoPlacingSigns) sign.SetActive(false);
        lastFrameNoPlacingSigns.Clear();
        */

        foreach (Planet planet in planets)
        {
            int innerRadius = planet.innerRadius, outerRadius = planet.outerRadius;
            float cellHeight = planet.cellHeight, cellIntervalAngle = planet.cellIntervalAngle;
            Vector3 dir = mousePos - planet.transform.position;
            if (dir.magnitude >= (innerRadius - 1f / 2f) * cellHeight && dir.magnitude <= (outerRadius - 1f / 2f) * cellHeight)
            {
                Cell mouseCell = planet.PosToCell(mousePos + new Vector3(0, 0, -mousePos.z + planet.currentLayer));
                //if (mouseCell != null) Debug.Log(mouseCell.radiusIdx + " " + mouseCell.angleIdx);
                //if (mouseCell != null) mouseCell.noPlacingSign.SetActive(true);

                mouseTip.SetActive(true);
                if (mouseCell != null)
                {
                    mouseTip.transform.position = mouseCell.position;
                    mouseTip.transform.rotation = mouseCell.rotation;

                    mouseCellRadiusIdxText.text = mouseCell.radiusIdx.ToString();
                    mouseCellAngleIdxText.text = mouseCell.angleIdx.ToString();
                    mouseCellLayerIdxText.text = mouseCell.layerIdx.ToString();
                    mouseCellWaterText.text = mouseCell.water.waterAmount.ToString();
                }
                else
                {
                    mouseTip.SetActive(false);

                    mouseCellRadiusIdxText.text = "null";
                    mouseCellAngleIdxText.text = "null";
                    mouseCellLayerIdxText.text = "null";
                    mouseCellWaterText.text = "null";
                }
                //mouseTip.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);

                if (placedObject != null)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        Destroy(placedObject.gameObject);
                        continue;
                    }
                    if (mouseCell == null)
                    {
                        placedObject.transform.position = mousePos;
                        continue;
                    }
                    placedObject.transform.position = mouseCell.position;
                    Vector2 direction = mousePos - planet.transform.position;
                    placedObject.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);

                    bool canMousePlace = true;
                    //Debug.Log(placedObject.dots.Count);
                    foreach (Dot d in placedObject.dots)
                    {
                        int radiusIdx = d.y + mouseCell.radiusIdx, angleIdx = -d.x + mouseCell.angleIdx;
                        if (radiusIdx >= planet.innerRadius && radiusIdx < planet.outerRadius)
                        {
                            int temp = Mathf.RoundToInt(360f / planet.cellIntervalAngle);
                            if (angleIdx < 0) angleIdx += temp;
                            if (angleIdx >= temp) angleIdx -= temp;
                            Cell processingCell = planet.grid[radiusIdx, angleIdx, planet.currentLayer];
                            Debug.Log(radiusIdx + " " + angleIdx);
                            if (!processingCell.canPlace)
                            {
                                //lastFrameNoPlacingSigns.Add(processingCell.noPlacingSign);
                                //processingCell.noPlacingSign.SetActive(true);
                                canMousePlace = false;
                            }
                        }
                    }
                    if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()) && canMousePlace)
                    {
                        placedObject.SetPlacedObject(mouseCell);
                    }

                }

                if (Input.GetMouseButtonDown(1)) DeselectBaseUnit();

                // else mouseTip.SetActive(false);

                /*
                if (baseUnit != null && baseUnit is Creature)
                {
                    if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
                    {
                        Cell cell = planet.PosToCell(mousePos);
                        if (cell != null)
                        {
                            baseUnit.GetComponent<Creature>().SetTargetCell(cell);
                        }
                    }
                    if (Input.GetMouseButtonDown(1)) DeselectBaseUnit();
                }
                */
            }
        }
    }
    public void LateUpdate()
    {
        if (isChoosingBaseUnitFrame)
        {
            if (isPushIn && lastBaseUnit == baseUnit) CameraController.instance.PushIn(baseUnit.transform.position);
            isChoosingBaseUnitFrame = false;
            lastBaseUnit = baseUnit;

        }
    }
    public void SelectBaseUnit(BaseUnit bu)
    {
        if (placedObject != null) return;

        if (baseUnit != null && isChoosingBaseUnitFrame)
        {
            SpriteRenderer sr1 = baseUnit.GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer sr2 = bu.GetComponentInChildren<SpriteRenderer>();
            //Debug.Log(baseUnit.currentCell.layerIdx + " " + bu.currentCell.layerIdx);
            if (Mathf.Abs(baseUnit.currentCell.layerIdx - planets[0].currentLayer) < Mathf.Abs(bu.currentCell.layerIdx - planets[0].currentLayer) || sr1.sortingOrder > sr2.sortingOrder) return;
        }
        isChoosingBaseUnitFrame = true;
        DeselectBaseUnit();
        baseUnit = bu;
        baseUnit.selectionRectangle.SetActive(true);

        baseUnitInfoPanel.SetBaseUnitInfoPanel(baseUnit);

        baseUnit.OnBaseUnitSelectedEvent.Invoke();
    }
    public void DeselectBaseUnit()
    {
        baseUnitInfoPanel.gameObject.SetActive(false);
        if (baseUnit == null) return;
        baseUnit.selectionRectangle.SetActive(false);
        baseUnit.OnBaseUnitDeselectedEvent.Invoke();
        baseUnit = null;
    }
    public bool IsBaseUnitSelected(BaseUnit bu)
    {
        return baseUnit == bu;
    }
    public void ReviewPlacedObject(PlacedObject po)
    {
        DeselectBaseUnit();
        baseUnitInfoPanel.gameObject.SetActive(true);
        List<ActionType> tempActionTypes = new List<ActionType>(po.baseUnitInfo.actionTypes);
        po.baseUnitInfo.actionTypes.Clear();
        baseUnitInfoPanel.SetBaseUnitInfoPanel(po);
        po.baseUnitInfo.actionTypes = tempActionTypes;
    }
    public void OnBillInfoValueChanged(int index)
    {
        Debug.Log(index);
    }
}
