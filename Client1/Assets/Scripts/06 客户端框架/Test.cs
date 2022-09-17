using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button connect;
    public Button move;

    void Start()
    {
        NetManager.AddMsgListener("MsgMove",(msg =>
        {
            var move = msg as MsgMove;
            Debug.Log($"Move {move.x} {move.y} {move.z}");
        }));
        
        connect.onClick.AddListener((() =>
        {
            NetManager.Connect("127.0.0.1", 8765);
        }));
        
        move.onClick.AddListener((() =>
        {
            var msg = new MsgMove();
            msg.x = 1;
            msg.y = 2;
            msg.z = 3;

            NetManager.Send(msg);
        }));
    }

    private void Update()
    {
        NetManager.Update();
    }
}


