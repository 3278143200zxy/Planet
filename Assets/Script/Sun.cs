using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public float angularVelocity;

    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.back, angularVelocity * Time.deltaTime);
    }
}
