using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TcpServerASync2 : MonoBehaviour
{
    public int port = 7777;
    private TcpListener listener;
    
    async void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Debug.Log("Server started. Waiting for connections...");

        while (true) // 비동기 Accept를 계속 호출하기 위해 무한루프 사용
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync(); // 비동기적으로 클라이언트 연결을 기다림
                Debug.Log("Client connected!");
                _ = HandleClient(client); // 클라이언트 처리 루틴 호출 (별도의 Task로 실행)
            }
            catch (Exception e)
            {
                Debug.LogError("Error accepting client: " + e.Message);
                break;
            }
        }
    }

    async Task HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) // 비동기 읽기
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Received: " + message);

                await stream.WriteAsync(buffer, 0, bytesRead); // 비동기 쓰기 (에코)
                Debug.Log("Sent: " + message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error handling client: " + e.Message);
        }
        finally
        {
            Debug.Log("Client disconnected.");
            client.Close();
        }
    }

    private void OnApplicationQuit()
    {
        if(listener != null)
            listener.Stop();
    }
}
