using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Native.Csharp.App.LuaEnv.Socket
{
    class WebSocket_Client
    {
        public event Action Opened;
        public event Action<string> MessageReceived;
        public event Action<Exception> Error;
        public event Action Closed;

        private WebSocket WebSocket;

        bool _isRunning = false;

        Thread _thread;

        public bool Start(string connectTo)
        {
            if (_isRunning) return true;
            WebSocket = new WebSocket(connectTo);
            WebSocket.Opened += WebSocket_Opened;
            WebSocket.Error += WebSocket_Error;
            WebSocket.Closed += WebSocket_Closed;
            WebSocket.MessageReceived += WebSocket_MessageReceived;
            bool result = true;
            try
            {
                WebSocket.Open();
                _isRunning = true;
                _thread = new Thread(new ThreadStart(ConnectionChecker));
                _thread.Start();
            }
            catch
            {
                result = false;
            }
            return result;
        }

        void WebSocket_Opened(object sender, EventArgs e)
        {
            Opened?.Invoke();
        }

        void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(e.Message);
        }

        void WebSocket_Error(object sender, ErrorEventArgs e)
        {
            Error?.Invoke(e.Exception);
        }

        void WebSocket_Closed(object sender, EventArgs e)
        {
            Closed?.Invoke();
        }

        private void ConnectionChecker()
        {
            do
            {
                try
                {
                    if (WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Connecting)
                    {
                        WebSocket.Close();
                        WebSocket.Open();
                    }
                }
                catch
                { }
                Thread.Sleep(5000);
            } while (_isRunning);
        }

        public void SendMessage(string Message)
        {
            if (!_isRunning) return;

            Task.Factory.StartNew(() =>
            {
                if (WebSocket != null && WebSocket.State == WebSocketState.Open)
                {
                    WebSocket.Send(Message);
                }
            });
        }

        public void Dispose()
        {
            if (!_isRunning) return;

            _isRunning = false;
            try
            {
                _thread.Abort();
            }
            catch
            { }
            WebSocket?.Close();
            WebSocket?.Dispose();
            WebSocket = null;
        }
    }
}
