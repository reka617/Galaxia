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

    private int entitiesCount = 8;
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
        
        //갱신된 골드를 얻기 위해 Hook
        player.Wallet.golds.OnValueChanged += (_, newGolds) => HandleGoldsChanged(player.OwnerClientId, newGolds);
    }

    private void HandleGoldsChanged(ulong clientId, int newGolds)
    {
        for (int i = 0; i < boardEntities.Count; i++)
        {
            if (boardEntities[i].ClientId != clientId) continue;

            boardEntities[i] = new BoardEntityState
            {
                ClientId = boardEntities[i].ClientId,
                PlayerName = boardEntities[i].PlayerName,
                Golds = newGolds
            };

            return;
        }
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
        
        player.Wallet.golds.OnValueChanged -= (_, newGolds) => HandleGoldsChanged(player.OwnerClientId, newGolds);
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
        //x,y의 값을 정렬
        boardEntityDisplays.Sort((x,y) => y.Golds.CompareTo(x.Golds));

        for (int i = 0; i < boardEntityDisplays.Count; i++)
        {
            boardEntityDisplays[i].transform.SetSiblingIndex(i);
            boardEntityDisplays[i].UpdateText();
            boardEntityDisplays[i].gameObject.SetActive(i <= entitiesCount -1);
        }

        BoardEntityDisplay display =
            boardEntityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if (display != null)
        {
            if (display.transform.GetSiblingIndex () >= entitiesCount)
            {
                boardEntityHolder.GetChild(entitiesCount-1).gameObject.SetActive(false);
                display.gameObject.SetActive(true);
            }
        }
    }
}
