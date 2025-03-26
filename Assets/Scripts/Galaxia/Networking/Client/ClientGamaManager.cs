using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;

public class ClientGameManager : IDisposable
{   
    //클라이언트는 joinAllocation 객체를 사용
    private JoinAllocation joinAllocation;  

    private const string MenuSceneName = "GalaxiaStart";


    private NetworkClient networkClient;
    private MatchplayMatchmaker matchmaker;
    private UserData userData;

    public async Task<bool> InitAsync()
    {
        
        //플레이어 인증처리
        
        //UnityServices.InitializeAsync()를 호출 -> UGS 초기화
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);
        matchmaker = new MatchplayMatchmaker();
        
        AuthState authState = await AuthenticateWrapper.DoAuth();
        if (authState == AuthState.Authenticated)
        {
            userData = new UserData
            {
                userName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Missing name"),
                //AuthenticationService 인스턴스의 PlayerId를 가져와서 설정
                userAuthId = AuthenticationService.Instance.PlayerId,
                userGamePreferences = new GameInfo
                {
                map = Map.Default,
                gameMode = GameMode.Default,
                gameQue = GameQue.Solo
                }
            };
            Debug.Log("Auth Success");
            return true;
        }

        Debug.Log("Auth Fail");
        return false;
    }

    public void StartMenu()
    {
        Debug.Log("call start scene");
        SceneManager.LoadScene(MenuSceneName);

    }

    public  async Task StartClientAsync(string codeText)
    {
        try
        {
            joinAllocation = await Relay.Instance.JoinAllocationAsync(codeText);

        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp"); 
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls"); //for security
        transport.SetRelayServerData(relayServerData);

        ConnectClient();
    }
    private void ConnectClient()
    {
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        
        //네트워크매니저의 커넥션 데이터에 페이로드 설정
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        
        //네트워크 매니저 클라이언트 시작
        NetworkManager.Singleton.StartClient();
    }
    
    public async void MatchMakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        if (matchmaker.IsMatchmaking) return;

        MatchmakerPollingResult matchResult = await GetMatchAsync();
        onMatchmakeResponse?.Invoke(matchResult);
    }

    public async Task CancelMatchMaking()
    {
        await matchmaker.CancelMatchmaking();
    }

    //매칭 결과를 리턴
    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult matchmakingResult = await matchmaker.Matchmake(userData);

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            //서버 연결
            StartClient(matchmakingResult.ip, matchmakingResult.port);
        }

        return matchmakingResult.result;
    }
    
    public void StartClient(string ip, int port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        
        ConnectClient();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}
