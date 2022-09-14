namespace Framework
{
    public class MsgPing : MsgBase
    {
        public override string protoName => "MsgPing";
    }

    public class MsgPong : MsgBase
    {
        public override string protoName => "MsgPong";
    }
}