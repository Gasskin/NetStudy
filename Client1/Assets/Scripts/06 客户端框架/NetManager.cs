using System;
using System.Collections.Generic;
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

        private static Socket client;
        private static ByteArray readBuffer;
        private static Queue<ByteArray> writeQueue;
        
        // 网络事件监听
        private static Dictionary<NetEvent, Action<string>> eventListeners = new Dictionary<NetEvent, Action<string>>();

        private static bool isConnecting = false;

        private static bool isClosing = false;
    #endregion

    #region 网络事件
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
            // No Delay
            client.NoDelay = true;
            // 正在连接
            isConnecting = true;
            // 正在关闭
            isClosing = false;
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
    }
}

