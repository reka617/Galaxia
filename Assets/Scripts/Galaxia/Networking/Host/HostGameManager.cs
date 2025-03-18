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
    private Allocation allocation; // 호스트 할당 정보
    private string joinCode; //릴레이는 최적의 조인을 위한 코드정보를 받아옴
    private const int maxConnection = 20; // 최대 연결수
    private const string GameSceneName = "GalaxiaPlay";

    private string lobbyID;

    private NetworkServer networkServer;
    
    // public async Task InitAsync()
    // {
    //     //플레이어 인증처리
    // }

    public async Task StartHostAsync()
    {
        try
        {
            //최대 연결수 만큼 들어갈 수 있는 방을 만든다
            allocation = await Relay.Instance.CreateAllocationAsync(maxConnection); 
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            //할당받은 방에 대한 접근하기 위한 코드를 받아옴
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code : " + joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        //네트워크 매니저의 트랜스포트를 가져옴
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        //로비생성
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false; // 로비의 공개여부 설정
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode)
                }
            };

            string playerName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Unknown");
            
            //로비 생성 딜레이 대비
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}(Lobby)", maxConnection, options);

            lobbyID = lobby.Id;
            
            //15초 대기로 하트비트 메세지 전송
            HostSingleton.Instance.StartCoroutine(ReadyLobby(15));

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }
        
        //RelayServerData 생성
        //RelayServerData relayServerData = new RelayServerData(allocation, "udp");
        //보안을 위해서 dtls 사용
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls"); 
        
        //트랜스포트에 RelayServerData 설정
        transport.SetRelayServerData(relayServerData);
        //위의 3문구를 통해 어디가 최적으로 릴레이시킬 수 있는지 설정한 것

        networkServer = new NetworkServer(NetworkManager.Singleton); // 서버 생성

        #region SettingConnectionData

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Missing name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        
        #endregion
        
        //네트워크 매니저 호스트 시작
        NetworkManager.Singleton.StartHost();
        //게임 씬 로드
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator ReadyLobby(float waitTime) //로비가 제대로 생성됬는지 확인
    {
        Debug.Log("Ready Lobby : " + lobbyID);
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

        if (!string.IsNullOrEmpty(lobbyID))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(lobbyID);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
        
        networkServer?.Dispose();
    }
}
