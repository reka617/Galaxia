using System;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;

    //public HostGameManager hostGameManager;

    public HostGameManager HostGameManager { get; private set; }
    
    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType<HostSingleton>();

            if (instance == null)
            {
                Debug.LogError("HostSingleton is not found");
                return null;
            }

            return instance;
        }
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    //호스트 생성
    public void CreateHost()
    {
        HostGameManager = new HostGameManager();
    }

    private void OnDestroy()
    {
        HostGameManager?.Dispose();
    }
}
