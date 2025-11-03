using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimpleEventType
{
    Destory,
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
            if (simpleEventNodes[i].parameters.Count == 0)
            {
                InvokeSimpleEvent(simpleEventNodes[i]);
                simpleEventNodes.RemoveAt(i);
            }
        }
    }
    private void Update()
    {
        for (int i = 0; i < simpleEventNodes.Count; i++)
        {
            simpleEventNodes[i].parameters[0] -= Time.deltaTime;
            if(simpleEventNodes[i].parameters[0] <= 0)
            {
                InvokeSimpleEvent(simpleEventNodes[i]);
                simpleEventNodes.RemoveAt(i);
            }
        }
    }
    public void InvokeSimpleEvent(SimpleEventNode simpleEventNode)
    {
        switch (simpleEventNode.simpleEventType)
        {
            case SimpleEventType.Destory:
                Destroy(gameObject);
                break;
        }
    }
}

