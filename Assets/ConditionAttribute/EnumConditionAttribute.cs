using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class EnumConditionAttribute : PropertyAttribute
{
    public string EnumFieldName { get; private set; }
    public int[] ShowForEnumValues { get; private set; }

    public EnumConditionAttribute(string enumFieldName, params int[] showForEnumValues)
    {
        EnumFieldName = enumFieldName;
        ShowForEnumValues = showForEnumValues;
    }
}
