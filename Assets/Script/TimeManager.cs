using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    private static float timeScaleValue = 1f;
    public static float timeScale
    {
        get => timeScaleValue;
        set
        {
            timeScaleValue = value;

        }
    }

    public static float deltaTime
    {
        get => Time.deltaTime * timeScaleValue;
    }
    public UnityEvent<float> ChangeTimeScaleEvent = new UnityEvent<float>();

    public Text fpsText;
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(timeScale);
        float fps = 1f / Time.deltaTime;
        fpsText.text = $"FPS: {fps:F1}";


    }
    public void ChangeTimeScale(float ts)
    {
        timeScale = ts;
        ChangeTimeScaleEvent.Invoke(timeScale);
    }
}
