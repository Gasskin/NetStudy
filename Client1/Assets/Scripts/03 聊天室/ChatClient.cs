using System;
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
        SendMsg("客户端1 加入聊天");
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

    private void Update()
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }

        if (socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] readBuff = new byte[1024];
            int count = socket.Receive(readBuff);
            if(count == 0)
            {
                socket.Close();
                socket = null;
                return;
            }
            string recvStr = Encoding.Default.GetString(readBuff, 0, count);
            sb.Append($"{recvStr}\n");
            text.text = sb.ToString();
        }
    }
}
