// MeteorSpawner.cs
using System.Collections;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab;
    public float spawnInterval = 1.0f;
    public float spawnXMin = -6f;
    public float spawnXMax = 6f;
    public float spawnHeight = 12f;
    public float spawnZ = 0f;
    public bool isSpawning = true;

    void OnEnable()
    {
        StartCoroutine(SpawnRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            SpawnSingleMeteor();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnSingleMeteor()
    {
        if (meteorPrefab == null) return;
        float x = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPosition = new Vector3(x, spawnHeight, spawnZ);
        Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
    }
}