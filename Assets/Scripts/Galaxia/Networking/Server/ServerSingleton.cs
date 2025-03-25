using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class ServerSingleton : MonoBehaviour
{
    private static ServerSingleton instance;
    
    //private HostGameManager hostGamaManager;
    public ServerGameManager ServerGameManager { get; private set; }

    public static ServerSingleton Instance
    {
        get
        {
            if(instance != null) return instance;

            instance = FindObjectOfType<ServerSingleton>();
            if(instance ==null)
            {
                Debug.LogError("ServerSingleton is not found");
                return null;
            }

            return instance;
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    //Ŭ���̾�Ʈ ����
    public async Task CreateServer()
    {
        await UnityServices.InitializeAsync();
        ServerGameManager = new ServerGameManager
        (
            ApplicationData.IP(),
            ApplicationData.Port(),
            ApplicationData.QPort(),
            NetworkManager.Singleton
        );
    }

    private void OnDestroy()
    {
        ServerGameManager?.Dispose();
    }
}

