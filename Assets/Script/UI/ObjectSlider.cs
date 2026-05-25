using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectSlider : MonoBehaviour
{
    public GameObject fillRect;

    public SliderDirection sliderDirection;
    [Range(0f, 1f)] public float value;


    public void SetValue(float v)
    {
        value = v;

        fillRect.transform.localPosition = new Vector3(0, (value - 1) / 2, 0);
        fillRect.transform.localScale = new Vector3(1, value, 1);
    }
    private void OnValidate()
    {
        SetValue(value);
    }
}

