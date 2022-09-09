using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

class Poll : MonoBehaviour
{

    //定义套接字
    Socket socket;
    //UGUI
    public InputField InputFeld;
    public Text text;

    //点击连接按钮
    public void Connection()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork,   SocketType.Stream, ProtocolType.Tcp);
        //Connect
        socket.Connect("127.0.0.1", 8888);
    }

    //点击发送按钮
    public void Send()
    {
        
    }

    public void Update()
    {
        if (socket == null)
        {
            return;
        }

        if (socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] readBuff = new byte[1024];
            int count = socket.Receive(readBuff);
            string recvStr =
                System.Text.Encoding.Default.GetString(readBuff, 0, count);
            text.text = recvStr;
        }
    }
}
