using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Native.Csharp.App.LuaEnv.Socket
{
    class SJFSocket
    {
        static WebSocket _client = null;

        static bool _isRunning = false;

        public SJFSocket()
        {
        }

        private static void Client_Opened(object sender, EventArgs e)
        {
            Common.AppData.CQLog.Info("SJF Socket", "Opened");
            LuaEnv.LuaStates.Run("sjf", "SJFSocketOpened", new
            {
                e
            });
        }

        private static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                SJFDataPack data = new SJFDataPack(e.RawData);
                Common.AppData.CQLog.InfoReceive("SJF Socket", data.ToString());
                LuaEnv.LuaStates.Run("sjf", "SJFSocketData", new
                {
                    e,
                    data
                });
            }
        }

        private static void Client_Error(object sender, ErrorEventArgs e)
        {
            Common.AppData.CQLog.Error("SJF Socket", e.Exception.ToString());
            LuaEnv.LuaStates.Run("sjf", "SJFSocketError", new {
                e
            });
        }

        private static void Client_Closed(object sender, CloseEventArgs e)
        {
            Common.AppData.CQLog.Info("SJF Socket", "Closed");
            LuaEnv.LuaStates.Run("sjf", "SJFSocketClosed", new
            {
                e
            });
        }

        public static bool Start()
        {
            if (_isRunning) return true;

            _client = new WebSocket(Utils.setting.SJFSocketConnect);
            _client.OnOpen += Client_Opened;
            _client.OnMessage += Client_MessageReceived;
            _client.OnError += Client_Error;
            _client.OnClose += Client_Closed;
            _client.Connect();
            Common.AppData.CQLog.Info("SJF Socket", $"Start to connect {Utils.setting.SJFSocketConnect}");
            _isRunning = true;

            return true;
        }

        public static void Stop()
        {
            if (!_isRunning) return;

            Common.AppData.CQLog.Info("SJF Socket", "Stop");

            _client?.Close();
            _client = null;
            _isRunning = false;
        }

        public static void Send(SJFDataPack data)
        {
            if (!_isRunning) return;

            _client.Send(data.Encode());
        }
    }
}