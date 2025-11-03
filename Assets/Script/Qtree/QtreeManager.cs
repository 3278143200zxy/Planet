using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class QtreeManager : MonoBehaviour
{
    public static QtreeManager instance;

    public int maxDepth;
    public int maxChild;
    public float maxWidth;
    public float maxHeight;
    public Qtree qtree;

    public List<BaseUnit> baseUnits = new List<BaseUnit>();
    private void Awake()
    {
        instance = this;
        qtree = new Qtree(0, Vector2.zero, maxWidth, maxHeight, maxDepth, maxChild);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        qtree = new Qtree(0, Vector2.zero, maxWidth, maxHeight, maxDepth, maxChild);
        for (int i = 0; i < baseUnits.Count; i++)
        {
            //qtree.Remove(qtree, baseUnits[i]);
            qtree.Insert(qtree, baseUnits[i]);

        }
        //Debug.Log(baseUnits.Count);
        DrawLine(qtree);
    }
    public void DrawLine(Qtree q)
    {
        Vector2 v1 = new Vector2(q.width / 2, q.height / 2);
        Vector2 v2 = new Vector2(v1.x, -v1.y);
        Vector2 v3 = new Vector2(-v1.x, v1.y);
        Vector2 v4 = new Vector2(-v1.x, -v1.y);

        v1 += q.center; v2 += q.center; v3 += q.center; v4 += q.center;

        Color c = Color.green;
        Debug.DrawLine(v1, v2, c);
        Debug.DrawLine(v2, v4, c);
        Debug.DrawLine(v3, v4, c);
        Debug.DrawLine(v3, v1, c);

        if (q.isLeaf) return;
        else for (int i = 0; i < 4; i++) DrawLine(q.childQtrees[i]);
    }
    public BaseUnit FindClosestTarget(Vector3 pos, float atttackRange)
    {
        List<BaseUnit> targets = FindTargets(pos, atttackRange);
        float minDis = float.MaxValue;
        BaseUnit target = null;
        foreach (var b in targets)
        {
            float dis = Vector2.Distance(pos, b.transform.position);
            if (dis < minDis)
            {
                target = b;
                minDis = dis;
            }
        }
        return target;
    }
    public BaseUnit FindClosestTarget(Vector3 pos, float atttackRange, Type type)
    {
        List<BaseUnit> targets = FindTargets(pos, atttackRange, type);
        float minDis = float.MaxValue;
        BaseUnit target = null;
        foreach (var b in targets)
        {
            float dis = Vector2.Distance(pos, b.transform.position);
            if (dis < minDis)
            {
                target = b;
                minDis = dis;
            }
        }
        return target;
    }
    public BaseUnit FindClosestTarget(Vector3 pos, float atttackRange, Type type, List<BaseUnit> excludedBaseUnits)
    {
        List<BaseUnit> targets = FindTargets(pos, atttackRange, type);
        float minDis = float.MaxValue;
        BaseUnit target = null;
        foreach (var b in targets)
        {
            if (excludedBaseUnits.Contains(b)) continue;
            float dis = Vector2.Distance(pos, b.transform.position);
            if (dis < minDis)
            {
                target = b;
                minDis = dis;
            }
        }
        return target;
    }
    public BaseUnit FindClosestTarget(Vector3 pos, float atttackRange, List<BaseUnit> excludedBaseUnits)
    {
        List<BaseUnit> targets = FindTargets(pos, atttackRange);
        float minDis = float.MaxValue;
        BaseUnit target = null;
        foreach (var b in targets)
        {
            if (excludedBaseUnits.Contains(b)) continue;
            float dis = Vector2.Distance(pos, b.transform.position);
            if (dis < minDis)
            {
                target = b;
                minDis = dis;
            }
        }
        return target;
    }
    public List<BaseUnit> FindTargets(Vector3 pos, float attackRange)
    {
        List<BaseUnit> targets = new List<BaseUnit>();
        QtreeFindTargets(qtree, pos, attackRange, targets);
        return targets;
    }
    public List<BaseUnit> FindTargets(Vector3 pos, float attackRange, Type type)
    {
        List<BaseUnit> targets = new List<BaseUnit>();
        QtreeFindTargets(qtree, pos, attackRange, targets);
        targets.RemoveAll(n => !type.IsInstanceOfType(n));
        return targets;
    }
    public List<BaseUnit> FindTargets(Vector3 pos, float attackRange, Type type, List<BaseUnit> excludedBaseUnits)
    {
        List<BaseUnit> targets = new List<BaseUnit>();
        QtreeFindTargets(qtree, pos, attackRange, targets);
        targets.RemoveAll(n => !type.IsInstanceOfType(n) || excludedBaseUnits.Contains(n));
        return targets;
    }
    private void QtreeFindTargets(Qtree q, Vector3 pos, float attackRange, List<BaseUnit> targets)
    {
        if (q.isLeaf)
        {
            foreach (var b in q.childList)
            {
                if (targets.Contains(b)) break;
                foreach (var c in b.clickCircles)
                {
                    if (Vector2.Distance(b.transform.position + new Vector3(c.x, c.y), pos) <= attackRange + c.radius)
                    {
                        targets.Add(b);
                        break;
                    }
                }
                foreach (var r in b.clickRectangles)
                {
                    if (MathEx.IsRectangleOverCircle(b.transform.position + new Vector3(r.x, r.y), r.width, r.height, pos, attackRange))
                    {
                        targets.Add(b);
                        break;
                    }
                }
            }

        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                Qtree t = q.childQtrees[i];
                if (MathEx.IsRectangleOverCircle(t.center, t.width, t.height, pos, attackRange))
                {
                    QtreeFindTargets(t, pos, attackRange, targets);
                }
            }
        }
    }
    public void AddBaseUnit(BaseUnit b)
    {
        baseUnits.Add(b);
        qtree.Insert(qtree, b);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(maxWidth, maxHeight, 0f));
    }
}
