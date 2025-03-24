using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Board : NetworkBehaviour
{
    [SerializeField] private Transform boardEntityHolder;
    [SerializeField] private BoardEntityDisplay boardEntityPrefab;

    private NetworkList<BoardEntityState> boardEntities;
    
    //Entity 데이터 저장
    private List<BoardEntityDisplay> boardEntityDisplays = new List<BoardEntityDisplay>();
    private void Awake()
    {
        boardEntities = new NetworkList<BoardEntityState>();
    }

    private void HandlePlayerSpawned(AirPlayer player)
    {
        boardEntities.Add(new BoardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Golds = 0
        });
    }

    private void HandlePlayerDespawned(AirPlayer player)
    {
        if (boardEntities == null) return;

        foreach (BoardEntityState entity in boardEntities)
        {
            if(entity.ClientId != player.OwnerClientId) continue;

            boardEntities.Remove(entity);
            break;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AirPlayer[] players = FindObjectsByType<AirPlayer>(FindObjectsSortMode.None);
            foreach (AirPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }

            AirPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            AirPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }

        if (IsClient)
        {
            boardEntities.OnListChanged += HandleBoardEntitiesChanged;

            foreach (BoardEntityState entitiy in boardEntities)
            {
                HandleBoardEntitiesChanged(new NetworkListEvent<BoardEntityState>
                {
                    Type = NetworkListEvent<BoardEntityState>.EventType.Add,
                    Value = entitiy
                });
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            AirPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            AirPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }

        if (IsClient)
        {
            boardEntities.OnListChanged -= HandleBoardEntitiesChanged;
        }
    }

    private void HandleBoardEntitiesChanged(NetworkListEvent<BoardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<BoardEntityState>.EventType.Add:
                if (!boardEntityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    BoardEntityDisplay entityDisplay = Instantiate(boardEntityPrefab, boardEntityHolder);
                    entityDisplay.Initialize(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Golds);
                    boardEntityDisplays.Add(entityDisplay);
                }
                break;
            
            case NetworkListEvent<BoardEntityState>.EventType.Remove:
                BoardEntityDisplay entityRemove =
                    boardEntityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (entityRemove != null)
                {
                    entityRemove.transform.SetParent(null);
                    Destroy(entityRemove.gameObject);
                    boardEntityDisplays.Remove(entityRemove);
                }
                break;
            
            case NetworkListEvent<BoardEntityState>.EventType.Value:
                BoardEntityDisplay entityUpdate =
                    boardEntityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (entityUpdate != null)
                {
                    entityUpdate.UpdateGolds(changeEvent.Value.Golds);
                }
                break;
        }
    }
}
