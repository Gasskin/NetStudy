using System;
using Framework;

namespace Server
{
    class Program
    {
        private static Server server;

        private const string IP = "127.0.0.1";
        private const int HOST = 8765;
        
        static void Main(string[] args)
        {
            NetManager.StartLoop(IP, HOST);
        }
    }
}