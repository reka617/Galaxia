using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
   private NetworkManager networkManager;
   
   //클라이언트 ID와 인증 ID 매핑
   private Dictionary<ulong, string> clientToAuth = new Dictionary<ulong, string>();
   private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
   
   //ID정보 캐싱
   private Dictionary<ulong, string> clientAuthCache = new Dictionary<ulong, string>();

   public NetworkServer (NetworkManager networkManager)
   {
      this.networkManager = networkManager;
      // 연결 승인 콜백 등록
      networkManager.ConnectionApprovalCallback += ApprovalCheck;

      networkManager.OnServerStarted += OnNetworkReady;
   }
   
   private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
   {
      string payload = System.Text.Encoding.UTF8.GetString(request.Payload); // 요청 데이터를 문자열로 변환
      UserData userData = JsonUtility.FromJson<UserData>(payload); // 요청 데이터를 UserData객체로 변환
      
      Debug.Log("ApprovalCheck: " + userData.userName);

      clientToAuth[request.ClientNetworkId] = userData.userAuthId; // 클라이언트 ID와 인증 ID 매핑
      authIdToUserData[userData.userAuthId] = userData; // 인증 ID와 클라이언트 ID 매핑
      
      response.Approved = true; // 연결 승인

      //스폰 위치 설정
      response.Position = SpawnPoint.GetRandomSpawnPos();
      response.Rotation = Quaternion.identity;
      
      response.CreatePlayerObject = true; // 플레이어 오브젝트 생성, 이부분이 빠질경우 정상적으로 게임씬으로 가더라도 플레이어를 확인 못할수도있음
   }
   
   private void OnNetworkReady()
   {
      networkManager.OnClientDisconnectCallback += OnClientDisconnect;
   }

   private void OnClientDisconnect(ulong clientId)
   {
      if (clientToAuth.TryGetValue(clientId, out string authId))
      {
         // 연결 종료 시 매핑 정보 삭제
         clientToAuth.Remove(clientId);
         authIdToUserData.Remove(authId);
      }
   }

   public UserData GetUserDataByClientId(ulong clientId)
   {
      if (clientToAuth.TryGetValue(clientId, out string authId))
      {
         if (authIdToUserData.TryGetValue(authId, out UserData userData))
         {
            return userData;
         }
         return null;
      }
   
      return null;
   }
   

   public void Dispose()
   {
      if (networkManager == null) return;
      //연결중인 콜백들 제거
      networkManager.ConnectionApprovalCallback -= ApprovalCheck;
      networkManager.OnServerStarted -= OnNetworkReady;
      networkManager.OnClientDisconnectCallback -= OnClientDisconnect;

      // 서버가 연결 중일 때
      if (networkManager.IsListening)
      {
         networkManager.Shutdown();
      }
   }
   
   // //클라이언트 ID 캐싱
   // //매핑 정보를 캐시에 저장함
   // public void CacheClientAuth(ulong clientId)
   // {
   //    if (clientToAuth.TryGetValue(clientId, out string authId))
   //    {
   //       clientAuthCache[clientId] = authId;
   //       Debug.Log($"Cached auth mapping: clientId {clientId} -> authId {authId}");
   //    }
   // }
   //
   // // 캐시에서 매핑 정보를 복원하는 메서드
   // public void RestoreClientAuth(ulong clientId)
   // {
   //    if (clientAuthCache.TryGetValue(clientId, out string authId))
   //    {
   //       // 기존 매핑이 없는 경우에만 복원
   //       if (!clientToAuth.ContainsKey(clientId))
   //       {
   //          clientToAuth[clientId] = authId;
   //          Debug.Log($"Restored auth mapping: clientId {clientId} -> authId {authId}");
   //       }
   //    }
   // }
}
