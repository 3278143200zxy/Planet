using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.05f;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }
    public void ZoomOut()
    {
        transform.localScale = Vector3.one;
    }
    public void SetGameObjectActive(GameObject go)
    {
        go.SetActive(true);
    }
    public void SetGameObjectInactive(GameObject go)
    {
        go.SetActive(false);

    }
    public void ChangeGameObjectActive(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
    }
}
