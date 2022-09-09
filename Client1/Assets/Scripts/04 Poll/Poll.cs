using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

class Poll : MonoBehaviour
{

    //�����׽���
    Socket socket;
    //UGUI
    public InputField InputFeld;
    public Text text;

    //������Ӱ�ť
    public void Connection()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork,   SocketType.Stream, ProtocolType.Tcp);
        //Connect
        socket.Connect("127.0.0.1", 8888);
    }

    //������Ͱ�ť
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
