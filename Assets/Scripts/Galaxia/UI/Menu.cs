using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInputField;
    public async void StartHost()
    {
        await HostSingleton.Instance.HostGameManager.StartHostAsync();
    }

    public async void StartClinet()
    {
        await ClientSingleton.Instance.ClientGameManager.StartClientAsync(codeInputField.text);
    }
}
