using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TaskType
{
    CutTree,
    Build,
    MoveItem,
}
public class CreatureTaskNode
{
    public Creature creature;
    public float cost;

    public CreatureTaskNode(Creature creature, float cost)
    {
        this.creature = creature;
        this.cost = cost;
    }
}
public class Task
{
    public TaskType taskType;
    public BaseUnit[] baseUnits;
    public Task(TaskType taskType, BaseUnit[] baseUnits)
    {
        this.taskType = taskType;
        this.baseUnits = baseUnits;
    }
}
public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;

    public List<Task> tasks = new List<Task>();
    public List<Creature> creatures = new List<Creature>();
    public Dictionary<Task, List<CreatureTaskNode>> taskToCreatureTaskNodes = new Dictionary<Task, List<CreatureTaskNode>>();

    public Item item;

    private void Awake()
    {
        instance = this;

        creatures = new List<Creature>(FindObjectsOfType<Creature>());
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(tasks.Count);
    }
    public void AddTask(Task t)
    {
        if (t == null) return;
        tasks.Add(t);
        taskToCreatureTaskNodes[t] = new List<CreatureTaskNode>();
        foreach (Creature creature in creatures)
        {
            creature.FindTask();
        }
    }
    public void RemoveTask(Task t)
    {
        if (t == null && !tasks.Contains(t)) return;
        tasks.Remove(t);

        foreach (var creatureTaskNode in taskToCreatureTaskNodes[t])
        {
            creatureTaskNode.creature.CancelTask();
        }
        taskToCreatureTaskNodes.Remove(t);
    }
    public void DistributeTask(Task t)
    {
        foreach (var creature in creatures)
        {

        }

    }
}
