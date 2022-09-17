using System;

namespace Framework
{
    public partial class MsgHandler
    {
        public static void OnDisconnect(ClientState c)
        {
            Console.WriteLine("Event Close");
        }

        public static void OnTimer()
        {
        }
    }
}
