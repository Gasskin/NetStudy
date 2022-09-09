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
        NetManager.AddEventListener(NetEvent.ConnectSucc,(s =>
        {
            Debug.Log("连接成功的回调函数");
        }));
        
        btn.onClick.AddListener(() =>
        {
            NetManager.Connect("127.0.0.1", 8888);
        });
    }

    void Update()
    {
        
    }
}
