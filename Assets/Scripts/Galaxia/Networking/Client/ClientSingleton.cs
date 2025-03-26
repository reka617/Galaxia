using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;

    //Ŭ���̾�Ʈ �Ŵ���
    public ClientGameManager ClientGameManager { get; private set; }
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

    //Ŭ���̾�Ʈ ����
    public async Task<bool> CreateClient()
    {
        ClientGameManager = new ClientGameManager();
        
        return await ClientGameManager.InitAsync();
    }

    private void OnDestroy()
    {
        ClientGameManager?.Dispose();
    }
}

