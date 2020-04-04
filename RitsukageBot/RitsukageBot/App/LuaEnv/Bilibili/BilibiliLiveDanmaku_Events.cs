using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Bilibili
{
    class BilibiliLiveDanmaku_Events
    {
        public delegate void SocketConnected(object sender, SocketConnectedArgs e);

        public delegate void SocketReceivedDanmaku(object sender, SocketReceivedDanmakuArgs e);

        public delegate void SocketReceivedUserCount(object sender, SocketReceivedUserCountArgs e);

        public delegate void SocketDisconnected(object sender, SocketDisconnectedArgs e);

        public delegate void SocketLogMessage(object sender, SocketLogMessageArgs e);

        public class SocketConnectedArgs
        {
            public int RoomID;
        }

        public class SocketReceivedUserCountArgs
        {
            public int RoomID;
            public uint UserCount;
        }

        public class SocketReceivedDanmakuArgs
        {
            public int RoomID;
            public BilibiliLiveDanmaku_SocketReceiveData Danmaku;
        }

        public class SocketDisconnectedArgs
        {
            public int RoomID;
            public bool ByError = false;
            public Exception Error;
        }

        public class SocketLogMessageArgs
        {
            public int RoomID;
            public string Message = string.Empty;
        }
    }
}
