using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TcpClientASync : MonoBehaviour
{
    [SerializeField] private string serverIp = "127.0.0.1"; // 서버 IP 주소
    [SerializeField] private int serverPort = 7077;         // 서버 포트 번호

    [SerializeField] private TMP_InputField inputField;    // 텍스트 입력 필드
    [SerializeField] private Button sendButton;        // 전송 버튼
    [SerializeField] private TextMeshProUGUI responseText;       
    
    private TcpClient client;
    private NetworkStream stream;
    // Start is called before the first frame update
    private async void Start()
    {
        // UI 요소가 null인지 확인
        if (inputField == null || sendButton == null || responseText == null)
        {
            Debug.LogError("UI elements not assigned!");
            return;
        }
        
        sendButton.onClick.AddListener(SendData); // 버튼 클릭 이벤트에 SendData 함수 연결

        try
        {
            await ConnectToServer(); // 비동기로 서버에 연결
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
            responseText.text = "Connection failed."; //연결 실패시
        }
    }

    private async Task ConnectToServer()
    {
        client = new TcpClient();
        await client.ConnectAsync(serverIp, serverPort);
        stream = client.GetStream();
        responseText.text = "Connected to server.";
        Debug.Log("Connected to server.");
    }
    
    public async void SendData() //async 추가
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Not connected to server.");
            responseText.text = "Not connected.";
            return;
        }

        string message = inputField.text;

        if (string.IsNullOrEmpty(message)) // 빈 문자열 방지
        {
            Debug.LogWarning("Input is empty.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message); // 메시지를 바이트 배열로 변환

        try
        {
            //await stream.WriteAsync(data, 0, data.Length); // 서버에 데이터 전송 (비동기)
            await Task.Run(() => stream.Write(data, 0, data.Length));
            Debug.Log("Sent: " + message);

            byte[] buffer = new byte[1024];
            //int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 서버 응답 수신 (비동기)
            int bytesRead = await Task.Run(() => stream.Read(buffer, 0, buffer.Length));
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            responseText.text = "Server says: " + response; // 응답 텍스트 업데이트
            Debug.Log("Received: " + response);
        }
        catch (Exception e)
        {
            Debug.LogError("Send/Receive error: " + e.Message);
            responseText.text = "Error: " + e.Message;
        }
    }

    private void OnDestroy()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }
}
