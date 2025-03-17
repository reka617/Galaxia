using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : MonoBehaviour
{
    //클라이언트는 joinAllocation 객체를 사용
    private JoinAllocation clientallocation;
    
    private const string MenuSceneName = "GalaxiaStart";
    public async Task<bool> InitAsync()
    {
        //플레이어 인증처리
        
        //UnityServices.InitializeAsync()를 호출 -> UGS 초기화
        await UnityServices.InitializeAsync();

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
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string inputCode)
    {
        try
        {
            clientallocation = await Relay.Instance.JoinAllocationAsync(inputCode);
        }
        catch (Exception e)
        {
           Debug.LogError(e);
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(clientallocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        #region SettingConnectionData

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Missing name")
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        
        //네트워크매니저의 커넥션 데이터에 페이로드 설정
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        #endregion
        
        //네트워크 매니저 클라이언트 시작
        NetworkManager.Singleton.StartClient();
    }
}
