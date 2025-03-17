using Unity.Netcode;
using UnityEngine;

public class ConnectServer : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
}
