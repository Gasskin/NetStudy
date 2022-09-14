using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

namespace Framework
{
    public enum NetEvent
    {
        ConnectSucc,
        ConnectFail,
        Close,
    }
    
    public static class NetManager
    {
    #region 字段
        // 每一次Update处理的消息量
        private static readonly int MAX_MESSAGE_FIRE = 10; 
        
        private static Socket client;
        private static ByteArray readBuffer;
        private static Queue<ByteArray> writeQueue;
        // 网络事件监听
        private static Dictionary<NetEvent, Action<string>> eventListeners = new Dictionary<NetEvent, Action<string>>();
        // 消息事件监听
        private static Dictionary<string, Action<MsgBase>> msgListeners = new Dictionary<string, Action<MsgBase>>();
        // 消息列表
        private static List<MsgBase> msgList = new List<MsgBase>();
        // 消息列表长度
        static int msgCount = 0;
        private static bool isConnecting = false;
        private static bool isClosing = false;
        // 是否启用心跳
        public static bool isUsePing = true;
        // 心跳间隔时间
        public static int pingInterval = 3;
        // 上一次发送PING的时间
        static float lastPingTime = 0;
        // 上一次收到PONG的时间
        static float lastPongTime = 0; 
    #endregion

    #region 事件
        /// <summary>
        /// 添加一个网络事件
        /// </summary>
        /// <param name="netEvent">事件类型</param>
        /// <param name="func">回调函数</param>
        public static void AddEventListener(NetEvent netEvent, Action<string> func)
        {
            if (eventListeners.ContainsKey(netEvent))
                eventListeners[netEvent] += func;
            else
                eventListeners[netEvent] = func;
        }

        /// <summary>
        /// 移出一个网络事件
        /// </summary>
        /// <param name="netEvent">事件类型</param>
        /// <param name="func">回调函数</param>
        public static void RemoveEventListener(NetEvent netEvent, Action<string> func)
        {
            if (eventListeners.ContainsKey(netEvent)) 
                eventListeners[netEvent] -= func;
            if(eventListeners[netEvent] == null)
                eventListeners.Remove(netEvent);
        }

        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="netEvent">事件类型</param>
        /// <param name="msg">参数</param>
        private static void FireEvent(NetEvent netEvent, String msg)
        {
            if (eventListeners.ContainsKey(netEvent))
                eventListeners[netEvent]?.Invoke(msg);
        }

        /// <summary>
        /// 添加消息监听
        /// </summary>
        /// <param name="msgName">监听的消息</param>
        /// <param name="listener">回调</param>
        public static void AddMsgListener(string msgName, Action<MsgBase> listener)
        {
            // 添加
            if (msgListeners.ContainsKey(msgName))
                msgListeners[msgName] += listener;
            // 新增
            else
                msgListeners[msgName] = listener;
        }
        
