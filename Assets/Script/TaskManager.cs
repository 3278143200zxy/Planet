using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TaskType
{
    None,
    CutTree,
    MineStone,
    Build,
    MoveItem,
    Craft,
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
[Serializable]
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

    public List<TaskType> sharedTaskTypes = new List<TaskType>();

    public Item item;

    public bool isCreatureFindTaskFrame = false;

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
        //if (taskToCreatureTaskNodes.Count > 0) Debug.Log(taskToCreatureTaskNodes[taskToCreatureTaskNodes.Keys.First()].Count());
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
        // isCreatureFindTaskFrame = true;

    }
    public void AddTaskWithoutCreatureFindTask(Task t)
    {
        if (t == null) return;
        tasks.Add(t);
        taskToCreatureTaskNodes[t] = new List<CreatureTaskNode>();
    }
    public void RemoveTask(Task t)
    {
        if (t == null || !tasks.Contains(t)) return;
        tasks.Remove(t);

        for (int i = 0; i < taskToCreatureTaskNodes[t].Count; i++)
        {
            taskToCreatureTaskNodes[t][i].creature.CancelTask();
        }
        taskToCreatureTaskNodes.Remove(t);
    }
    public void RemoveTaskWithoutCancelCreatureTask(Task t)
    {
        if (t == null || !tasks.Contains(t)) return;
        tasks.Remove(t);
        taskToCreatureTaskNodes.Remove(t);
    }
    public void RemoveTaskWithoutCreatureFindTask(Task t)
    {
        if (t == null || !tasks.Contains(t)) return;
        tasks.Remove(t);
        for (int i = 0; i < taskToCreatureTaskNodes[t].Count; i++)
        {
            taskToCreatureTaskNodes[t][i].creature.CancelTaskWithoutFindTask();
        }
        taskToCreatureTaskNodes.Remove(t);
    }
    private void LateUpdate()
    {

        if (isCreatureFindTaskFrame)
        {
            foreach (Creature creature in creatures)
            {
                creature.FindTask();
            }
            isCreatureFindTaskFrame = false;
        }

    }
}
