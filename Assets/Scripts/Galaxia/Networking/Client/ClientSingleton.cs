using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;

    //클라이언트 매니저
    public ClientGamaManager ClientGameManager { get; private set; }
    public static ClientSingleton Instance
    {
        get
        {
            if(instance != null) return instance;

            instance = FindObjectOfType<ClientSingleton>();

            if(instance ==null)
            {
                Debug.LogError("ClientSingleton is not found");
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

    //클라이언트 생성
    public async Task<bool> CreateClient()
    {
        ClientGameManager = new ClientGamaManager();
        
        return await ClientGameManager.InitAsync();
    }

    private void OnDestroy()
    {
        ClientGameManager?.Dispose();
    }
}

