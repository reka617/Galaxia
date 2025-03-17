using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberGuessingClient : MonoBehaviour
{   
    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private int serverPort = 8888;
    [SerializeField] private TMP_InputField guessInput;          // 숫자 입력 UI
    [SerializeField] private Button submitButton;            // 제출 버튼 UI
    [SerializeField] private TextMeshProUGUI responseText;              // 결과 표시 UI (TextMeshPro)

    private TcpClient client;
    private NetworkStream stream;
    private bool isConnected = false;

    private void Start()
    {
        // UI 요소가 null인지 체크
        if (guessInput == null || submitButton == null || responseText == null)
        {
            Debug.LogError("UI elements not assigned!");
            return;
        }

        submitButton.onClick.AddListener(SendGuess); // 버튼에 이벤트 리스너 연결
        ConnectToServer(); // 서버 연결 시도
    }

    private async void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIP, serverPort); // 비동기 연결
            stream = client.GetStream();
            isConnected = true;
            Debug.Log("Connected to server!");
            responseText.text = "Connected to server!";
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
            responseText.text = "Connection failed.";
        }
    }

    private async void SendGuess()
    {
        if (!isConnected || !client.Connected)
        {
            Debug.LogError("Not connected to server.");
            responseText.text = "Not connected to server.";
            return;
        }

        string guessText = guessInput.text;
        if (string.IsNullOrEmpty(guessText))
        {
            Debug.LogWarning("Please enter a number.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(guessText);
            await stream.WriteAsync(data, 0, data.Length); // 서버로 추측값 전송

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 서버로부터 응답 받기
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            Debug.Log("Server says: " + response);
            responseText.text = response;

            if (response == "Correct!")
            {
                await Task.Delay(1000); // 1초 대기
                ConnectToServer(); // 새 게임을 위해 재연결
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Communication Error: " + e);
            isConnected = false;
            responseText.text = "Connection lost";

            // 연결 종료 처리
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
    }

    private void OnDisable()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }
}
