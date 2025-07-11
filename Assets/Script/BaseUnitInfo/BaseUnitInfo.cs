using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class BaseUnitInfo
{

    public string baseUnitName;
    public string baseUnitDescription;

    public List<ActionType> actionTypes = new List<ActionType>();
}
