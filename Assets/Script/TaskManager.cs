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
    public Dictionary<Task, List<Creature>> taskToCreatures = new Dictionary<Task, List<Creature>>();

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

    }
    public void AddTask(Task t)
    {
        tasks.Add(t);
        taskToCreatures[t] = new List<Creature>();
        foreach (Creature creature in creatures)
        {
            creature.FindTask();
        }
    }
    public void RemoveTask(Task t)
    {
        if (!tasks.Contains(t)) return;
        tasks.Remove(t);
        foreach (Creature c in taskToCreatures[t])
        {
            c.CancelTask();
        }
        taskToCreatures.Remove(t);
    }
}
