using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Framework
{
    public static class NetManager
    {
        // 监听Socket
        private static Socket listenfd;

        // 客户端Socket及状态信息
        private static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        // Select的检查列表
        private static List<Socket> checkRead = new List<Socket>();

        /// <summary>
        /// 开启服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="listenPort"></param>
        public static void StartLoop(string ip,int listenPort)
        {
            // Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 绑定IP
            IPAddress ipAdr = IPAddress.Parse(ip);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, listenPort);
            listenfd.Bind(ipEp);
            // 开始监听
            listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");
            
            //循环
            while (true)
            {
                ResetCheckRead(); //重置checkRead 
                Socket.Select(checkRead, null, null, 1000);
                //检查可读对象
                for (int i = checkRead.Count - 1; i >= 0; i--)
                {
                    Socket s = checkRead[i];
                    if (s == listenfd)
                    {
                        // 处理服务器消息
                        ReadListenfd(s);
                    }
                    else
                    {
                        // 不理客户端你消息
                        ReadClientfd(s);
                    }
                }

                // 超时处理
            }
        }

        /// <summary>
        /// 重置并填充多路复用列表
        /// </summary>
        private static void ResetCheckRead()
        {
            checkRead.Clear();
            checkRead.Add(listenfd); 
            foreach (var s in clients.Values)
                checkRead.Add(s.socket);
        }

        /// <summary>
        /// 处理监听消息（客户端请求连接）
        /// </summary>
        /// <param name="listenfd"></param>
        public static void ReadListenfd(Socket listenfd)
        {
            try
            {
                Socket clientfd = listenfd.Accept();
                Console.WriteLine($"[客户端已连接]{clientfd.RemoteEndPoint}");
                ClientState state = new ClientState {socket = clientfd};
                clients.Add(clientfd, state);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("[客户端连接失败]" + ex.ToString());
            }
        }

        /// <summary>
        /// 处理客户端消息
        /// </summary>
        /// <param name="clientfd"></param>
        public static void ReadClientfd(Socket clientfd)
        {
            var state = clients[clientfd];
            var readBuff = state.readBuff;
            
            // 接收
            // 缓冲区不够，清除，若依旧不够，只能返回
            // 缓冲区长度只有1024，单条协议超过缓冲区长度时会发生错误，根据需要调整长度
            int count = 0;
            if (readBuff.Remain <= 0)
            {
                readBuff.MoveBytes();
                if (readBuff.Remain <= 0)
                {
                    Console.WriteLine("接受数据错误，消息长度超过最大上限");
                    Close(state);
                    return;
                }
            }

            try
            {
                count = clientfd.Receive(readBuff.Bytes, readBuff.WriteIndex, readBuff.Remain, 0);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Receive SocketException " + ex.ToString());
                Close(state);
                return;
            }

            // 客户端关闭
            if (count <= 0)
            {
                Console.WriteLine("Socket Close " + clientfd.RemoteEndPoint.ToString());
                Close(state);
                return;
            }

            // 消息处理
            readBuff.WriteIndex += count;
            //处理二进制消息
            OnReceiveData(state);
            //移动缓冲区
            readBuff.CheckAndMoveBytes();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="state"></param>
        public static void Close(ClientState state)
        {
            // 事件分发
            MethodInfo method = typeof(MsgHandler).GetMethod("OnDisconnect");
            object[] ob = {state};
            method.Invoke(null, ob);
            // 关闭
            state.socket.Close();
            clients.Remove(state.socket);
        }
        
        /// <summary>
        /// 数据处理
        /// </summary>
        /// <param name="state"></param>
        public static void OnReceiveData(ClientState state)
        {
            ByteArray readBuffer = state.readBuff;
            // 消息长度
            if (readBuffer.Length <= 2)
                return;

            // 获取消息体长度
            int readIdx = readBuffer.ReadIndex;
            byte[] bytes = readBuffer.Bytes;
            Int16 bodyLength = (Int16) ((bytes[readIdx + 1] << 8) | bytes[readIdx]);
            if (readBuffer.Length < bodyLength)
                return;
            readBuffer.ReadIndex += 2;
            
            // 解析协议名
            int nameCount = 0;
            string protoName = MsgBase.DecodeName(readBuffer.Bytes, readBuffer.ReadIndex, out nameCount);
            if (protoName == "")
            {
                Console.WriteLine("OnReceiveData MsgBase.DecodeNamefail");
                Close(state);
            }
            readBuffer.ReadIndex += nameCount;
            
            // 解析协议体
            int bodyCount = bodyLength - nameCount;
            MsgBase msgBase = MsgBase.Decode(protoName, readBuffer.Bytes, readBuffer.ReadIndex, bodyCount);
            readBuffer.ReadIndex += bodyCount;
            readBuffer.CheckAndMoveBytes();
            
            // 分发消息
            MethodInfo method = typeof(MsgHandler).GetMethod(protoName);
            object[] o = {state, msgBase};
            Console.WriteLine("[Receive]" + protoName);
            if (method != null)
                method.Invoke(null, o);
            else
                Console.WriteLine("OnReceiveData Invoke fail " + protoName);

            // 继续读取消息
            if (readBuffer.Length > 2)
                OnReceiveData(state);
        }
        
        /// <summary>
        /// 定时器
        /// </summary>
        public static void Timer()
        {
            // 消息分发
            MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
            object[] ob = {};
            mei.Invoke(null, ob);
        }
        
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="msg"></param>
        public static void Send(ClientState cs, MsgBase msg)
        {
            // 状态判断
            if (cs == null)
                return;
            if (!cs.socket.Connected)
                return;

            // 数据编码
            byte[] nameBytes = MsgBase.EncodeName(msg);
            byte[] bodyBytes = MsgBase.Encode(msg);
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] sendBytes = new byte[2 + len];
            
            // 组装长度
            sendBytes[0] = (byte) (len % 256);
            sendBytes[1] = (byte) (len / 256);
            
            // 组装名字
            Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
            
            // 组装消息体
            Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
            
            // 为简化代码，不设置回调
            try
            {
                cs.socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket Close on BeginSend" + ex.ToString());
            }
        }
    }
}
