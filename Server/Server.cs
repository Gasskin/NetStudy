using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Server : IDisposable
    {
        // IP地址
        private const string IP = "127.0.0.1";
        // 端口号
        private const int HOST = 1234;

        // 服务器Socket
        private Socket server;
        // 客户端Socket及状态信息
        private Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        // 多路复用
        List<Socket> checkRead = new List<Socket>();

        public void Init()
        {
            //Socket
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            var ipAdr = IPAddress.Parse(IP);
            var ipEp = new IPEndPoint(ipAdr, HOST);
            server.Bind(ipEp);
            //Listen
            server.Listen(0);
            Console.WriteLine("[服务器]启动成功");

            //主循环
            while (true) 
            {
                //填充checkRead列表
                checkRead.Clear();
                checkRead.Add(server);
                foreach (var state in clients.Values)
                    checkRead.Add(state.socket);
                //select
                Socket.Select(checkRead, null, null, 1000);
                //检查可读对象
                foreach (var socket in checkRead)
                {
                    if (socket == server)
                    {
                        AcceptClient(socket);
                    }
                    else
                    {
                        ReadClientfd(socket);
                    }
                }
            }
        }

        //读取Listenfd
        public void AcceptClient(Socket server)
        {
            var clientfd = server.Accept();
            var state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);

            Console.WriteLine($"[客户端登录]{clientfd.RemoteEndPoint}");
        }

        //读取Clientfd
        public bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            //接收
            int count = 0;
            try
            {
                count = clientfd.Receive(state.buff);
            }
            catch (SocketException ex)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                return false;
            }
            //客户端关闭
            if (count == 0)
            {
                Console.WriteLine($"[客户端关闭]{clientfd.LocalEndPoint}");
                clientfd.Close();
                clients.Remove(clientfd);
                return false;
            }
            //广播
            string recvStr = Encoding.UTF8.GetString(state.buff, 0, count);
            Console.WriteLine($"[接受客户端消息]{recvStr}");
            byte[] sendBytes = Encoding.UTF8.GetBytes(recvStr);
            foreach (ClientState cs in clients.Values)
            {
                cs.socket.Send(sendBytes);
            }
            return true;
        }

        public void Dispose()
        {
            if (server != null)
            {
                Console.WriteLine("[服务器]关闭");
                server.Dispose();
            }
        }
    }
}