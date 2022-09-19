using Framework;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public InputField account;
    public InputField password;
    public Button login;
    public Button register;
    public InputField text;
    public Button get;
    public Button save;

    private string accountText;
    
    void Start()
    {
        NetManager.Connect("127.0.0.1", 8765);

    #region 登录

        login.onClick.AddListener((() =>
        {
            if (string.IsNullOrEmpty(account.text) || string.IsNullOrEmpty(password.text)) 
                return;
            var msg = new MsgLogin() {account = account.text, password = password.text};
            NetManager.Send(msg);
        }));
        
        NetManager.AddMsgListener("MsgLogin",(msgBase =>
        {
            var msg = msgBase as MsgLogin;
            if (msg == null) 
                return;
            switch (msg.result)
            {
                case 0:
                    accountText = msg.account;
                    break;
                case 1:
                    Debug.Log("账号或密码错误");
                    break;
                case 2:
                    Debug.Log("不允许重复登录");
                    break;
            }
        }));

    #endregion

    #region 注册
        register.onClick.AddListener((() =>
        {
            if (string.IsNullOrEmpty(account.text) || string.IsNullOrEmpty(password.text)) 
                return;
            var msg = new MsgRegister() {account = account.text, password = password.text};
            NetManager.Send(msg);
        }));
        
        NetManager.AddMsgListener("MsgRegister",(msgBase =>
        {
            var msg = msgBase as MsgRegister;
            switch (msg.result)
            {
                case 1:
                    Debug.Log("注册失败");
                    break;
            }
        }));
    #endregion

    #region 获取
        get.onClick.AddListener((() =>
        {
            var msg = new MsgGetText() {account = accountText};
            NetManager.Send(msg);
        }));

        NetManager.AddMsgListener("MsgGetText", (msgBase) =>
        {
            var msg = msgBase as MsgGetText;
            switch (msg.result)
            {
                case 0:
                    text.text = msg.text;
                    break;
                case 1:
                    Debug.Log("账号非法");
                    break;
                case 2:
                    Debug.Log("没有数据");
                    break;
                case 3:
                    Debug.Log("没有登录");
                    break;
            }
        });
    #endregion

    #region 保存
        save.onClick.AddListener((() =>
        {
            if (string.IsNullOrEmpty(text.text))
                return;
            var msg = new MsgSaveText() {account = accountText,text = text.text};
            NetManager.Send(msg);
        }));

        NetManager.AddMsgListener("MsgSaveText", (msgBase) =>
        {
            var msg = msgBase as MsgSaveText;
            switch (msg.result)
            {
                case 1:
                    Debug.Log("非法账号");
                    break;
                case 3:
                    Debug.Log("没有登录");
                    break;
            }
        });
    #endregion
    }

    private void Update()
    {
        NetManager.Update();
    }
}


