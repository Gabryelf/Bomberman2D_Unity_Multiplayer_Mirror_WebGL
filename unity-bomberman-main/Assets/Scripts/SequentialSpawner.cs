using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialSpawner : MonoBehaviour
{
    public List<Transform> spawnPoints; // Список точек спавна
    public GameObject prefabToSpawn; // Префаб для спавна
    public float spawnInterval = 1f; // Задержка между спавнами

    private void Start()
    {
        StartSpawn();
    }

    private IEnumerator SpawnPrefabsSequentially()
    {
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Instantiate(prefabToSpawn, point.position, Quaternion.identity); // Спавним префаб
                Debug.Log($"Prefab spawned at: {point.name}");
                prefabToSpawn.transform.localScale = Vector3.one * DynamicReferenceResolution.ScaleFactor;
                yield return new WaitForSeconds(spawnInterval); // Ждём перед следующим спавном

            }
        }

        Debug.Log("Все поинты обработаны!");
    }

    public void StartSpawn()
    {
        if (spawnPoints.Count > 0 && prefabToSpawn != null)
        {
            StartCoroutine(SpawnPrefabsSequentially());
        }
        else
        {
            Debug.LogError("Список точек или префаб не установлен!");
        }
    }
}

