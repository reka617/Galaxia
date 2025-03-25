using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{

    private string sIP;
    private int sPort;
    private int sPort2;

    private NetworkServer networkServer;
    private MultiplayAllocationService multiplayAllocationService;

    private const string GameScenename = "GalaxiaPlay";

    public ServerGameManager(string sIP, int sPort, int sPort2, NetworkManager manager)
    {
        this.sIP = sIP;
        this.sPort = sPort;
        this.sPort2 = sPort2;
        networkServer = new NetworkServer(manager);
        multiplayAllocationService = new MultiplayAllocationService();
    }

    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();
        if (!networkServer.OpenConnection(sIP, sPort))
        {
            Debug.LogWarning("Network server did not start");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(GameScenename, LoadSceneMode.Single);
    }
    public void Dispose()
    {
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }
}
