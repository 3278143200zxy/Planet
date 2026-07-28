using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public Planet planet;

    public float angularVelocity;

    public int startAngleIdx, endAngleIdx;
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.back, angularVelocity * TimeManager.deltaTime);

        float angle = Vector2.SignedAngle(Vector2.left, transform.up) + planet.cellIntervalAngle / 2;
        if (angle < 0) angle += 360f;
        startAngleIdx = (int)(angle / planet.cellIntervalAngle);

        angle += 180f;
        angle %= 360f;
        endAngleIdx = (int)(angle / planet.cellIntervalAngle);
    }
}
