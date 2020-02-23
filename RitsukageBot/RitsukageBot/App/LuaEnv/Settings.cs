using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class RecordSetting
    {
        public bool TcpServerEnable = false;
        public int TcpServerPort = 20010;
        public bool WebSocketServerEnable = false;
        public int WebSocketServerPort = 20020;
        public bool WebSocketClientEnable = false;
        public string WebSocketClientConnect = "ws://localhost:20020/";
        public bool SJFSocketEnable = false;
        public string SJFSocketConnect = "ws://localhost:9961/";
    }

    [PropertyChanged.AddINotifyPropertyChangedInterface]
    class Settings
    {
        private RecordSetting record = new RecordSetting();

        private void Save()
        {
            File.WriteAllText(Common.AppData.CQApi.AppDirectory + "settings.json",
                JsonConvert.SerializeObject(record));
        }

        public long AdminQQ = 0;

        public bool TcpServerEnable
        {
            get => record.TcpServerEnable;
            set
            {
                record.TcpServerEnable = value;
                if (value)
                    TcpServer.Start();
                else
                    TcpServer.Stop();
                Save();
            }
        }
        public int TcpServerPort
        {
            get => record.TcpServerPort;
            set
            {
                if (record.TcpServerPort != value)
                {
                    record.TcpServerPort = value;
                    if (TcpServerEnable)
                    {
                        TcpServer.Stop();
                        TcpServer.Start();
                    }
                }
                Save();
            }
        }

        public bool WebSocketServerEnable
        {
            get => record.WebSocketServerEnable;
            set
            {
                record.WebSocketServerEnable = value;
                if (value)
                    WSServer.Start();
                else
                    WSServer.Stop();
                Save();
            }
        }
        public int WebSocketServerPort
        {
            get => record.WebSocketServerPort;
            set
            {
                if (record.WebSocketServerPort != value)
                {
                    record.WebSocketServerPort = value;
                    if (WebSocketServerEnable)
                    {
                        WSServer.Stop();
                        WSServer.Start();
                    }
                }
                Save();
            }
        }

        public bool WebSocketClientEnable
        {
            get => record.WebSocketClientEnable;
            set
            {
                record.WebSocketClientEnable = value;
                if (value)
                    WSClient.Start();
                else
                    WSClient.Stop();
                Save();
            }
        }
        public string WebSocketClientConnect
        {
            get => record.WebSocketClientConnect;
            set
            {
                if (record.WebSocketClientConnect != value)
                {
                    record.WebSocketClientConnect = value;
                    if (WebSocketServerEnable)
                    {
                        WSClient.Stop();
                        WSClient.Start();
                    }
                }
                Save();
            }
        }

        public bool SJFSocketEnable
        {
            get => record.SJFSocketEnable;
            set
            {
                record.SJFSocketEnable = value;
                if (value)
                    SJFSocket.Start();
                else
                    SJFSocket.Stop();
                Save();
            }
        }
        public string SJFSocketConnect
        {
            get => record.SJFSocketConnect;
            set
            {
                if (record.SJFSocketConnect != value)
                {
                    record.SJFSocketConnect = value;
                    if (SJFSocketEnable)
                    {
                        SJFSocket.Stop();
                        SJFSocket.Start();
                    }
                }
                Save();
            }
        }

        public bool MqttEnable = false;
        public string MqttServer = "";
        public int MqttPort = 0;
    }
}
