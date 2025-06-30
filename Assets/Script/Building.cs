using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    public List<Cell> cells = new List<Cell>();
    public UnityEvent SetBuildingEvent = new UnityEvent();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetBuilding()
    {
        SetBuildingEvent.Invoke();
    }
    public void AddCell(Cell cell)
    {
        cells.Add(cell);
    }
}
