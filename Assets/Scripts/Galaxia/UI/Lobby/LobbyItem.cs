using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyPlayerText;

    private LobbyList lobbiesList;

    private Lobby lobby;

    public void Init(LobbyList lobbiesList, Lobby lobby)
    {
        this.lobbiesList = lobbiesList;
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        lobbyPlayerText.text = lobby.Players.Count.ToString() + "/" + lobby.MaxPlayers.ToString();
    }

    public void Join()
    {
        lobbiesList.JoinAsync(lobby);
    }
}
