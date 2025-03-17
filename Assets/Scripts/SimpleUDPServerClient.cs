using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SimpleUDPServerClient1 : MonoBehaviour
{
    [Header("Network Config")]
    [SerializeField] private string serverIp = "127.0.0.1"; // 서버 IP 주소
    [SerializeField] private int port = 8080;         // 포트 번호

    [Header("Mode")]
    [SerializeField] private bool isServer = false; // 서버/클라이언트 모드

    private UdpClient udpClient;

    private Thread serverThread; // 서버 스레드 (서버 모드일 때만 사용)
    private CancellationTokenSource cancellationTokenSource; // 스레드 취소 토큰

    private void Start()
    {
        if (isServer)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
    }

    // Update is called once per frame
    void StartServer()
    {
        cancellationTokenSource = new CancellationTokenSource(); // 취소 토큰 생성

        serverThread = new Thread(() => RunServer(cancellationTokenSource.Token));
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void RunServer(CancellationToken cancellationToken)
    {

        try
        {
            udpClient = new UdpClient(port);
            Debug.Log("UDPServerClient listening on port " + port);

            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(data);
                    Debug.Log("Receive from :" + clientEndPoint + " : " + message);

                    udpClient.Send(data, data.Length, clientEndPoint);

                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.Interrupted)
                    {
                        Debug.LogError("Server Socketexception :" + ex.SocketErrorCode);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Server exception: " + ex.ToString());
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError("Start Server error : " + ex.ToString());
        }
        finally
        {
            if (udpClient != null)
            {
                udpClient.Close();
            }
            Debug.Log("Server thread exiting");
        }

    }

    void StartClient()
    {
        try
        {
            udpClient = new UdpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
            udpClient.Connect(serverEndPoint);
            Debug.Log("UDP Client connected to " + serverEndPoint);

            ReceiveDataAsync();
            //Invoke("SendTestMessage", 5f); // 사용하지 않는 함수 호출 제거
            SendDataAsync(); // 바로 메시지 전송 (또는 원하는 시점에 호출)
        }
        catch (Exception ex)
        {
            Debug.LogError("Client Start Error : " + ex);

        }

    }

    async void ReceiveDataAsync()
    {
        // while 루프 추가: 계속해서 데이터 수신
        while (true)
        {
            try
            {
                // 방법 1: UdpClient.ReceiveAsync() 사용 (비동기)
                //UdpReceiveResult result = await udpClient.ReceiveAsync();

                // 방법 2: Task.Run + UdpClient.Receive() (동기 함수를 비동기처럼) - 권장
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); //
                UdpReceiveResult result = await Task.Run(() =>
                {
                    return new UdpReceiveResult(udpClient.Receive(ref remoteEndPoint), remoteEndPoint);
                });


                byte[] data = result.Buffer;
                IPEndPoint clientEndPoint = result.RemoteEndPoint;

                string message = Encoding.UTF8.GetString(data);
                Debug.Log("Received from server:" + message);
            }
            catch (ObjectDisposedException)
            {
                Debug.Log("Receive loop exiting (UdpClient closed).");
                break; // while 루프 종료
            }
            catch (Exception e)
            {
                Debug.LogError("Client Receive Error: " + e);
                // break; // Receive 에러 발생해도, while 루프를 종료하면 안됨.
            }
        }
    }

    //SendDataAsync 이름 변경 및 즉시 호출
    async void SendDataAsync()
    {
        if (udpClient != null)
        {
            string message = "Hello from client!";
            byte[] data = Encoding.UTF8.GetBytes(message);

            try
            {
                // 방법 1: UdpClient.SendAsync() (비동기)
                //await udpClient.SendAsync(data, data.Length);

                // 방법 2: Task.Run + UdpClient.Send()
                await Task.Run(() => udpClient.Send(data, data.Length));


                Debug.Log("sent: " + message);
            }
            catch (Exception ex)
            {
                Debug.LogError("SendDataAsync Error : " + ex);
            }
        }
    }

    private void OnDestroy()
    {

        if (isServer && cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Join();
            }
        }

        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}