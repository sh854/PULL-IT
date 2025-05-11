using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{

    public int initialEnemyCount = 30;
    public float spawnInterval = 5.0f;
    public int maxEnemyCount = 50;
    float lastSpawnTime;

    public List<GameObject> ENEMIES = new List<GameObject>();

    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public GameObject prefab4;

    public float probabilityPrefab1 = 0.25f;
    public float probabilityPrefab2 = 0.35f;
    public float probabilityPrefab3 = 0.2f;
    public float probabilityPrefab4 = 0.2f;

    GameManager cm;

    void Start()
    {
        SpawnEnemies(initialEnemyCount,-1);
        lastSpawnTime = Time.time;
        cm = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (Time.time - lastSpawnTime >= spawnInterval && ENEMIES.Count < maxEnemyCount)
        {
            int enemyToSpawn = Random.Range(1, 4); 
            SpawnEnemies(enemyToSpawn,0.5f);
            lastSpawnTime = Time.time;

        }

    }

    void SpawnEnemies(int count,float range)
    {


        for (int i = 0; i < count; i++)
        {
            float randomValue = Random.value;

            GameObject selectedPrefab = null;

            switch (randomValue)
            {
                case float _ when randomValue < probabilityPrefab1:
                    selectedPrefab = prefab1;
                    break;
                case float _ when randomValue < probabilityPrefab1 + probabilityPrefab2:
                    selectedPrefab = prefab2;
                    break;
                case float _ when randomValue < probabilityPrefab1 + probabilityPrefab2 + probabilityPrefab3:
                    selectedPrefab = prefab3;
                    break;
                default:
                    selectedPrefab = prefab4;
                    break;
            }

            if (selectedPrefab != null)
            {
                Vector2 spawnPosition = Utility.CustomFunctions.GetRandomNavMeshPositionOutsideCamera(range);

                GameObject newEnemy = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, transform);
                ENEMIES.Add(newEnemy);
            }
        }


    }


}
