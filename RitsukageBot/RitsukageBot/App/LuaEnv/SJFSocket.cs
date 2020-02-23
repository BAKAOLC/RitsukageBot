using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class SJFSocket
    {
        static WebSocket_Client _client = null;
        static bool _isRunning = false;

        public SJFSocket()
        {
        }

        private static void Client_Opened()
        {
            Common.AppData.CQLog.Info("SJF Socket", "Opened");
            LuaEnv.LuaStates.Run("sjf", "SJFSocketOpened", new { });
        }

        private static void Client_MessageReceived(string msg)
        {
            SJFDataPack data = new SJFDataPack(msg);
            Common.AppData.CQLog.InfoReceive("SJF Socket", data.ToString());
            LuaEnv.LuaStates.Run("sjf", "SJFSocketData", new
            {
                data
            });
        }

        private static void Client_Error(Exception e)
        {
            Common.AppData.CQLog.Error("SJF Socket", e.ToString());
            LuaEnv.LuaStates.Run("sjf", "SJFSocketError", new {
                e
            });
        }

        private static void Client_Closed()
        {
            Common.AppData.CQLog.Info("SJF Socket", "Closed");
            LuaEnv.LuaStates.Run("sjf", "SJFSocketClosed", new { });
        }

        public static bool Start()
        {
            if (_isRunning) return true;

            _isRunning = true;

            if (_client == null)
            {
                _client = new WebSocket_Client();
                _client.Opened += Client_Opened;
                _client.MessageReceived += Client_MessageReceived;
                _client.Error += Client_Error;
                _client.Closed += Client_Closed;
            }

            var result = _client.Start(Utils.setting.SJFSocketConnect);
            if (result)
                Common.AppData.CQLog.Info("SJF Socket", $"Start to connect {Utils.setting.SJFSocketConnect}");

            return result;
        }

        public static void Stop()
        {
            if (!_isRunning) return;

            Common.AppData.CQLog.Info("SJF Socket", "Stop");

            _client?.Dispose();
            _client = null;
            _isRunning = false;
        }

        public static void SendMessage(SJFDataPack data)
        {
            if (!_isRunning) return;

            _client.SendMessage(Encoding.UTF8.GetString(data.Encode()));
        }
    }
}