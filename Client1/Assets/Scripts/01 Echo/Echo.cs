using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
public class Echo : MonoBehaviour 
{
    public InputField InputFeld;
    public Button connect;
    public Button send;

    private Socket socket;

    private void Start()
    {
        connect.onClick.AddListener(Connection);
        send.onClick.AddListener(Send);
    }

    private void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 8888);
    }

    private void Send()
    {
        //Send
        string sendStr = InputFeld.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
        //Recv
        byte[] readBuff = new byte[1024]; 
        int count = socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        Debug.Log(recvStr);
        socket.Close();
    }
}