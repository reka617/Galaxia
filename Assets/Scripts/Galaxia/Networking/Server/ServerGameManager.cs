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

    //private NetworkServer networkServer;
    
    public NetworkServer NetworkServer { get; private set; }
    private MultiplayAllocationService multiplayAllocationService;

    private const string GameScenename = "GalaxiaPlay";
    private MatchplayBackfiller backfiller;

    public ServerGameManager(string sIP, int sPort, int sPort2, NetworkManager manager)
    {
        this.sIP = sIP;
        this.sPort = sPort;
        this.sPort2 = sPort2;
        NetworkServer = new NetworkServer(manager);
        multiplayAllocationService = new MultiplayAllocationService();
    }

    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();

        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if (matchmakerPayload != null)
            {
                await StartBackfill(matchmakerPayload);
                NetworkServer.OnUserJoined += UserJoined;
                NetworkServer.OnUserLeft += UserLeft;
            }
            else
            {
                Debug.LogWarning("Matchmaker payload time out");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        if (!NetworkServer.OpenConnection(sIP, sPort))
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

    private void UserJoined(UserData user)
    {
        backfiller.AddPlayerToMatch(user);
        multiplayAllocationService.AddPlayer();
        if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            _ = backfiller.StopBackfill();
        }
    }

    private void UserLeft(UserData user)
    {
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        multiplayAllocationService.RemovePlayer();
        if (playerCount <= 0)
        {
            CloseServer();
            return;
        }

        if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            _ = backfiller.BeginBackfilling();
        }
    }

    private async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }
    
    public void Dispose()
    {
        NetworkServer.OnUserJoined -= UserJoined;
        NetworkServer.OnUserLeft -= UserLeft;
        
        backfiller?.Dispose();
        
        multiplayAllocationService?.Dispose();
        NetworkServer?.Dispose();
    }
}
