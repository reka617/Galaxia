using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable
{

    private Allocation allocation; //ȣ��Ʈ �Ҵ� ����
    private string joinCode;
    private const int maxConnection = 20;//�ִ� ���� ��
    private const string GameSceneName = "GalaxiaPlay";

    private string lobbyID;

    //public NetworkServer networkServer;  //  
    public NetworkServer NetworkServer { get; private set; } //  

    public async Task StartHostAsync()
    {
        try
        {
            allocation =  await Relay.Instance.CreateAllocationAsync(maxConnection);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            //for client..
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code : " + joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        //��Ʈ��ũ �Ŵ����� Ʈ������Ʈ�� ������  
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //RelayServerData ����
        //RelayServerData relayServerData = new RelayServerData(allocation, "udp");
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls"); //for security

        //Ʈ������Ʈ�� RelayServerData ����
        transport.SetRelayServerData(relayServerData);


        //�κ� ����
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions(); //�κ� �ɼ� ����
            options.IsPrivate = false; //���� �κ�
            options.Data = new Dictionary<string, DataObject>() //������ ����
            {
                { 
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )

                }
            };

            string playerName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Unknown");

            // �κ���� �ð��� ����� ��� ��� 
            //Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("Game Lobby", maxConnection, options); 
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}(Lobby)", maxConnection, options);

            lobbyID = lobby.Id;
            //15�� ���� ��Ʈ��Ʈ �޽��� ����    
            HostSingleton.Instance.StartCoroutine(ReadyLobby(15));
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }


        NetworkServer  = new NetworkServer(NetworkManager.Singleton); //���� ����

        //************���� ������ ����************
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Missing name"),
            
            userAuthId = AuthenticationService.Instance.PlayerId

        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        //��Ʈ�� �Ŵ����� Ŀ�ؼ� �����Ϳ� ���̷ε� ����
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        //************���� ������ ����************


        //��Ʈ�� �޴��� ȣ��Ʈ ����  
        NetworkManager.Singleton.StartHost();

        //���� �� �ε�
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single); 
    }

    private IEnumerator ReadyLobby(float waitTime)
    {
        //�κ� ���� Heartbeat �޽��� ����
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(waitTime);

        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return wait;

        }
              
       
    }

    public async void Dispose()
    {
        if (!string.IsNullOrEmpty(lobbyID)) return;

        HostSingleton.Instance.StopCoroutine(nameof(ReadyLobby));
        
        try
        {
            await Lobbies.Instance.DeleteLobbyAsync(lobbyID);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        lobbyID = string.Empty;
        
        NetworkServer?.Dispose();
    }
}
