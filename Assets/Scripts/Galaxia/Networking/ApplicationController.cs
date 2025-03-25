using System;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;

    private async void  Start()
    {
        DontDestroyOnLoad(gameObject);

        //for dedicated server  
       await  LaunchMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);

    }

    //��������Ƽ�� �������� Ŭ���̾�Ʈ���� ����    
    private async Task LaunchMode(bool isDedicateServer)
    {

        if(isDedicateServer) 
        {
            Debug.Log("Dedicated Server");
            ServerSingleton serverSingleton = Instantiate(serverPrefab);

            await serverSingleton.CreateServer();
            await serverSingleton.ServerGameManager.StartGameServerAsync();

        }
        else
        {
            Debug.Log("Client server");

            //ȣ��Ʈ ���� ����    
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            //Ŭ���̾�Ʈ ����
            ClientSingleton clientSingleton = Instantiate(clientPrefab);

            //await clientSingleton.CreateClient();
            bool authenticated  = await clientSingleton.CreateClient();

            

            //���� ������ �޴� ����
            if (authenticated)
            {
               
                clientSingleton.ClientGameManager.StartMenu();
            }
        }
    }
   
}
