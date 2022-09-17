using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    public class MsgMove : MsgBase
    {
        public override string protoName => "MsgMove";

        public int x = 0;
        public int y = 0;
        public int z = 0;
    }

    public class MsgBattle : MsgBase
    {
        public override string protoName => "MsgBattle";

        public string desc = "127.0.0.1:6543";
    }
}
