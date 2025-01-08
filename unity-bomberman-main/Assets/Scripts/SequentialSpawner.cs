using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialSpawner : MonoBehaviour
{
    public List<Transform> spawnPoints; // ������ ����� ������
    public GameObject prefabToSpawn; // ������ ��� ������
    public float spawnInterval = 1f; // �������� ����� ��������

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
                Instantiate(prefabToSpawn, point.position, Quaternion.identity); // ������� ������
                Debug.Log($"Prefab spawned at: {point.name}");
                prefabToSpawn.transform.localScale = Vector3.one * DynamicReferenceResolution.ScaleFactor;
                yield return new WaitForSeconds(spawnInterval); // ��� ����� ��������� �������

            }
        }

        Debug.Log("��� ������ ����������!");
    }

    public void StartSpawn()
    {
        if (spawnPoints.Count > 0 && prefabToSpawn != null)
        {
            StartCoroutine(SpawnPrefabsSequentially());
        }
        else
        {
            Debug.LogError("������ ����� ��� ������ �� ����������!");
        }
    }
}

