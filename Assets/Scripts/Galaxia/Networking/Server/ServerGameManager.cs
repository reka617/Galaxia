using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
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
    private MatchplayBackfiller backfiller;

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

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask =
            multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        //20초내에 결과를 내라
        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        backfiller = new MatchplayBackfiller($"{sIP}:{sPort}", payload.QueueName, payload.MatchProperties, 20);

        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }
    }
    
    public void Dispose()
    {
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }
}
