using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;

public class RespawnHandler : NetworkBehaviour
{
   [SerializeField] private NetworkObject playerPrefab;

   public override void OnNetworkSpawn()
   {
      if (!IsServer) return;

      AirPlayer[] players = FindObjectsOfType<AirPlayer>();
      foreach (AirPlayer player in players)
      {
         HandlePlayerSpawned(player);
      }

      AirPlayer.OnPlayerSpawned += HandlePlayerSpawned;
      AirPlayer.OnPlayerDespawned += HandlePlayerDespawned;
   }
   
   public override void OnNetworkDespawn()
   {
      if (!IsServer) return;

      AirPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
      AirPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
   }

   private void HandlePlayerSpawned(AirPlayer player)
   {
      //OnDeath가 일어나면 Health내부에서  HandlePlayerDie를 진행시킴
      player.Health.OnDeath += (Health) => HandlePlayerDie(player);
      
   }
   
   private void HandlePlayerDespawned(AirPlayer player)
   {
      player.Health.OnDeath -= (Health) => HandlePlayerDie(player);
   }

   private void HandlePlayerDie(AirPlayer player)
   {
      Destroy(player.gameObject);

      StartCoroutine(RespawnPlayer(player.OwnerClientId));
   }

   private IEnumerator RespawnPlayer(ulong ownerClientId)
   {
      yield return null; // 다음 프레임까지 대기

      NetworkObject playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
      
      playerInstance.SpawnAsPlayerObject(ownerClientId);
   }
}
