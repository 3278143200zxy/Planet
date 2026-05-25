using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
class PriorityQueue<T>
{
    private SortedList<float, Queue<T>> elements = new SortedList<float, Queue<T>>();

    // Enqueue an item with a priority
    public void Enqueue(T item, float priority)
    {
        if (!elements.ContainsKey(priority))
        {
            elements[priority] = new Queue<T>();
        }
        elements[priority].Enqueue(item);
    }

    // Dequeue the item with the highest priority (smallest key)
    public T Dequeue()
    {
        while (elements.Count > 0)
        {
            var firstPriority = elements.Keys[0];
            var queue = elements[firstPriority];

            if (queue.Count == 0)
            {
                elements.Remove(firstPriority); // 直接移除空队列
                continue;
            }

            var item = queue.Dequeue();

            if (queue.Count == 0)
            {
                elements.Remove(firstPriority); // 再次移除空队列
            }

            return item;
        }

        throw new InvalidOperationException("Queue is empty");
    }

    // Check if the queue contains an item
    public bool Contains(T item)
    {
        foreach (var queue in elements.Values)
        {
            if (queue.Contains(item))
                return true;
        }
        return false;
    }

    // Update the priority of an item
    public bool UpdatePriority(T item, float newPriority)
    {
        foreach (var priority in elements.Keys.ToList()) // 修改点：避免遍历时修改
        {
            var queue = elements[priority];
            if (queue.Contains(item))
            {
                // 过滤掉 item
                var newQueue = new Queue<T>(queue.Where(x => !EqualityComparer<T>.Default.Equals(x, item)));

                if (newQueue.Count > 0)
                    elements[priority] = newQueue;
                else
                    elements.Remove(priority); // 修改点：移除空队列

                Enqueue(item, newPriority);
                return true;
            }
        }
        return false;
    }

    // Count of elements in the queue
    public int Count => elements.Values.Sum(queue => queue.Count);
}
