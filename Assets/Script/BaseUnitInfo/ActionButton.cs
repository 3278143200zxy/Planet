using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum ActionType
{
    CutTree,
    CancelCuttingTree,
}
public class ActionButton : MonoBehaviour
{

    [HideInInspector] public ActionType actionType;

    public void PressButton()
    {
        MouseManager.instance.baseUnit.actionTypeToEvent[actionType].Invoke();
    }
}