        /// <summary>
        /// 删除消息监听
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="listener"></param>
        public static void RemoveMsgListener(string msgName, Action<MsgBase> listener)
        {
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName] -= listener;
                if (msgListeners[msgName] == null)
                {
                    msgListeners.Remove(msgName);
                }
            }
        }

        /// <summary>
        /// 派发消息
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="msgBase"></param>
        private static void FireMsg(string msgName, MsgBase msgBase)
        {
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName](msgBase);
            }
        }
    #endregion

    #region 连接
        /// <summary>
        /// 请求连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void Connect(string ip, int port)
        {
            if (client != null && client.Connected) 
            {
                Debug.LogError("已连接，禁止重复连接");
                return;
            }

            if (isConnecting)
            {
                Debug.LogError("正在连接中，请等待");
                return;
            }

            if (isClosing)
            {
                Debug.LogError("正在关闭中，请等待");
                return;
            }
            
            // Socket
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 接收缓冲区
            readBuffer = new ByteArray();
            // 写入队列
            writeQueue = new Queue<ByteArray>();
            // 消息列表
            msgList = new List<MsgBase>();
            // 消息列表的长度
            msgCount = 0;
            // No Delay
            client.NoDelay = true;
            // 正在连接
            isConnecting = true;
            // 正在关闭
            isClosing = false;
            // 心跳
            lastPingTime = Time.time;
            lastPongTime = Time.time;
            
            //监听PONG协议
            if(!msgListeners.ContainsKey("MsgPong"))
                AddMsgListener("MsgPong", OnMsgPong);
            
            client.BeginConnect(ip, port, ConnectCallback, client);
        }

        /// <summary>
        /// Connect回调
        /// </summary>
        /// <param name="ar"></param>
        private static void ConnectCallback(IAsyncResult ar)
        {
            isConnecting = false;

            try
            {
                var socket = (Socket) ar.AsyncState;
                socket.EndConnect(ar);
                Debug.Log("连接成功");
                FireEvent(NetEvent.ConnectSucc, "");
                
                // 开始接受数据
                socket.BeginReceive( readBuffer.Bytes, readBuffer.WriteIndex, readBuffer.Remain, 0, ReceiveCallback, socket);
            }
            catch (SocketException ex)
            {
                Debug.LogError("连接错误：" + ex);
                FireEvent(NetEvent.ConnectFail, ex.ToString());
            }
        }
    #endregion

    #region Close

        /// <summary>
        /// 关闭连接
        /// </summary>
        public static void Close()
        {
            // 状态判断
            if (client == null || !client.Connected)
                return;
            if (isConnecting)
                return;

            // 还有数据在发送
            if (writeQueue.Count > 0)
            {
                isClosing = true;
            }
            // 没有数据在发送
            else
            {
                client.Close();
                FireEvent(NetEvent.Close, "");
            }
        }

    #endregion

    #region 发送

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="msg"></param>
        public static void Send(MsgBase msg)
        {
            // 状态判断
            if (client == null || !client.Connected)
                return;
            if (isConnecting)
                return;
            if (isClosing)
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
            
            // 写入队列
            ByteArray ba = new ByteArray(sendBytes);
            int count = 0; //writeQueue的长度
            lock (writeQueue)
            {
                writeQueue.Enqueue(ba);
                count = writeQueue.Count;
            }

            // send
            if (count == 1)
            {
                client.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, client);
            }
        }
        
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            // 获取state、EndSend的处理
            Socket socket = (Socket) ar.AsyncState;
            // 状态判断
            if (socket == null || !socket.Connected)
                return;
            
            // EndSend
            int count = socket.EndSend(ar);
            // 获取写入队列第一条数据 
            ByteArray ba;
            lock (writeQueue)
            {
                ba = writeQueue.First();
            }

            // 完整发送
            ba.ReadIndex += count;
            if (ba.Length == 0)
            {
                lock (writeQueue)
                {
                    writeQueue.Dequeue();
                    if (writeQueue.Count > 0) 
                        ba = writeQueue.Peek();
                }
            }

            // 继续发送
            if (ba != null)
                socket.BeginSend(ba.Bytes, ba.ReadIndex, ba.Length, 0, SendCallback, socket);
            // 正在关闭
            else if (isClosing)
                socket.Close();
        } 
    #endregion

    #region 接受
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // 获取接收数据长度
                Socket socket = (Socket) ar.AsyncState;
                int count = socket.EndReceive(ar);
                if (count == 0)
                {
                    Close();
                    return;
                }
                readBuffer.WriteIndex += count;
                
                // 处理二进制消息
                OnReceiveData();
                
                // 继续接收数据
                if (readBuffer.Remain < 8)
                {
                    readBuffer.MoveBytes();
                    readBuffer.ReSize(readBuffer.Length * 2);
                }

                socket.BeginReceive(readBuffer.Bytes, readBuffer.WriteIndex, readBuffer.Remain, 0, ReceiveCallback, socket);
            }
            catch (SocketException ex)
            {
                Debug.Log("Socket Receive fail" + ex.ToString());
            }
        }

        /// <summary>
        /// 数据处理
        /// </summary>
        public static void OnReceiveData()
        {
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
                Debug.Log("OnReceiveData MsgBase.DecodeName 失败");
                return;
            }

            readBuffer.ReadIndex += nameCount;
            // 解析协议体
            int bodyCount = bodyLength - nameCount;
            MsgBase msgBase = MsgBase.Decode(protoName, readBuffer.Bytes, readBuffer.ReadIndex, bodyCount);
            readBuffer.ReadIndex += bodyCount;
            readBuffer.CheckAndMoveBytes();
            // 添加到消息队列
            lock (msgList)
            {
                msgList.Add(msgBase);
            }

            msgCount++;
            // 继续读取消息
            if (readBuffer.Length > 2)
            {
                OnReceiveData();
            }
        }

    #endregion

    #region Update

        public static void Update()
        {
            MsgUpdate();
            PingUpdate();
        }

        /// <summary>
        /// 更新
        /// </summary>
        private static void MsgUpdate()
        {
            //初步判断，提升效率
            if (msgCount == 0)
                return;

            //重复处理消息
            for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
            {
                //获取第一条消息
                MsgBase msgBase = null;
                lock (msgList)
                {
                    if (msgList.Count > 0)
                    {
                        msgBase = msgList[0];
                        msgList.RemoveAt(0);
                        msgCount--;
                    }
                }

                //分发消息
                if (msgBase != null)
                    FireMsg(msgBase.protoName, msgBase);
                //没有消息了
                else
                    break;
            }
        }
        
        /// <summary>
        /// 心跳
        /// </summary>
        private static void PingUpdate()
        {
            // 是否启用
            if (!isUsePing)
                return;

            // 发送PING
            if (Time.time - lastPingTime > pingInterval)
            {
                Send(new MsgPing());
                lastPingTime = Time.time;
            }

            // 检测PONG时间
            if (Time.time - lastPongTime > pingInterval * 4)
                Close();
        } 
    #endregion

    #region 监听
        private static void OnMsgPong(MsgBase msgBase)
        {
            lastPongTime = Time.time;
        }
    #endregion
    }
}

