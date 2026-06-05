using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public Enemy birdPrefab;
    public float spawnInterval;
    public float spawnTimer;
    private void Awake()
    {
        instance = this;
    }
    void Update()
    {
        spawnTimer += TimeManager.deltaTime;
        if (spawnTimer > spawnInterval)
        {
            spawnTimer -= spawnInterval;
            SpawnEnemy();
        }
    }
    public void SpawnEnemy()
    {
        Vector3 spawnPos = transform.position;
        Enemy bird = Instantiate(birdPrefab);
        bird.transform.position = spawnPos;
        bird.ChangeLayer(bird.gameObject, LayerMask.NameToLayer("0"));
    }
}
