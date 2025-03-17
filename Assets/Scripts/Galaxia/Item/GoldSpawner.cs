using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GoldSpawner : NetworkBehaviour
{
    [SerializeField] private RespawnGold goldObj;
    [SerializeField] private int maxGolds = 10;
    [SerializeField] private int goldsValue = 10;

    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;

    [SerializeField] private LayerMask layerMask;

    private float goldRadius;

    private Collider2D[] goldBuffer = new Collider2D[1];

    private void SpawnGold()
    {
        RespawnGold goldInstance = Instantiate(goldObj, GetSpawnPoint(), Quaternion.identity);
        
        goldInstance.SetValue(goldsValue);
        
        goldInstance.GetComponent<NetworkObject>().Spawn();

        goldInstance.OnGoldCollected += HandleGoldCollected;
    }

    private void HandleGoldCollected(RespawnGold gold)
    {
        gold.transform.position = GetSpawnPoint();
        gold.Reset();
    }

    private Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;

        while (true)
        {
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);

            Vector2 spawnPoint = new Vector2(x, y);
            //콜라이더에 서클이 있는지 확인
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, goldRadius, goldBuffer, layerMask);

            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        goldRadius = goldObj.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < maxGolds; i++)
        {
            SpawnGold();
        }
    }
    
}
