using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
   [SerializeField] private AirPlayer playerPrefab;
   [SerializeField] private float keepGoldPercentage;

   public override void OnNetworkSpawn()
   {
      if (!IsServer) return;

      //AirPlayer[] players = FindObjectsOfType<AirPlayer>();

      AirPlayer[] players = FindObjectsByType<AirPlayer>(FindObjectsSortMode.None);
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
      int keepGolds = (int)(player.Wallet.golds.Value * (keepGoldPercentage / 100));
      ulong clientId = player.OwnerClientId;
      
      // //플레이어 파괴 전에 매핑 정보 캐시
      // if (HostSingleton.Instance?.HostGameManager?.NetworkServer != null)
      // {
      //    HostSingleton.Instance.HostGameManager.NetworkServer.CacheClientAuth(clientId);
      // }
      
      Destroy(player.gameObject);

      //StartCoroutine(RespawnPlayer(player.OwnerClientId));
      StartCoroutine(RespawnPlayer(player.OwnerClientId, keepGolds));
   }

   private IEnumerator RespawnPlayer(ulong ownerClientId, int keepGolds)
   {
      yield return null; // 다음 프레임까지 대기
   
      // //매핑 정보 복원
      // if (HostSingleton.Instance?.HostGameManager?.NetworkServer != null)
      // {
      //    HostSingleton.Instance.HostGameManager.NetworkServer.RestoreClientAuth(ownerClientId);
      // }
      //NetworkObject playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
      AirPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), quaternion.identity);
      
      playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
      playerInstance.Wallet.golds.Value += keepGolds;
   }
   
}
