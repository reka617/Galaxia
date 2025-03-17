using System;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TcpClientSync : MonoBehaviour
{
    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 8888;

    [SerializeField] private TMP_InputField inputMessage;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI responseText;

    private TcpClient tcpClient;
    private NetworkStream stream;
    private void Start()
    {
        if (inputMessage == null || sendButton == null || responseText == null)
        {
            Debug.LogError("UI Elements not assigned");
            return;
        }
        
        sendButton.onClick.AddListener(SendData);

        ConnectToServer();

    }

    private void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient(serverIp, serverPort); // 연결시도
            stream = tcpClient.GetStream();
            responseText.text = "Connected to server.";
            Debug.Log("Connected to Server");
            ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError("Connection Error : " + e.Message);
            responseText.text = "Connection failed.";
        }
    }

    public void SendData()
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            Debug.LogError("Not conected to server");
            responseText.text = "Not connected.";
            return;
        }

        string message = inputMessage.text;
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("Input is empty");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);

        try
        {
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent: " + message);
            inputMessage.text = "";

        }
        catch (Exception e)
        {
            Debug.LogError("Send error : " + e.Message);
            responseText.text = "Send error.";
        }
    }

    private void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (tcpClient != null && tcpClient.Connected)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length); // 데이터 수신 (blocking방식)
                if (bytesRead == 0)
                {
                    Debug.Log("Server disconnected.");
                    responseText.text = "Server disconnected.";
                    break;
                }

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                responseText.text = "Server : " + receivedMessage;
                Debug.Log("Received : " + receivedMessage);
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Debug.Log("Server Disconnected");
                }
                else
                {
                    Debug.LogError("Received error : " + e.Message);
                }

                responseText.text = "Receive error.";
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if(stream != null)
            stream.Close();
        if(tcpClient != null)
            tcpClient.Close();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
