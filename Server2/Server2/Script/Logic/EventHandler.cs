using System;

namespace Framework
{
    public partial class MsgHandler
    {
        public static void OnDisconnect(ClientState c)
        {
            Console.WriteLine($"[客户端关闭]{c.socket.RemoteEndPoint}");
        }

        public static void OnTimer()
        {
            CheckPing();
        }
        
        /// <summary>
        /// Ping检查
        /// </summary>
        public static void CheckPing()
        {
            // 现在的时间戳
            long timeNow = NetManager.GetTimeStamp();
            // 遍历，删除
            foreach (ClientState s in NetManager.clients.Values)
            {
                if (timeNow - s.lastPingTime > NetManager.pingInterval * 4)
                {
                    Console.WriteLine("[客户端失去连接]" + s.socket.RemoteEndPoint.ToString());
                    NetManager.Close(s);
                    return;
                }
            }
        }

    }
}
