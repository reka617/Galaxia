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
        
        //UnityServices.InitializeAsync()�� ȣ���Ͽ� Unity ���񽺸� �ʱ�ȭ.
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

        //��Ʈ��ũ �Ŵ����� Ʈ������Ʈ�� ������  
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //RelayServerData ����
        //RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp"); 
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls"); //for security

        //Ʈ������Ʈ�� RelayServerData ����
        transport.SetRelayServerData(relayServerData);

        //************���� ������ ����************
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NamePicker.PlayerNameKey, "Missing name"),

            //AuthenticationService �ν��Ͻ��� PlayerId�� �����ͼ� ����
            userAuthId = AuthenticationService.Instance.PlayerId 
        };

        string payload = JsonUtility.ToJson(userData);
        //byte�� ��ȯ
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

        //��Ʈ�� �Ŵ����� Ŀ�ؼ� �����Ϳ� ���̷ε� ����
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        //************���� ������ ����************

        //��Ʈ�� �޴��� ȣ��Ʈ ����  
        NetworkManager.Singleton.StartClient();

    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}
