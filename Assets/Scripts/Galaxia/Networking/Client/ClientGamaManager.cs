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

public class ClientGamaManager : IDisposable
{    
    private JoinAllocation joinAllocation;  

    private const string MenuSceneName = "Start";


    private NetworkClient networkClient;    

    public async Task<bool> InitAsync()
    {
        
        //UnityServices.InitializeAsync()를 호출하여 Unity 서비스를 초기화.
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);


        AuthState authState = await AuthenticateWrapper.DoAuth();
        if (authState == AuthState.Authenticated)
        {
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

        //네트워크 매니저의 트랜스포트를 가져옴  
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //RelayServerData 생성
        //RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp"); 
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls"); //for security

        //트랜스포트에 RelayServerData 설정
        transport.SetRelayServerData(relayServerData);

        //************연결 데이터 설정************
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Missing name"),

            //AuthenticationService 인스턴스의 PlayerId를 가져와서 설정
            userAuthId = AuthenticationService.Instance.PlayerId 
        };

        string payload = JsonUtility.ToJson(userData);
        //byte로 변환
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

        //네트웍 매니져의 커넥션 데이터에 페이로드 설정
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        //************연결 데이터 설정************

        //네트웍 메니져 호스트 시작  
        NetworkManager.Singleton.StartClient();

    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}
