using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class AsyncClient : MonoBehaviour
{
    //UGUI
    public InputField InputFeld;
    public Button connect;
    public Button send;

    //定义套接字
    private Socket socket;
    
    //接收缓冲区
    private byte[] readBuff = new byte[1024];
    private string recvStr = "";
    private string sendStr;

    private void Start()
    {
        connect.onClick.AddListener(Connetion);
        send.onClick.AddListener(Send);
    }

    private void Connetion()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect("127.0.0.1", 8888, ConnectCallback, socket);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        var client = ar.AsyncState as Socket;
        client.EndConnect(ar);
        Debug.Log("Socket Connect Succ ");
        client.BeginReceive( readBuff, 0, 1024, 0, ReceiveCallback, client);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        var client = ar.AsyncState as Socket;
        int count = client.EndReceive(ar);
        recvStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
        Debug.Log(recvStr);
        client.BeginReceive( readBuff, 0, 1024, 0, ReceiveCallback, client);
    }

    private void Send()
    {
        //Send
        sendStr = InputFeld.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    
    }

    private void SendCallback(IAsyncResult ar)
    {
        var client = ar.AsyncState as Socket;
        client.EndSend(ar);
    }
}