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
    
    [Header("Fall speed progression")]
    public float initialFallSpeed = 1.0f;
    public float fallAcceleration = 0.1f;
    public float maxFallSpeed = 10.0f; 
    private float currentFallSpeed = 0f;

    [Header("Spawn Control")]
    public float minSpawnInterval = 0.5f;  
    private float currentSpawnInterval;

    void OnEnable()
    {
        currentFallSpeed = initialFallSpeed;
        currentSpawnInterval = spawnInterval;
        StartCoroutine(SpawnRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            SpawnSingleMeteor();
            
            currentFallSpeed = Mathf.Min(currentFallSpeed + (fallAcceleration * spawnInterval), maxFallSpeed);
            
            currentSpawnInterval = Mathf.Max(spawnInterval - (Time.timeSinceLevelLoad * 0.01f), minSpawnInterval);
            
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnSingleMeteor()
    {
        if (meteorPrefab == null) return;
        float x = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPosition = new Vector3(x, spawnHeight, spawnZ);
        GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
        var meteorScript = meteor.GetComponent<Meteor>();
        if (meteorScript != null)
        {
            meteorScript.directFallSpeed = currentFallSpeed;
        }
    }
}