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

    private Allocation allocation; //호스트 할당 정보
    private string joinCode;
    private const int maxConnection = 20;//최대 연결 수
    private const string GameSceneName = "Play";

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

        //네트워크 매니저의 트랜스포트를 가져옴  
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //RelayServerData 생성
        //RelayServerData relayServerData = new RelayServerData(allocation, "udp");
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls"); //for security

        //트랜스포트에 RelayServerData 설정
        transport.SetRelayServerData(relayServerData);


        //로비 생성
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions(); //로비 옵션 생성
            options.IsPrivate = false; //공개 로비
            options.Data = new Dictionary<string, DataObject>() //데이터 설정
            {
                { 
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )

                }
            };

            string playerName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Unknown");

            // 로비생성 시간이 길어질 경우 대비 
            //Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("Game Lobby", maxConnection, options); 
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}(Lobby)", maxConnection, options);

            lobbyID = lobby.Id;
            //15초 대기로 하트비트 메시지 전송    
            HostSingleton.Instance.StartCoroutine(ReadyLobby(15));
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }


        NetworkServer  = new NetworkServer(NetworkManager.Singleton); //서버 생성

        //************연결 데이터 설정************
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Missing name"),
            
            userAuthId = AuthenticationService.Instance.PlayerId

        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        //네트웍 매니져의 커넥션 데이터에 페이로드 설정
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        //************연결 데이터 설정************


        //네트웍 메니져 호스트 시작  
        NetworkManager.Singleton.StartHost();

        //게임 씬 로드
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single); 
    }

    private IEnumerator ReadyLobby(float waitTime)
    {
        //로비에 대한 Heartbeat 메시지 전송
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(waitTime);

        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return wait;

        }
              
       
    }

    public async void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(ReadyLobby));

        if(!string.IsNullOrEmpty(lobbyID))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(lobbyID);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            lobbyID = string.Empty;
        }

        NetworkServer?.Dispose();
    }
}
