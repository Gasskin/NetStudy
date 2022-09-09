using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NetManager : MonoBehaviour
{
    public Socket socket;
    public Button connnetBtn;

    private byte[] buffer = new byte[1024];
    private Dictionary<string, Action<string>> msgMap = new Dictionary<string, Action<string>>();
    private List<string> msgList = new List<string>();

    private Player player;

    private void Start()
    {
        connnetBtn.onClick.AddListener(Connect);
    }

    private void Update()
    {
        if (msgList.Count <= 0)
            return;
        String msgStr = msgList[0];
        msgList.RemoveAt(0);

        string[] split = msgStr.Split(';');
        
        string msgName = split[0];
        string msgArgs = split[1];

        if (msgMap.ContainsKey(msgName))
            msgMap[msgName].Invoke(msgArgs);
    }

    public void Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 1234);

        var res = Resources.Load("Player");
        player = (Instantiate(res) as GameObject).GetComponent<Player>();
        player.Init(this);

        socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, ReceiveCallBack, socket);
        Send($"Enter;{socket.LocalEndPoint}");
    }

    private void ReceiveCallBack(IAsyncResult obj)
    {
        var socket = obj.AsyncState as Socket;
        if (socket == null)
            return;
        var count = socket.EndReceive(obj);
        var msg = Encoding.UTF8.GetString(buffer, 0, count);
        msgList.Add(msg);
        socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, ReceiveCallBack, socket);
    }

    public  void Send(string sendStr)
    {
        if (socket == null) return;
        if (!socket.Connected) return;
        byte[] sendBytes =
        System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
    }

    public void AddListener(string type,Action<string> callback)
    {
        if (msgMap.ContainsKey(type))
        {
            msgMap[type] += callback;
        }
        else
        {
            msgMap.Add(type, callback);
        }
    }
}
