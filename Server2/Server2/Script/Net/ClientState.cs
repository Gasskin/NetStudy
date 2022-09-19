using System.Net.Sockets;

namespace Framework
{
    public class ClientState
    {
        public Socket socket; 
        public ByteArray readBuff = new ByteArray();
        public long lastPingTime = 0;
        public bool isLogin = false;
    }
}