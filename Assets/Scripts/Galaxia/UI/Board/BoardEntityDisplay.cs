using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BoardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    private FixedString32Bytes playerName;
    public ulong ClientId { get; private set; }
    public int Golds { get; private set; }

    public void Initialize(ulong clientId, FixedString32Bytes playerName, int golds)
    {
        this.ClientId = clientId;
        this.playerName = playerName;

        UpdateGolds(golds);
    }

    public void UpdateGolds(int golds)
    {
        Golds = golds;
        UpdateText();
    }

    public void UpdateText()
    {
        displayText.text = $"[1] {playerName} / {Golds}";
    }

}
