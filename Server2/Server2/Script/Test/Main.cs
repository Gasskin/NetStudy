using Framework;

namespace Server
{
    class Program
    {
        private static Server server;
        
        private const string IP = "127.0.0.1";
        private const int HOST = 8765;
        
        private const string DB = "playerdata";
        private const string DB_IP = "127.0.0.1";
        private const int DB_PORT = 3306;
        private const string DB_USER = "root";
        private const string DB_PW = "415753";

        static void Main(string[] args)
        {
            if (DbManager.Connect(DB, DB_IP, DB_PORT, DB_USER, DB_PW))
            {
                NetManager.StartLoop(IP, HOST);
            }
        }
    }
}