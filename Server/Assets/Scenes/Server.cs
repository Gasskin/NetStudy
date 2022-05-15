using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Server : MonoBehaviour
{
    public class ClientState
    {
        public Socket socket;
        public byte[] buff = new byte[1024];
    }
    
    private Socket socket;
    private IPAddress ipAdr;
    private IPEndPoint ipEp;

    private Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

    private void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipAdr = IPAddress.Parse("127.0.0.1");
        ipEp = new IPEndPoint(ipAdr, 8888);
        socket.Bind(ipEp);
        socket.Listen(0);

        socket.BeginAccept(AcceptCallback, socket);
    }
    
    private void OnDestroy()
    {
        socket.Close();
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        try {
            var server = (Socket) ar.AsyncState;
            var handle = server.EndAccept(ar);

            if (clients.TryGetValue(handle, out var state)) 
            {
                state.socket = handle;
            }
            else
            {
                state = new ClientState();
                state.socket = handle;
                clients.Add(handle, state);
            }

            handle.BeginReceive(state.buff, 0, 1024, 0, ReceiveCallback, state);
            server.BeginAccept (AcceptCallback, server);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket Accept fail" + ex.ToString());
        }
    }
    
    private void ReceiveCallback(IAsyncResult ar)
    {
        try {
            var state = (ClientState) ar.AsyncState;
            var handle = state.socket;
            int count = handle.EndReceive(ar);
            //客户端关闭
            if(count == 0)
            {
                handle.Close();
                clients.Remove(handle);
                Console.WriteLine("Socket Close");
                return;
            }
            
            
            string recvStr = Encoding.Default.GetString(state.buff, 0, count);
            Debug.Log($"{handle.RemoteEndPoint}: {recvStr}");
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes($"<color=#3c4ffe>{handle.RemoteEndPoint}</color>: {recvStr}");
            
            foreach (var client in clients.Values)
            {
                client.socket.Send(sendBytes);
            }
            
            handle.BeginReceive( state.buff, 0, 1024, 0, ReceiveCallback, state);
        }
        catch (SocketException ex)
        { 
            Console.WriteLine("Socket Receive fail" + ex.ToString());
        }
    }
}

