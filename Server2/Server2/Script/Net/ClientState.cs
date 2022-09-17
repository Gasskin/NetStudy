using System.Net.Sockets;

namespace Framework
{
    public class ClientState
    {
        public Socket socket; 
        public ByteArray readBuff = new ByteArray(); 
    }
}