using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket
{
    class WSClient
    {
        static WebSocket_Client _client = null;
        static bool _isRunning = false;

        public WSClient()
        {
        }

        private static void Client_Opened()
        {
            Common.AppData.CQLog.Info("WebSocket Client", "Client opened");
            LuaEnv.LuaStates.Run("ws_client", "ClientOpened", new { });
        }

        private static void Client_MessageReceived(string msg)
        {
            Common.AppData.CQLog.InfoReceive("WebSocket Client", msg);
            LuaEnv.LuaStates.Run("ws_client", "ServerMessage", new
            {
                msg
            });
        }

        private static void Client_Error(Exception e)
        {
            Common.AppData.CQLog.Error("WebSocket Client", e.ToString());
            LuaEnv.LuaStates.Run("ws_client", "ClientError", new {
                e
            });
        }

        private static void Client_Closed()
        {
            Common.AppData.CQLog.Info("WebSocket Client", "Client closed");
            LuaEnv.LuaStates.Run("ws_client", "ClientClosed", new { });
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

            var result = _client.Start(Utils.setting.WebSocketClientConnect);
            if (result)
                Common.AppData.CQLog.Info("WebSocket Client", $"Start to connect {Utils.setting.WebSocketClientConnect}");

            return result;
        }

        public static void Stop()
        {
            if (!_isRunning) return;

            Common.AppData.CQLog.Info("WebSocket Client", "Close");

            _client?.Dispose();
            _client = null;
            _isRunning = false;
        }

        public static void SendMessage(string msg)
        {
            if (!_isRunning) return;

            _client.SendMessage(msg);
        }
    }
}