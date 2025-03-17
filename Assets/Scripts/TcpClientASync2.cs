using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TcpClientASync2 : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // 서버 IP 주소 (로컬백 주소)
    public int serverPort = 7777;
    public TMP_InputField inputField;    // 메시지 입력 UI
    public Button sendButton;        // 메시지 전송 UI
    public TextMeshProUGUI receiveText;         // 수신 메시지 표시 UI

    private TcpClient client;
    private NetworkStream stream;
    
    async void Start()
    {
        sendButton.onClick.AddListener(SendMessage); // 버튼 클릭 이벤트에 SendMessage 함수 연결
        await ConnectToServer(); // 서버 연결 시작
    }
     async Task ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIP, serverPort); // 비동기 연결 시도
            stream = client.GetStream();
            Debug.Log("Connected to server!");
            receiveText.text = "Connected to server!";
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect: " + e.Message);
            receiveText.text = "Failed to connect: " + e.Message;
        }
    }

    async void SendMessage()
    {
        if (client == null || !client.Connected)
        {
            Debug.LogWarning("Not connected to server.");
            receiveText.text = "Not connected to server.";
            return;
        }

        try
        {
            string message = inputField.text;
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length); // 비동기 전송
            Debug.Log("Sent: " + message);

            //에코 메시지 비동기 수신 대기 추가
            byte[] receiveBuffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
            
            if (bytesRead > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                Debug.Log("Received: " + receivedMessage);
                receiveText.text = "Received: " + receivedMessage; //UI업데이트
            }
            else
            {
                Debug.Log("Server closed connection.");
                receiveText.text = "Server closed connection.";
                client.Close();
                client = null;
            }

            inputField.text = ""; // 입력 필드 초기화
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
            receiveText.text = "Error sending message: " + e.Message;

            if (client != null) //추가: 클라이언트 객체가 있으면 닫기
            {
                client.Close();
                client = null;
            }
        }
    }

    private void OnApplicationQuit()
    {
        if(client != null)
            client.Close();
    }
}

