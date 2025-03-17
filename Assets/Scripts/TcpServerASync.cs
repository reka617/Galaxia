using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpServerASync : MonoBehaviour
{
    //전통적인 Thread 방식으로 진행한 비동기화 방식
    private TcpListener tcpListener;
    private Thread listenerThread;
    
    private void Start()
    {
        listenerThread = new Thread(ListenForIncomingRequests);
        listenerThread.IsBackground = true; // 메인 스레드가 종료되면 같이 종료
        listenerThread.Start();
    }

    private void ListenForIncomingRequests()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7077);
            tcpListener.Start();
            Debug.Log("Server is listening");

            while (true)
            {
                TcpClient connectedTcpClient = tcpListener.AcceptTcpClient(); // 새 클라이언트 연결
                // 각 클라이언트에 대해 새 스레드 시작
                Thread clientThread = new Thread(() => HandleClientComm(connectedTcpClient));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException);
        }
        finally
        {
            // 서버 종료 시 리스너 종지 (예: 게임 종료 시)
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }
    }
    
    private void HandleClientComm(TcpClient client)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024]; // 읽기 버퍼
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // 읽은 데이터를 문자열로 변환
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Received: " + dataReceived);

                    // 클라이언트에 응답 (에코)
                    string response = "Server received: " + dataReceived;
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);

                    // 종료 조건 (예: 클라이언트가 "exit" 메시지를 보낸 경우)
                    if (dataReceived.Trim().ToLower() == "exit")
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Communication error: " + ex);
            }

            Debug.Log("Client disconnected");
        }
    }

    private void OnApplicationQuit()
    {
        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Abort();
        }
    }
}
