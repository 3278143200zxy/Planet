using System.Collections.Generic;

public class UnionFind<T>
{
    private Dictionary<T, T> parent = new Dictionary<T, T>();
    private Dictionary<T, int> rank = new Dictionary<T, int>();

    // 添加元素
    public void Add(T x)
    {
        if (!parent.ContainsKey(x))
        {
            parent[x] = x;
            rank[x] = 0;
        }
    }

    // 查找根节点（路径压缩）
    public T Find(T x)
    {
        if (!parent.ContainsKey(x))
            Add(x);

        if (!parent[x].Equals(x))
            parent[x] = Find(parent[x]);

        return parent[x];
    }

    // 合并两个集合（按秩）
    public void Union(T a, T b)
    {
        T rootA = Find(a);
        T rootB = Find(b);

        if (rootA.Equals(rootB))
            return;

        if (rank[rootA] < rank[rootB])
            parent[rootA] = rootB;
        else if (rank[rootA] > rank[rootB])
            parent[rootB] = rootA;
        else
        {
            parent[rootB] = rootA;
            rank[rootA]++;
        }
    }

    // 获取所有根节点（所有集合的代表）
    public HashSet<T> GetRoots()
    {
        HashSet<T> roots = new HashSet<T>();
        var keys = new List<T>(parent.Keys); // 复制 Keys 避免遍历时修改字典
        foreach (var x in keys)
        {
            roots.Add(Find(x));
        }
        return roots;
    }

    // 获取集合数量
    public int GetSetCount()
    {
        return GetRoots().Count;
    }

    // 获取每个集合的成员
    public Dictionary<T, List<T>> GetGroups()
    {
        Dictionary<T, List<T>> groups = new Dictionary<T, List<T>>();
        var keys = new List<T>(parent.Keys); // 复制 Keys 避免遍历时修改字典

        foreach (var x in keys)
        {
            T root = Find(x);

            if (!groups.ContainsKey(root))
                groups[root] = new List<T>();

            groups[root].Add(x);
        }

        return groups;
    }

    // 判断两个元素是否属于同一个集合
    public bool Connected(T a, T b)
    {
        return Find(a).Equals(Find(b));
    }
}