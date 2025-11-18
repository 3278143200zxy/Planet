using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleButton : MonoBehaviour
{
    public void SetGameObjectActive(GameObject go)
    {
        if (go.activeInHierarchy) go.SetActive(false);
        else go.SetActive(true);
    }
}
