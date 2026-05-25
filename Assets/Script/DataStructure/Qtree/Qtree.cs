using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Qtree
{
    private int maxDepth;
    private int maxChild;

    private int[] dir1 = new int[] { 1, 1, -1, -1 };
    private int[] dir2 = new int[] { 1, -1, 1, -1 };

    public Vector2 center;
    public float width;
    public float height;

    public int depth;
    public bool isLeaf;
    public List<BaseUnit> childList;
    public Qtree[] childQtrees;
    public Qtree(int _depth, Vector2 _center, float _width, float _height, int _maxDepth, int _maxChild)
    {
        this.depth = _depth;
        this.center = _center;
        this.width = _width;
        this.height = _height;
        this.maxDepth = _maxDepth;
        this.maxChild = _maxChild;

        isLeaf = true;
        childList = new List<BaseUnit>();

    }
    public void Clear()
    {

    }

    public void Insert(Qtree q, BaseUnit baseUnit)//插入元素
    {
        if (q.isLeaf)//是叶节点则插入，超出则分裂
        {
            if (q.depth < maxDepth && q.childList.Count + 1 > maxChild)
            {
                Split(q);
                Insert(q, baseUnit);
            }
            else q.childList.Add(baseUnit);
        }
        else//不是叶节点则插到子节点
        {
            for (int i = 0; i < 4; i++)
            {
                Qtree t = q.childQtrees[i];
                if (IsBoxOverCircles(t.center, t.width, t.height, baseUnit.transform.position, baseUnit.clickCircles) || IsBoxOverBoxes(t.center, t.width, t.height, baseUnit.transform.position, baseUnit.clickRectangles))
                {
                    Insert(q.childQtrees[i], baseUnit);
                }
            }
        }

    }
    public void Split(Qtree q)//树的分裂
    {
        q.isLeaf = false;
        q.childQtrees = new Qtree[4];
        //初始化所有子节点
        for (int i = 0; i < 4; i++)
        {
            Vector2 boxCenter = new Vector2(dir1[i] * q.width / 4, dir2[i] * q.height / 4) + q.center;
            q.childQtrees[i] = new Qtree(q.depth + 1, boxCenter, q.width / 2, q.height / 2, maxDepth, maxChild);
        }
        //将单位移到子结点中
        foreach (var b in q.childList)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 boxCenter = new Vector2(dir1[i] * q.width / 4, dir2[i] * q.height / 4) + q.center;
                if (IsBoxOverCircles(boxCenter, q.width / 2, q.height / 2, b.transform.position, b.clickCircles) || IsBoxOverBoxes(boxCenter, q.width / 2, q.height / 2, b.transform.position, b.clickRectangles))
                {
                    Insert(q.childQtrees[i], b);
                }
            }
        }
        q.childList.Clear();
    }
    public void Remove(Qtree q, BaseUnit baseUnit)
    {
        // 如果是叶节点，则直接尝试移除
        if (q.isLeaf)
        {
            q.childList.Remove(baseUnit);
            return;
        }

        // 否则递归检查子节点
        for (int i = 0; i < 4; i++)
        {
            var child = q.childQtrees[i];
            if (child == null) continue;

            // 判断该单位是否可能在这个子节点范围内
            if (IsBoxOverCircles(child.center, child.width, child.height, baseUnit.transform.position, baseUnit.clickCircles)
                || IsBoxOverBoxes(child.center, child.width, child.height, baseUnit.transform.position, baseUnit.clickRectangles))
            {
                Remove(child, baseUnit);
            }
        }

        // 同时从当前节点的 childList 中移除（防止上层残留）
        q.childList.Remove(baseUnit);

        // 删除后尝试合并节点（防止碎片化）
        TryMerge(q);
    }

    private void TryMerge(Qtree q)
    {
        if (q.isLeaf || q.childQtrees == null)
            return;

        int totalCount = 0;
        bool allLeaf = true;

        for (int i = 0; i < 4; i++)
        {
            var child = q.childQtrees[i];
            if (child == null)
                continue;

            if (!child.isLeaf)
            {
                allLeaf = false;
                break;
            }

            totalCount += child.childList.Count;
        }

        // 所有子节点都是叶节点 且 总数不超过 maxChild，则合并
        if (allLeaf && totalCount <= maxChild)
        {
            for (int i = 0; i < 4; i++)
            {
                q.childList.AddRange(q.childQtrees[i].childList);
                q.childQtrees[i] = null;
            }
            q.childQtrees = null;
            q.isLeaf = true;
        }
    }
    public bool IsBoxOverCircles(Vector2 boxCenter, float width, float height, Vector2 circleCenterOffset, List<Circle> circles)//判断矩形与多个圆是否相交
    {
        foreach (var c in circles)
        {
            Vector2 realCircleCenter = circleCenterOffset + new Vector2(c.x, c.y);
            if (MathEx.IsRectangleOverCircle(boxCenter, width, height, realCircleCenter, c.radius)) return true;
        }
        return false;
    }
    public bool IsBoxOverBoxes(Vector2 boxCenter, float width, float height, Vector2 boxCenterOffset, List<Rectangle> rectangles)
    {
        foreach (var r in rectangles)
        {
            Vector2 realBoxCenter = boxCenterOffset + new Vector2(r.x, r.y);
            if (MathEx.IsRectangleOverRectangle(boxCenter, width, height, realBoxCenter, r.width, r.height)) return true;
        }
        return false;
    }
    public bool IsCirleOverCircle(Vector2 c1, float r1, Vector2 c2, float r2)//判断圆与圆是否相交
    {
        float dis = Vector2.Distance(c1, c2);
        return dis < r1 + r2;
    }

}