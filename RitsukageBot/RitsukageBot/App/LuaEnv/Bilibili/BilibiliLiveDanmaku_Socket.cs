using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Native.Csharp.App.LuaEnv.Bilibili.BilibiliLiveDanmaku_Events;

namespace Native.Csharp.App.LuaEnv.Bilibili
{
    class BilibiliLiveDanmaku_Socket
    {
        private string[] DefaultHosts = new string[] { "livecmt-2.bilibili.com", "livecmt-1.bilibili.com" };
        private string ChatHost = "chat.bilibili.com";
        private int ChatPort = 2243;
        private string CIDInfoUrl = "https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id=";
        private short ProtocolVersion = 1;

        public int RoomID;

        public bool IsConnected = false;
        private TcpClient Client;
        private NetworkStream NetStream;
        private HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
        private Thread MessageReceiveThread;

        public event SocketConnected Connected;
        public event SocketReceivedDanmaku ReceivedDanmaku;
        public event SocketDisconnected Disconnected;
        public event SocketReceivedUserCount ReceivedUserCount;
        public event SocketLogMessage LogMessage;

        public BilibiliLiveDanmaku_Socket(int roomid)
        {
            RoomID = roomid;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (IsConnected)
                    return true;
                try
                {
                    var req = await _httpClient.GetStringAsync(CIDInfoUrl + RoomID);
                    var roomobj = JObject.Parse(req);
                    ChatHost = roomobj["data"]["host"] + "";
                    ChatPort = roomobj["data"]["port"].Value<int>();
                    if (string.IsNullOrEmpty(ChatHost))
                        throw new Exception();
                }
                catch (WebException ex)
                {
                    ChatHost = DefaultHosts[new Random().Next(DefaultHosts.Length)];
                    HttpWebResponse e = ex.Response as HttpWebResponse;
                    if (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        LogMessage?.Invoke(this, new SocketLogMessageArgs()
                        {
                            RoomID = RoomID,
                            Message = "该直播间疑似不存在，请检查房间号是否正确"
                        });
                    }
                    else
                    {
                        LogMessage?.Invoke(this, new SocketLogMessageArgs()
                        {
                            RoomID = RoomID,
                            Message = "B站服务器响应弹幕服务器地址出错，尝试使用常见地址连接"
                        });
                    }
                }
                catch (Exception)
                {
                    ChatHost = DefaultHosts[new Random().Next(DefaultHosts.Length)];
                    LogMessage?.Invoke(this, new SocketLogMessageArgs()
                    {
                        RoomID = RoomID,
                        Message = "获取弹幕服务器地址时出现未知错误，尝试使用常见地址连接"
                    });
                }
            }
            catch
            {
                return false;
            }
            Client = new TcpClient();
            await Client.ConnectAsync(ChatHost, ChatPort);
            NetStream = Client.GetStream();
            if (SendJoinChannel())
            {
                IsConnected = true;
                HeartbeatLoop();
                MessageReceiveThread = new Thread(ReceiveMessageLoop)
                {
                    IsBackground = true
                };
                MessageReceiveThread.Start();
                Connected?.Invoke(this, new SocketConnectedArgs() {
                    RoomID = RoomID
                });
                return true;
            }
            return false;
        }

        private async void HeartbeatLoop()
        {
            try
            {
                while (IsConnected)
                {
                    SendHeartbeatAsync();
                    await Task.Delay(30000);
                }
            }
            catch (Exception ex)
            {
                Disconnect(ex);
            }
        }

        private void SendHeartbeatAsync() => SendSocketData(2);

        private bool SendJoinChannel()
        {
            Random r = new Random();
            var tmpuid = (long)(1e14 + 2e14 * r.NextDouble());
            var packetModel = new { roomid = RoomID, uid = tmpuid };
            var playload = JsonConvert.SerializeObject(packetModel);
            SendSocketData(7, playload);
            return true;
        }

