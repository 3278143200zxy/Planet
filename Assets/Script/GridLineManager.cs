using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

public class GridLineManager : MonoBehaviour
{
    public static GridLineManager instance;

    public Transform horizontalAxisSegmentPool;

    public float axisSegmentInterval;
    public List<Text> axisSegmentParameterTexts = new List<Text>();
    public float intervalAngle;

    private void Awake()
    {
        instance = this;
        axisSegmentInterval = float.MaxValue;
        axisSegmentParameterTexts = new List<Text>(horizontalAxisSegmentPool.GetComponentsInChildren<Text>());
        for (int i = 1; i < horizontalAxisSegmentPool.childCount; i++) axisSegmentInterval = Mathf.Min(axisSegmentInterval, horizontalAxisSegmentPool.GetChild(i).localPosition.x - horizontalAxisSegmentPool.GetChild(0).localPosition.x);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        float cameraRad = MathEx.SignedAngleRad(Camera.main.transform.position, Vector2.up);
        float cameraDeg = cameraRad * Mathf.Rad2Deg;
        horizontalAxisSegmentPool.transform.localPosition = new Vector3(-axisSegmentInterval * MathEx.Wrap(cameraDeg, intervalAngle) / intervalAngle, 0);

        int centerTextId = 2;
        cameraDeg = (cameraDeg + 360) % 360;
        for (int i = 0; i < axisSegmentParameterTexts.Count; i++)
        {

            float parameter = ((int)(cameraDeg / intervalAngle) + i - centerTextId) * intervalAngle;
            parameter = (parameter + 360) % 360;
            axisSegmentParameterTexts[i].text = parameter.ToString() + "°";
        }


    }
    public void ShowOrHideGridLine()
    {
        if (horizontalAxisSegmentPool.gameObject.activeInHierarchy) horizontalAxisSegmentPool.gameObject.SetActive(false);
        else horizontalAxisSegmentPool.gameObject.SetActive(true);

    }

}