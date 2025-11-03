using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;

public static class MathEx
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
    public static bool HasIntersection<T>(this IEnumerable<T> listA, IEnumerable<T> listB)
    {
        return listA.Any(x => listB.Contains(x));
    }
    public static List<T> GetIntersection<T>(this IEnumerable<T> listA, IEnumerable<T> listB)
    {
        if (listA == null || listB == null)
            return new List<T>();

        HashSet<T> setB = new HashSet<T>(listB);
        List<T> result = new List<T>();

        foreach (var item in listA)
        {
            if (setB.Contains(item))
                result.Add(item);
        }

        return result;
    }
    public static float Wrap(float x, float period)
    {
        return ((x % period) + period) % period;
    }
    public static bool IsRectangleOverCircle(Vector2 boxCenter, float width, float height, Vector2 circleCenter, float radius)//判断矩形与圆是否相交
    {
        Vector2 relativeCirclePos = circleCenter - boxCenter;
        relativeCirclePos = new Vector2(Mathf.Abs(relativeCirclePos.x), Mathf.Abs(relativeCirclePos.y));
        Vector2 temp = relativeCirclePos - new Vector2(width / 2, height / 2);
        if (temp.x < 0) temp = new Vector2(0, temp.y);
        if (temp.y < 0) temp = new Vector2(temp.x, 0);
        if (Vector2.Distance(temp, Vector2.zero) <= radius) return true;
        else return false;
    }
    public static bool IsRectangleOverRectangle(Vector2 boxCenter1, float width1, float height1, Vector2 boxCenter2, float width2, float height2)
    {
        // 计算矩形1的边界
        float left1 = boxCenter1.x - width1 / 2f;
        float right1 = boxCenter1.x + width1 / 2f;
        float bottom1 = boxCenter1.y - height1 / 2f;
        float top1 = boxCenter1.y + height1 / 2f;

        // 计算矩形2的边界
        float left2 = boxCenter2.x - width2 / 2f;
        float right2 = boxCenter2.x + width2 / 2f;
        float bottom2 = boxCenter2.y - height2 / 2f;
        float top2 = boxCenter2.y + height2 / 2f;

        // 判断是否有分离轴（即不重叠的条件）
        if (right1 < left2 || left1 > right2)
            return false; // X轴没有重叠

        if (top1 < bottom2 || bottom1 > top2)
            return false; // Y轴没有重叠

        // 两个矩形有重叠
        return true;
    }

}
