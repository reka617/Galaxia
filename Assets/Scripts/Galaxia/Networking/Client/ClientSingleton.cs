using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;

    public ClientGameManager ClientGameManager { get; private set; }

    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType<ClientSingleton>();

            if (instance == null)
            {
                Debug.LogError("ClientSingleton is not found");
                return null;
            }

            return instance;
        }
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> CreateClient()
    {
        ClientGameManager = new ClientGameManager();
        return await ClientGameManager.InitAsync();
    }
    
}
