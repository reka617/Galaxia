using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("Projectile Settings")] 
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private InputReader inputReader;

    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")] 
    [SerializeField] private float projectileSpeed = 10f;

    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;

    [SerializeField] private int costToFire;
    [SerializeField] private ItemWallet itemWallet;
    
    private bool shouldFire;
    //private float prevFireTime;
    private float timer;
    private float muzzleFlashTimer;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void Update()
    {
        if (muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }
        
        if (!IsOwner) return;

        if (timer > 0) timer -= Time.deltaTime;
        if (!shouldFire) return;
        
        //if(Time.time < (1/fireRate) + prevFireTime) return;
        if (timer > 0) return;

        if (itemWallet.golds.Value < costToFire) return;

        //서버쪽으로 Fire이벤트 전송
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        //클라이언트도 Fire이벤트 진행
        //SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);

        //prevFireTime = Time.time;

        timer = 1 / fireRate;
    }

    private void SpawnDummyProjectile(Vector3 spawnPosition, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;
        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPosition, Quaternion.identity);

        projectileInstance.transform.up = direction;
        
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigids))
        {
            rigids.velocity = rigids.transform.up * projectileSpeed;
        }
    }
    

    private void HandlePrimaryFire(bool isFiring)
    {
        this.shouldFire = isFiring;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPosition, Vector3 direction)
    {
        if (itemWallet.golds.Value < costToFire) return;

        itemWallet.SpendGold(costToFire);
        
        GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPosition, Quaternion.identity);

        projectileInstance.transform.up = direction;
        
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        //발사체의 소유자 설정
        if (projectileInstance.TryGetComponent<DamageTarget>(out DamageTarget netObject))
        {
            netObject.SetOwner(OwnerClientId);
        }
        
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigids))
        {
            rigids.velocity = rigids.transform.up * projectileSpeed;
        }
        
        SpawnDummyProjectileClientRpc(spawnPosition, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPosition, Vector3 direction)
    {
        //if (!IsOwner) return;
        
        SpawnDummyProjectile(spawnPosition, direction);
    }
}
