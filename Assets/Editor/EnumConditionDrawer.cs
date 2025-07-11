using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(EnumConditionAttribute))]
public class EnumConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 获取所有作用在该字段上的 EnumConditionAttribute 实例
        EnumConditionAttribute[] conditionAttributes = fieldInfo.GetCustomAttributes(typeof(EnumConditionAttribute), true) as EnumConditionAttribute[];

        bool shouldShow = false;
        foreach (var condition in conditionAttributes)
        {
            SerializedProperty enumProperty = GetEnumProperty(property, condition.EnumFieldName);
            if (enumProperty != null && condition.ShowForEnumValues.Contains(enumProperty.intValue))
            {
                shouldShow = true;
                break;
            }
        }

        if (shouldShow)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        EnumConditionAttribute[] conditionAttributes = fieldInfo.GetCustomAttributes(typeof(EnumConditionAttribute), true) as EnumConditionAttribute[];

        bool shouldShow = false;
        foreach (var condition in conditionAttributes)
        {
            SerializedProperty enumProperty = GetEnumProperty(property, condition.EnumFieldName);
            if (enumProperty != null && condition.ShowForEnumValues.Contains(enumProperty.intValue))
            {
                shouldShow = true;
                break;
            }
        }
        return shouldShow ? EditorGUI.GetPropertyHeight(property, label, true) : -EditorGUIUtility.standardVerticalSpacing;
    }

    /// <summary>
    /// 通过属性路径获取目标枚举字段（对于 List<T> 结构可能需要特殊处理）
    /// </summary>
    private SerializedProperty GetEnumProperty(SerializedProperty property, string enumFieldName)
    {
        string path = property.propertyPath;
        string enumPath = path.Replace(property.name, enumFieldName);
        return property.serializedObject.FindProperty(enumPath);
    }
}
