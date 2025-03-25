using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;
    
    //private HostGameManager hostGamaManager;
    public HostGameManager HostGameManager { get; private set; }

    public static HostSingleton Instance
    {
        get
        {
            if(instance != null) return instance;

            instance = FindObjectOfType<HostSingleton>();
            if(instance ==null)
            {
                Debug.LogError("HostSingleton is not found");
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
    public void  CreateHost()
    {
        HostGameManager = new HostGameManager();
    }

    private void OnDestroy()
    {
        HostGameManager?.Dispose();
    }
}

