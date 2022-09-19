namespace Framework
{
    public partial class MsgHandler
    {
        public static void MsgRegister(ClientState c, MsgBase msgBase)
        {
            var msg = msgBase as MsgRegister;
            if (msg == null)
                return;
            // 注册成功
            msg.result = DbManager.Register(msg.account, msg.password) ? 0 : 1;
            NetManager.Send(c, msg);
        }

        public static void MsgLogin(ClientState c, MsgBase msgBase)
        {
            var msg = msgBase as MsgLogin;
            if (msg == null)
                return;
            
            // 密码校验
            if (!DbManager.CheckPassword(msg.account, msg.password))
            {
                msg.result = 1;
                NetManager.Send(c, msg);
                return;
            }

            // 不允许再次登录
            if (c.isLogin)
            {
                msg.result = 2;
                NetManager.Send(c, msg);
                return;
            }
            
            //返回协议
            msg.result = 0;
            c.isLogin = true;
            NetManager.Send(c, msg);
        }
        
        public static void MsgGetText(ClientState c, MsgBase msgBase)
        {
            var msg = (MsgGetText) msgBase;
            if (msg == null) 
                return;
            if (!c.isLogin)
            {
                msg.result = 3;
                NetManager.Send(c, msg);
                return;
            }
            // 获取text
            DbManager.GetNotePad(msg);
            NetManager.Send(c, msg);
        }
        
        public static void MsgSaveText(ClientState c, MsgBase msgBase)
        {
            MsgSaveText msg = (MsgSaveText)msgBase;
            if(msg == null) 
                return;
            if (!c.isLogin)
            {
                msg.result = 3;
                NetManager.Send(c, msg);
                return;
            }
            // 获取text
            DbManager.SaveNotePad(msg);
            NetManager.Send(c, msg);
        }
    }
}