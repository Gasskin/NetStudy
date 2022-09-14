using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button btn;

    void Start()
    {
        var attack = new MsgAttack();
        attack.SkillName = "龟派气功！！";

        var byteData = attack.ToByteArray();

        IMessage msg = MsgAttack.Parser.ParseFrom(byteData);
        var data = (MsgAttack) msg;
        Debug.Log(data.SkillName);
    }
}
