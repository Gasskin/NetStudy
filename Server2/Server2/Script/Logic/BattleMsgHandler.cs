using System;

namespace Framework
{
    public partial class  MsgHandler
    {
        public static void MsgMove(ClientState c, MsgBase msgBase)
        {
            var msg = msgBase as MsgMove;
            msg.x++;
            NetManager.Send(c, msg);
        }
    }
}
