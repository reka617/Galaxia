using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager networkManager;
    private const string MenuScenename = "Start";
    
    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
        // 연결 승인 콜백 등록
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;

        
    }


    private void OnClientDisconnect(ulong clientId)
    {
        //연결중일때
        if(clientId != 0 && clientId != networkManager.LocalClientId)
        {
           return;
        }

        //게이중일때 
        if (SceneManager.GetActiveScene().name != MenuScenename)
        {
            SceneManager.LoadScene(MenuScenename);
        }

        //서버가 연결을 끊었을때    
        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();//연결 종료
        }

    }

    public void Dispose()
    {
        //연결 해제 콜백 제거
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect; 
    }
}