        private void ReceiveMessageLoop()
        {
            try
            {
                var stableBuffer = new byte[Client.ReceiveBufferSize];

                while (IsConnected)
                {
                    NetStream.ReadB(stableBuffer, 0, 4);
                    var packetlength = BitConverter.ToInt32(stableBuffer, 0);
                    packetlength = IPAddress.NetworkToHostOrder(packetlength);
                    if (packetlength < 16)
                    {
                        throw new NotSupportedException("协议失败: (L:" + packetlength + ")");
                    }
                    NetStream.ReadB(stableBuffer, 0, 2);
                    NetStream.ReadB(stableBuffer, 0, 2);
                    NetStream.ReadB(stableBuffer, 0, 4);
                    var typeId = BitConverter.ToInt32(stableBuffer, 0);
                    typeId = IPAddress.NetworkToHostOrder(typeId);
                    Console.WriteLine(typeId);
                    NetStream.ReadB(stableBuffer, 0, 4);
                    var playloadlength = packetlength - 16;
                    if (playloadlength == 0)
                    {
                        continue;
                    }
                    typeId = typeId - 1;
                    var buffer = new byte[playloadlength];
                    NetStream.ReadB(buffer, 0, playloadlength);
                    switch (typeId)
                    {
                        case 0:
                        case 1:
                        case 2:
                            {
                                var viewer = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0);
                                ReceivedUserCount?.Invoke(this, new SocketReceivedUserCountArgs()
                                {
                                    RoomID = RoomID,
                                    UserCount = viewer
                                });
                                break;
                            }
                        case 3:
                        case 4:
                            {
                                var json = Encoding.UTF8.GetString(buffer, 0, playloadlength);
                                try
                                {
                                    BilibiliLiveDanmaku_SocketReceiveData dama = new BilibiliLiveDanmaku_SocketReceiveData(json, 2);
                                    ReceivedDanmaku?.Invoke(this, new SocketReceivedDanmakuArgs()
                                    {
                                        RoomID = RoomID,
                                        Danmaku = dama
                                    });
                                }
                                catch (Exception)
                                {
                                }
                                break;
                            }
                        case 5:
                            {
                                break;
                            }
                        case 7:
                            {
                                break;
                            }
                        case 16:
                            {
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                Disconnect(ex);
            }
            catch (Exception ex)
            {
                Disconnect(ex);
            }
        }

        void SendSocketData(int action, string body = "")
        {
            SendSocketData(0, 16, ProtocolVersion, action, 1, body);
        }

        void SendSocketData(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
        {
            var playload = Encoding.UTF8.GetBytes(body);
            if (packetlength == 0)
            {
                packetlength = playload.Length + 16;
            }
            var buffer = new byte[packetlength];
            using (var ms = new MemoryStream(buffer))
            {
                var b = BitConverter.GetBytes(buffer.Length).ToBE();
                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(magic).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(ver).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(action).ToBE();
                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(param).ToBE();
                ms.Write(b, 0, 4);
                if (playload.Length > 0)
                {
                    ms.Write(playload, 0, playload.Length);
                }
                NetStream.WriteAsync(buffer, 0, buffer.Length);
                NetStream.FlushAsync();
            }
        }

        public void Disconnect(Exception Error = null)
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            try
            {
                MessageReceiveThread?.Abort();
            }
            catch (Exception)
            {
            }
            try
            {
                Client?.Close();
            }
            catch (Exception)
            {
            }
            try
            {
                NetStream?.Close();
                NetStream?.Dispose();
            }
            catch (Exception)
            {
            }
            MessageReceiveThread = null;
            Client = null;
            NetStream = null;
            Disconnected?.Invoke(this, new SocketDisconnectedArgs()
            {
                RoomID = RoomID,
                ByError = (Error != null),
                Error = Error
            });
        }
    }

    public static class BitUtils
    {
        public static byte[] ToBE(this byte[] b)
        {
            if (BitConverter.IsLittleEndian)
            {
                return b.Reverse().ToArray();
            }
            else
            {
                return b;
            }
        }

        public static void ReadB(this NetworkStream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            int read = 0;
            while (read < count)
            {
                var available = stream.Read(buffer, offset, count - read);
                if (available == 0)
                {
                    throw new ObjectDisposedException(null);
                }
                read += available;
                offset += available;
            }
        }
    }
}
