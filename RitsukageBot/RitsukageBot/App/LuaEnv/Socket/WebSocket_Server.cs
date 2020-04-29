using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperWebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket
{
    class WebSocket_Server
    {
        public event Action<WebSocketSession, string> MessageReceived;
        public event Action<WebSocketSession, byte[]> DataReceived;
        public event Action<WebSocketSession> NewConnected;
        public event Action<WebSocketSession> Closed;

        public WebSocketServer WebSocket;

        bool _isRunning = false;

        public WebSocket_Server()
        {
        }

        public bool Start(int port, string serverName, bool isUseCertificate = false, string serverStoreName = "", string serverSecurity = "", string serverThumbprint = "")
        {
            bool isSetuped = false;
            try
            {
                WebSocket = new WebSocketServer();
                var serverConfig = new ServerConfig
                {
                    Name = serverName,
                    MaxConnectionNumber = 10000,
                    Mode = SocketMode.Tcp,
                    Port = port,
                    ClearIdleSession = false,
                    ClearIdleSessionInterval = 120,
                    ListenBacklog = 10,
                    ReceiveBufferSize = 64 * 1024,
                    SendBufferSize = 64 * 1024,
                    KeepAliveInterval = 1,
                    KeepAliveTime = 60,
                    SyncSend = false
                };
                SocketServerFactory socketServerFactory = null;
                //开启wss 使用证书
                if (isUseCertificate)
                {
                    serverConfig.Security = serverSecurity;
                    serverConfig.Certificate = new CertificateConfig
                    {
                        StoreName = serverStoreName,
                        StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                        Thumbprint = serverThumbprint
                    };
                    socketServerFactory = new SocketServerFactory();
                }
                WebSocket.Setup(new RootConfig(), serverConfig, socketServerFactory);
                WebSocket.NewSessionConnected += NewSessionConnected;
                WebSocket.NewMessageReceived += NewMessageReceived;
                WebSocket.NewDataReceived += NewDataReceived;
                WebSocket.SessionClosed += SessionClosed;

                isSetuped = WebSocket.Start();

                if (isSetuped)
                {
                    _isRunning = true;
                }
            }
            catch
            { }
            return isSetuped;
        }

        void NewMessageReceived(WebSocketSession session, string value)
        {
            MessageReceived?.Invoke(session, value.ToString());
        }
        
        void NewDataReceived(WebSocketSession session, byte[] data)
        {
            DataReceived?.Invoke(session, data);
        }

        void NewSessionConnected(WebSocketSession session)
        {
            NewConnected?.Invoke(session);
        }

        void SessionClosed(WebSocketSession session, CloseReason value)
        {
            Closed?.Invoke(session);
        }

        public void Dispose()
        {
            if (!_isRunning) return;

            _isRunning = false;
            foreach (WebSocketSession session in WebSocket.GetAllSessions())
            {
                session.Close();
            }
            try
            {
                WebSocket.Stop();
            }
            catch { }
        }

        public void SendMessage(WebSocketSession session, string message)
        {
            if (!_isRunning) return;

            Task.Factory.StartNew(() => {
                if (session != null && session.Connected)
                    session.Send(message);
            });
        }

        public void SendMessage(WebSocketSession session, byte[] data)
        {
            if (!_isRunning) return;

            Task.Factory.StartNew(() => {
                if (session != null && session.Connected)
                    session.Send(data, 0, data.Length);
            });
        }

        public void BoardcastMessage(string message)
        {
            if (!_isRunning) return;

            foreach (WebSocketSession session in WebSocket.GetAllSessions())
            {
                Task.Factory.StartNew(() => {
                    if (session != null && session.Connected)
                        session.Send(message);
                });
            }
        }

        public void BoardcastMessage(byte[] data)
        {
            if (!_isRunning) return;

            foreach (WebSocketSession session in WebSocket.GetAllSessions())
            {
                Task.Factory.StartNew(() => {
                    if (session != null && session.Connected)
                        session.Send(data, 0, data.Length);
                });
            }
        }
    }
}
