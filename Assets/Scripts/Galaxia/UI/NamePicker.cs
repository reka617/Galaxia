using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NamePicker : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button connectBtn;
    [SerializeField] private int minNameLength = 1;
    [SerializeField] private int maxNameLength = 12;

    public const string PlayerNameKey = "PlayerName";

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            connectBtn.interactable = false;
            nameInput.interactable = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

            return;
        }

        nameInput.text = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
        
        HandleNameChanged();
    }

    public void HandleNameChanged()
    {
        connectBtn.interactable = nameInput.text.Length >= minNameLength && nameInput.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        PlayerPrefs.SetString(PlayerNameKey, nameInput.text); // 플레이어 이름을 저장
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


}
