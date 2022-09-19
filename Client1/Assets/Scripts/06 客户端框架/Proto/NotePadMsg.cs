namespace Framework
{
    /// <summary>
    /// 注册协议
    /// </summary>
    public class MsgRegister : MsgBase
    {
        public override string protoName => "MsgRegister";

        // 结果
        public int result;

        // 账号
        public string account;

        // 密码
        public string password;
    }

    /// <summary>
    /// 登录协议
    /// </summary>
    public class MsgLogin : MsgBase
    {
        public override string protoName => "MsgLogin";

        // 结果
        public int result = 0;

        // 账号
        public string account = "";

        // 密码
        public string password = "";
    }

    /// <summary>
    /// 强制下线
    /// </summary>
    public class MsgKick : MsgBase
    {
        public override string protoName => "MsgKick";

        // 结果
        public int reason = 0;
    }

    /// <summary>
    /// 获取记事本内容
    /// </summary>
    public class MsgGetText : MsgBase
    {
        public override string protoName => "MsgGetText";

        public int result;
        // 服务端回
        public string text = "";
        // 账号
        public string account;
    }

    /// <summary>
    /// 保存记事本内容
    /// </summary>
    public class MsgSaveText : MsgBase
    {
        public override string protoName => "MsgSaveText";

        // 结果
        public int result = 0;
        // 客户端发
        public string text = "";
        // 账号
        public string account;
    }
}