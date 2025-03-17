using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpServerSync : MonoBehaviour
{
    [SerializeField] private int port = 8888;

    private TcpListener tcpListener;
    private Thread serverThread;
    private TcpClient connectedClient;
    
    private volatile bool isRunning = true;
  
    void Start()
    {
        serverThread = new Thread(RunServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void RunServer()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Debug.Log("Server Started. Listening on port " + port);

            while (isRunning) // Thread.Abort() 대신 플래그 사용
            {
                if (tcpListener.Pending()) // 연결 요청이 있는지 확인
                {
                    connectedClient = tcpListener.AcceptTcpClient();
                    Debug.Log("Client connected");
                    HandleClient(connectedClient);
                }
                else
                {
                    Thread.Sleep(100); // CPU 사용률 감소
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Server error : " +e.Message);
        }
        finally
        {
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (true)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Debug.Log("Client Disconnected");
                    }

                    //데이터를 주고받을 때, 바이트를 가지고 문자열을 처리할 떄, 꼭 Encoding.UTF8.GetString 형태로 변환하는 과정이 필요
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Received : " + message);

                    //받은 메세지를 그대로 다시 서버에게 보내는 과정
                    stream.Write(buffer, 0, bytesRead);

                }
                catch (Exception e)
                {
                    if (e is SocketException || e is ObjectDisposedException)
                    {
                        Debug.LogError("Client Disconnect");
                    }
                    else
                    {
                        Debug.LogError("client conn error");
                    }
                    break;
                }
            }

        }
        
        client.Close();
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        
        if (tcpListener != null)
        {
            tcpListener.Stop();
        }

        if (connectedClient != null)
        {
            connectedClient.Close();
        }
        
        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Join(1000);
        }
    }
}
