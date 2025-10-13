using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathEx
{
    public static Vector2 RotateVector2(Vector2 v, float rad)
    {
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
    public static float SignedAngleRad(Vector2 from, Vector2 to)
    {
        float dot = Vector2.Dot(from, to);
        float cross = from.x * to.y - from.y * to.x;
        return Mathf.Atan2(cross, dot);
    }
}
