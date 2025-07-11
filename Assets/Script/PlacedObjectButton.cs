using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedObjectButton : MonoBehaviour
{
    public PlacedObject placedObjectPrefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetMousePlacedObject()
    {
        PlacedObject p = MouseManager.instance.placedObject;
        if (p != null) Destroy(p.gameObject);
        MouseManager.instance.placedObject = Instantiate(placedObjectPrefab);
        MouseManager.instance.DeselectBaseUnit();
        //Debug.Log(1);

    }
}
