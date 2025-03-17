using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    
    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        
        //Dedicated server
        await LaunchMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchMode(bool isDedicateServer)
    {
        if (isDedicateServer)
        {
            Debug.Log("Dedicated Server");
        }
        else
        {
            Debug.Log("Client");
            
            //호스트 먼저 생성
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();
            
            //클라이언트 생성
            ClientSingleton newClient = Instantiate(clientPrefab);
            bool Authenticated = await newClient.CreateClient();

            
            
            //인증 성공시 메뉴 시작
            if (Authenticated)
            {
                newClient.ClientGameManager.StartMenu();
            }
        }
    }
}
