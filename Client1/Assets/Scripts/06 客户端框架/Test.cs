using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button btn;

    void Start()
    {
        btn.onClick.AddListener((() =>
        {
            NetManager.Connect("127.0.0.1", 8888);
        }));

        NetManager.AddEventListener(NetEvent.ConnectSucc, (s => { Debug.Log("connect"); }));
        NetManager.AddEventListener(NetEvent.Close, (s => { Debug.Log("close"); }));
        NetManager.AddMsgListener("MsgMove", OnMsgMove);
    }

    //收到MsgMove协议
    public void OnMsgMove(MsgBase msgBase)
    {
        MsgMove msg = (MsgMove) msgBase;
        //消息处理
        Debug.Log("OnMsgMove msg.x = " + msg.x);
        Debug.Log("OnMsgMove msg.y = " + msg.y);
        Debug.Log("OnMsgMove msg.z = " + msg.z);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var msg = new MsgMove();
            msg.x = 1;
            msg.y = 2;
            msg.z = 3;
            NetManager.Send(msg);
        }
        NetManager.Update();
    }
}