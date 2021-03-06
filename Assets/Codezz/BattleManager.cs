using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BattleManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    [AssetsOnly]
    public GameObject[] enemies;

    [Tooltip("This is float to increment p� factor")]
    public float enemiesToSpawn;
    public int growthNum = 0;
    public float growthFactor = 1;
    public float secondsBetweenEnemySpawned = 0;

    [ShowInInspector]
    public static int enemiesRemaining = 0;

    IEnumerator Start()
    {
        enemiesRemaining = 0;
        while (true)
        {
            enemiesRemaining = 0;
            for (int i = 0; i < Mathf.FloorToInt(enemiesToSpawn); i++)
            {
                var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var newEnemy = Instantiate(enemies[Random.Range(0, enemies.Length)], spawnPoint.position, spawnPoint.rotation);

                enemiesRemaining++;
                yield return new WaitForSeconds(secondsBetweenEnemySpawned);
            }


            yield return new WaitUntil(() => enemiesRemaining <= 0);
            enemiesToSpawn *= growthFactor;
            enemiesToSpawn += growthNum;
            yield return null;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            foreach (var dood in GameObject.FindObjectsOfType<Dood>())
            {
                dood.OnHit(99999);
            }
        }
    }
}
