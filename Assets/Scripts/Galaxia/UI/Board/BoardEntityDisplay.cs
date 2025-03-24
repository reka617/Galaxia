using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BoardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    private FixedString32Bytes playerName;
    public ulong ClientId { get; private set; }
    public int Golds { get; private set; }

    [SerializeField] private Color myRankColor;

    public void Initialize(ulong clientId, FixedString32Bytes playerName, int golds)
    {
        this.ClientId = clientId;
        this.playerName = playerName;

        if (clientId == NetworkManager.Singleton.LocalClientId) displayText.color = myRankColor;

        UpdateGolds(golds);
    }

    public void UpdateGolds(int golds)
    {
        Golds = golds;
        UpdateText();
    }

    public void UpdateText()
    {
        displayText.text = $"[{transform.GetSiblingIndex()+1}] {playerName} / {Golds}";
    }

}
