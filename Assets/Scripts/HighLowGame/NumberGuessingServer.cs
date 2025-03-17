using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Random = System.Random;

public class NumberGuessingServer : MonoBehaviour
{
     [SerializeField] private int port = 8888;
    private TcpListener listener;
    private Thread listenerThread;
    private TcpClient connectedClient;
    private bool running = true;
    private System.Random random;

    private void Start()
    {
        //랜덤 객체 초기화
        random = new Random();
        // 리스너 스레드 시작
        listenerThread = new Thread(new ThreadStart(ListenForClients));
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    private void ListenForClients()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Debug.Log("Server started. Listening for connections...");

        try
        {
            while (running)
            {
                connectedClient = listener.AcceptTcpClient(); // 동기적으로 클라이언트 대기
                Debug.Log("Client connected!");
                StartNewGame(); // 새로운 게임 시작
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Server error: " + e.Message);
        }
        finally
        {
            if (listener != null)
            {
                listener.Stop();
            }
        }
    }

    private void StartNewGame()
    {
        // 1-100 사이의 랜덤 숫자 생성
        int secretNumber = random.Next(1, 101);
        Debug.Log("Secret number generated: " + secretNumber);

        HandleClient(connectedClient, secretNumber);
    }

    private void HandleClient(TcpClient client, int number)
    {
        string result = "";
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (true)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length); // 클라이언트 입력 받기(BLOCK)
                if (bytesRead == 0)
                {
                    Debug.Log("Client disconnected.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                int guessNumber;

                // 입력값이 숫자인지 확인
                if (!int.TryParse(message, out guessNumber))
                {
                    result = "숫자를 입력해주세요!";
                    byte[] responseData = Encoding.UTF8.GetBytes(result);
                    stream.Write(responseData, 0, responseData.Length); // 응답 전송
                    continue;
                }

                // 숫자 비교
                if (guessNumber > number)
                {
                    result = "Down";
                }
                else if (guessNumber < number)
                {
                    result = "Up";
                }
                else
                {
                    result = "Correct!";
                }

                // 결과 전송
                byte[] data = Encoding.UTF8.GetBytes(result);
                stream.Write(data, 0, data.Length);

                // 정답을 맞췄다면 게임 종료
                if (result == "Correct!")
                {
                    client.Close();
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Client communication error: " + e.Message);
                break;
            }
        }
    }

    private void OnDisable()
    {
        running = false;
        
        if (listener != null)
        {
            listener.Stop();
        }

        if (connectedClient != null)
        {
            connectedClient.Close();
        }

        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Abort();
        }
    }
}
