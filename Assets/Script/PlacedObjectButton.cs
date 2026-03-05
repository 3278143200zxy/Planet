using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacedObjectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlacedObject placedObjectPrefab;
    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseManager.instance.ReviewPlacedObject(placedObjectPrefab);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        MouseManager.instance.DeselectBaseUnit();
    }
    public void SetMousePlacedObject()
    {
        PlacedObject p = MouseManager.instance.placedObject;
        if (p != null) Destroy(p.gameObject);
        PlacedObject placedObject = Instantiate(placedObjectPrefab);
        placedObject.ChangeLayer(placedObject.gameObject, LayerMask.NameToLayer(MouseManager.instance.planets[0].currentLayer.ToString()));
        //MouseManager.instance.SelectBaseUnit(MouseManager.instance.placedObject);
        MouseManager.instance.placedObject = placedObject;
        MouseManager.instance.DeselectBaseUnit();
        //Debug.Log(1);

    }
}
