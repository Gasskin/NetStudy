using System.Net.Sockets;

namespace Server
{
    internal class ClientState
    {
        public Socket socket;
        public byte[] buff = new byte[1024];
    }
}
