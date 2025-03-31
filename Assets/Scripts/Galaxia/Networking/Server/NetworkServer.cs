using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;


public class NetworkServer : IDisposable
{
    
    private NetworkManager networkManager;

    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;

    public Action<string> OnClientLeft;

    // Ŭ���̾�Ʈ ID�� ���� ID ����
    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>(); 
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>(); 


    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
        // ���� ���� �ݹ� ���
        networkManager.ConnectionApprovalCallback += ApprovalCheck;

        networkManager.OnServerStarted += OnNetworkReady;


    }
    
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest req, 
        NetworkManager.ConnectionApprovalResponse res) //
    {
       
        string payload = System.Text.Encoding.UTF8.GetString(req.Payload); // ��û �����͸� ���ڿ��� ��ȯ
        UserData userData = JsonUtility.FromJson<UserData>(payload); // ��û �����͸� UserData ��ü�� ��ȯ
        
        Debug.Log("ApprovalCheck: " + userData.userName);

        clientIdToAuth[req.ClientNetworkId] = userData.userAuthId; // Ŭ���̾�Ʈ ID�� ���� ID ����
        authIdToUserData[userData.userAuthId] = userData; // ���� ID�� Ŭ���̾�Ʈ ID ����


        res.Approved = true; // ���� ����

        res.Position = SpawnPoint.GetRandomSpawnPos();
        res.Rotation = Quaternion.identity;

        res.CreatePlayerObject = true; // �÷��̾� ������Ʈ ����
    }

    private void OnNetworkReady()
    {
       networkManager.OnClientDisconnectCallback += OnClientDisconnect;

    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            // ���� ����� ���� ���� ����
            clientIdToAuth.Remove(clientId);
            
            OnUserLeft?.Invoke(authIdToUserData[authId]);
            
            authIdToUserData.Remove(authId);
            
            OnClientLeft?.Invoke(authId);
        }
    }

    public bool OpenConnection(string ip, int port)
    {
        UnityTransport transport = networkManager.gameObject.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        return networkManager.StartServer();
    }

    public void Dispose()
    {
       if( networkManager== null) return;
        // ���� ���� �ݹ� ����
        networkManager.ConnectionApprovalCallback -= ApprovalCheck; 
        networkManager.OnServerStarted -= OnNetworkReady;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;

        if(networkManager.IsListening)//������ �������϶�
        {
            networkManager.Shutdown();
        }
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if(authIdToUserData.TryGetValue(authId, out UserData userData))
            {
                return userData;
            }

            return null;
        }

        return null;
    }

}
