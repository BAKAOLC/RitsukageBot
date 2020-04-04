using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket
{
    class WSServer
    {
        static WebSocket_Server _server = null;
        static bool _isRunning = false;

        public WSServer()
        {
        }

        private static void Server_NewConnected(SuperWebSocket.WebSocketSession client)
        {
            Common.AppData.CQLog.Info("WebSocket Server", "New client has been connected.");
            LuaEnv.LuaStates.Run("ws_server", "ClientConnect", new {
                client
            });
        }

        private static void Server_MessageReceived(SuperWebSocket.WebSocketSession client, string msg)
        {
            Common.AppData.CQLog.InfoReceive("WebSocket Server", msg);
            LuaEnv.LuaStates.Run("ws_server", "ClientMessage", new
            {
                client,
                msg
            });
        }

        private static void Server_Closed(SuperWebSocket.WebSocketSession client)
        {
            Common.AppData.CQLog.Info("WebSocket Server", "A client has been closed.");
            LuaEnv.LuaStates.Run("ws_server", "ClientDisconnect", new
            {
                client
            });
        }

        public static bool Start()
        {
            if (_isRunning) return true;

            _isRunning = true;

            if (_server == null)
            {
                _server = new WebSocket_Server();
                _server.MessageReceived += Server_MessageReceived;
                _server.NewConnected += Server_NewConnected;
                _server.Closed += Server_Closed;
            }

            var result = _server.Start(Utils.setting.WebSocketServerPort, "Ritsukage WebSocket Server");
            if (result)
                Common.AppData.CQLog.Info("WebSocket Server", $"Start at port {Utils.setting.WebSocketServerPort}");
            return result;
        }

        public static void Stop()
        {
            if (!_isRunning) return;
            
            Common.AppData.CQLog.Info("WebSocket Server", "Close");

            _server?.Dispose();
            _server = null;
            _isRunning = false;
        }

        public static void BoardcastMessage(string msg)
        {
            if (!_isRunning) return;

            _server.BoardcastMessage(msg);
        }
    }
}
