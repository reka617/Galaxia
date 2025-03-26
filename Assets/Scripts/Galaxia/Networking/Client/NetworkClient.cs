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
        // ���� ���� �ݹ� ���
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;

        
    }


    private void OnClientDisconnect(ulong clientId)
    {
        //�������϶�
        if(clientId != 0 && clientId != networkManager.LocalClientId)
        {
           return;
        }

        //�������϶� 
        if (SceneManager.GetActiveScene().name != MenuScenename)
        {
            SceneManager.LoadScene(MenuScenename);
        }

        //������ ������ ��������    
        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();//���� ����
        }

    }

    public void Dispose()
    {
        //���� ���� �ݹ� ����
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect; 
    }
}
