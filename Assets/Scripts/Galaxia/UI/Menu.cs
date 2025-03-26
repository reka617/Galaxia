using System;
using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInputField;

    [SerializeField] private TMP_Text matchBtnText;
    [SerializeField] private TMP_Text queueTimerText;
    [SerializeField] private TMP_Text queueStateText;

    private bool isMatchMaking;
    private bool isCancelling;

    private void Start()
    {
        if (ClientSingleton.Instance == null) return;
        
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        queueStateText.text = string.Empty;
        queueTimerText.text = string.Empty;
    }

    public async void FindMatchPressed()
    {
        if (isCancelling) return;

        if (isMatchMaking)
        {
            //매칭 큐 캔슬
            queueStateText.text = "Cancelling...";
            isCancelling = true;

            await ClientSingleton.Instance.ClientGameManager.CancelMatchMaking();
            //매칭 잡힘
            isCancelling = false;
            isMatchMaking = false;
            matchBtnText.text = "Find Match";
            queueStateText.text = string.Empty;
            return;
        }

        ClientSingleton.Instance.ClientGameManager.MatchMakeAsync(OnMatchMade);
        //큐 시작
        matchBtnText.text = "Cancel";
        queueStateText.text = "Searching...";
        isMatchMaking = true;
    }
    
    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                queueStateText.text = "Match Success";
                break;
            case MatchmakerPollingResult.TicketCreationError:
                queueStateText.text = "Match Ticket Creation Error";
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                queueStateText.text = "Match Ticket Cancellation Error";
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                queueStateText.text = "Match Ticket Retrieval Error";
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                queueStateText.text = "Match Assignment Error";
                break;
            
            
        }
    }

    public async void StartHost()
    {
        await HostSingleton.Instance.HostGameManager.StartHostAsync();
    }

    public async void StartClinet()
    {
        await ClientSingleton.Instance.ClientGameManager.StartClientAsync(codeInputField.text);
    }
}
