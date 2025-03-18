using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
   private NetworkManager networkManager;
   private const string MenuScenename = "GalaxiaStart";

   public NetworkClient(NetworkManager networkManager)
   {
      this.networkManager = networkManager;
      // 연결 승인 콜백 등록
      networkManager.OnClientDisconnectCallback += OnClientDisconnect;
   }

   private void OnClientDisconnect(ulong clientId)
   {
      //연결 중일떄
      if (clientId != 0 && clientId != networkManager.LocalClientId) return;
      
      //메뉴 씬이 아닐때
      if (SceneManager.GetActiveScene().name != MenuScenename)
      {
         SceneManager.LoadScene(MenuScenename);
      }
      
      //서버가 연결을 끊었을 떄
      if (networkManager.IsConnectedClient)
      {
         networkManager.Shutdown();
      }
   }
   
   public void Dispose()
   {
      networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
   }
}
