using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ChatClient : MonoBehaviour
{
    //UGUI
    public InputField inputFeld;
    public Button connect;
    public Button send;
    public Text text;

    //定义套接字
    private Socket socket;
    
    //接收缓冲区
    private bool isChange = false;
    private byte[] buffer = new byte[1024];
    private StringBuilder sb = new StringBuilder();

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
        SendMsg("加入聊天");
        client.BeginReceive( buffer, 0, 1024, 0, ReceiveCallback, client);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        var client = ar.AsyncState as Socket;
        int count = client.EndReceive(ar);
        AddMsg(Encoding.UTF8.GetString(buffer, 0, count));
        client.BeginReceive( buffer, 0, 1024, 0, ReceiveCallback, client);
    }

    private void Send()
    {
        SendMsg(inputFeld.text);
    }

    private void SendMsg(string msg)
    {
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(msg);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    private void SendCallback(IAsyncResult ar)
    {
        var client = ar.AsyncState as Socket;
        client.EndSend(ar);
    }

    private void AddMsg(string msg)
    {
        sb.Append($"{msg}\n");
        isChange = true;
    }

    private void Update()
    {
        if (isChange)
        {
            text.text = sb.ToString();
            isChange = false;
        }
    }
}
