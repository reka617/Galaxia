using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyList : MonoBehaviour
{
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if(isRefreshing) return;

        isRefreshing = false;

        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions();
            queryLobbiesOptions.Count = 20; //로비 개수

            queryLobbiesOptions.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT, //GT = Grater than ~보다 높은
                    value: "0"), // value가 0보다 높은 빈 슬롯이 있는 로비만 가져오는 쿼리

                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ, // EQ = Equal 동일할경우 
                    value: "0") // 값이 0 일경우, 잠근다
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject); // 로비 아이템 삭제
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Init(this, lobby);
            }

        }
        catch (LobbyServiceException e) // 로비서비스 예외 처리
        {
            Debug.Log(e);
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) return;

        isJoining = true;

        try
        {
            //로비 할당
            Lobby joinLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joinLobby.Data["JoinCode"].Value; // join 코드 가져오기
            
            //클라이언트시작
            await ClientSingleton.Instance.ClientGameManager.StartClientAsync(joinCode);

        }
        catch (LobbyServiceException e)
        {
           Debug.LogError(e);
        }

        isJoining = false;
    }
}
