using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimpleEventType
{
    Destory,
    Zoom,
}
[Serializable]
public class SimpleEventNode
{
    public SimpleEventType simpleEventType;
    public List<float> parameters = new List<float>();

    public SimpleEventNode(SimpleEventType _simpleEventType, List<float> _parameters)
    {
        simpleEventType = _simpleEventType;
        parameters = _parameters;
    }
}
public class SimpleEvent : MonoBehaviour
{
    public List<SimpleEventNode> simpleEventNodes = new List<SimpleEventNode>();
    private void Start()
    {
        for (int i = 0; i < simpleEventNodes.Count; i++)
        {
            SimpleEventNode eventNode = simpleEventNodes[i];
            switch (eventNode.simpleEventType)
            {
                case SimpleEventType.Zoom:
                    eventNode.parameters.Add(0);
                    transform.localScale = Vector3.one * eventNode.parameters[2];
                    break;
            }
        }
    }
    private void Update()
    {
        for (int i = simpleEventNodes.Count - 1; i >= 0; i--)
        {
            SimpleEventNode eventNode = simpleEventNodes[i];
            List<float> parameters = eventNode.parameters;
            switch (eventNode.simpleEventType)
            {
                case SimpleEventType.Destory:
                    if (parameters[0] <= 0)
                    {
                        Destroy(gameObject);
                        simpleEventNodes.RemoveAt(i);
                    }
                    else parameters[0] -= TimeManager.deltaTime;
                    break;
                case SimpleEventType.Zoom:
                    if (parameters[0] <= 0)
                    {
                        parameters[4] += TimeManager.deltaTime;
                        if (parameters[4] <= eventNode.parameters[1])
                        {
                            transform.localScale = Vector3.one * ((parameters[4] / parameters[1]) * (parameters[3] - parameters[2]) + parameters[2]);
                        }
                        else simpleEventNodes.RemoveAt(i);
                    }
                    else parameters[0] -= TimeManager.deltaTime;
                    break;
            }
        }
    }
}

