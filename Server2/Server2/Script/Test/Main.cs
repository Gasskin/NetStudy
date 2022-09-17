using System;
using Framework;

namespace Server
{
    class Program
    {
        private static Server server;
        static void Main(string[] args)
        {
            server = new Server();
            server.Init();
        }
    }
}