using Tools.BitConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Native.Csharp.App.LuaEnv.Bilibili.BilibiliLiveDanmaku_Events;

namespace Native.Csharp.App.LuaEnv.Bilibili
{
    class BilibiliLiveDanmaku_Socket
    {
        private readonly bool IsDebug = false;

        private readonly string[] DefaultHosts = new string[] { "livecmt-2.bilibili.com", "livecmt-1.bilibili.com" };
        private string ChatHost = "chat.bilibili.com";
        private int ChatPort = 2243;
        private readonly string CIDInfoUrl = "https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id=";
        private readonly short ProtocolVersion = 1;

        public int RoomID;

        public bool IsConnected = false;
        private TcpClient Client;
        private NetworkStream NetStream;
        private readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

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
            var token = "";
            try
            {
                if (IsConnected)
                    return true;
                try
                {
                    var req = await _httpClient.GetStringAsync(CIDInfoUrl + RoomID);
                    var roomobj = JObject.Parse(req);
                    token = roomobj["data"]["token"] + "";
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
            if (await SendJoinChannel(RoomID, token))
            {
                IsConnected = true;
                _ = HeartbeatLoop();
                _ = ReceiveMessageLoop();
                Connected?.Invoke(this, new SocketConnectedArgs() {
                    RoomID = RoomID
                });
                return true;
            }
            return false;
        }

        private async Task ReceiveMessageLoop()
        {

            try
            {
                var stableBuffer = new byte[16];
                var buffer = new byte[4096];
                while (IsConnected)
                {
                    await NetStream.ReadBAsync(stableBuffer, 0, 16);
                    var protocol = DanmakuProtocol.FromBuffer(stableBuffer);
                    if (protocol.PacketLength < 16)
                    {
                        throw new NotSupportedException("协议失败: (L:" + protocol.PacketLength + ")");
                    }
                    var payloadlength = protocol.PacketLength - 16;
                    if (payloadlength == 0)
                    {
                        continue;
                    }

                    buffer = new byte[payloadlength];
                    await NetStream.ReadBAsync(buffer, 0, payloadlength);
                    if (protocol.Version == 2 && protocol.Action == 5)
                    {
                        using var ms = new MemoryStream(buffer, 2, payloadlength - 2);
                        using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
                        var headerbuffer = new byte[16];
                        try
                        {
                            while (true)
                            {
                                await deflate.ReadBAsync(headerbuffer, 0, 16);
                                var protocol_in = DanmakuProtocol.FromBuffer(headerbuffer);
                                payloadlength = protocol_in.PacketLength - 16;
                                var danmakubuffer = new byte[payloadlength];
                                await deflate.ReadBAsync(danmakubuffer, 0, payloadlength);
                                ProcessDanmaku(protocol.Action, danmakubuffer);
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        ProcessDanmaku(protocol.Action, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Disconnect(ex);
            }
        }

        private void ProcessDanmaku(int action, byte[] buffer)
        {
            switch (action)
            {
                case 3:
                    {
                        var viewer = EndianBitConverter.BigEndian.ToUInt32(buffer, 0);
                        ReceivedUserCount?.Invoke(this, new SocketReceivedUserCountArgs()
                        {
                            RoomID = RoomID,
                            UserCount = viewer
                        });
                        break;
                    }
                case 5:
                    {

                        var json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        if (IsDebug)
                        {
                            LogMessage?.Invoke(this, new SocketLogMessageArgs
                            {
                                RoomID = RoomID,
                                Message = json
                            });
                        }
                        try
                        {
                            var dama = new BilibiliLiveDanmaku_SocketReceiveData(json, 2);
                            ReceivedDanmaku?.Invoke(this, new SocketReceivedDanmakuArgs()
                            {
                                RoomID = RoomID,
                                Danmaku = dama
                            });
                        }
                        catch
                        {
                        }
                        break;
                    }
                case 8:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private async Task HeartbeatLoop()
        {
            try
            {
                while (IsConnected)
                {
                    await SendHeartbeatAsync();
                    await Task.Delay(30000);
                }
            }
            catch (Exception ex)
            {
                Disconnect(ex);
            }
        }

        private async Task SendHeartbeatAsync() => await SendSocketDataAsync(2);

        private async Task<bool> SendJoinChannel(int channelId, string token)
        {
            var packetModel = new { roomid = channelId, uid = 0, protover = 2, token, platform = "danmuji" };
            var playload = JsonConvert.SerializeObject(packetModel);
            await SendSocketDataAsync(7, playload);
            return true;
        }

        Task SendSocketDataAsync(int action, string body = "")
             => SendSocketDataAsync(0, 16, ProtocolVersion, action, 1, body);
        async Task SendSocketDataAsync(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
        {
            var playload = Encoding.UTF8.GetBytes(body);
            if (packetlength == 0)
            {
                packetlength = playload.Length + 16;
            }
            var buffer = new byte[packetlength];
            using var ms = new MemoryStream(buffer);
            var b = EndianBitConverter.BigEndian.GetBytes(buffer.Length);
            await ms.WriteAsync(b, 0, 4);
            b = EndianBitConverter.BigEndian.GetBytes(magic);
            await ms.WriteAsync(b, 0, 2);
            b = EndianBitConverter.BigEndian.GetBytes(ver);
            await ms.WriteAsync(b, 0, 2);
            b = EndianBitConverter.BigEndian.GetBytes(action);
            await ms.WriteAsync(b, 0, 4);
            b = EndianBitConverter.BigEndian.GetBytes(param);
            await ms.WriteAsync(b, 0, 4);
            if (playload.Length > 0)
            {
                await ms.WriteAsync(playload, 0, playload.Length);
            }
            await NetStream.WriteAsync(buffer, 0, buffer.Length);
        }

        public void Disconnect(Exception Error = null)
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            try
            {
                Client?.Close();
            }
            catch
            {
            }
            try
            {
                NetStream?.Close();
                NetStream?.Dispose();
            }
            catch
            {
            }
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
        public static async Task ReadBAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            int read = 0;
            while (read < count)
            {
                var available = await stream.ReadAsync(buffer, offset, count - read);
                if (available == 0)
                    throw new ObjectDisposedException(null);
                read += available;
                offset += available;
            }
        }
    }

    public struct DanmakuProtocol
    {
        /// <summary>
        /// 消息总长度 (协议头 + 数据长度)
        /// </summary>
        public int PacketLength;
        /// <summary>
        /// 消息头长度 (固定为16[sizeof(DanmakuProtocol)])
        /// </summary>
        public short HeaderLength;
        /// <summary>
        /// 消息版本号
        /// </summary>
        public short Version;
        /// <summary>
        /// 消息类型
        /// </summary>
        public int Action;
        /// <summary>
        /// 参数, 固定为1
        /// </summary>
        public int Parameter;

        public static DanmakuProtocol FromBuffer(byte[] buffer)
        {
            if (buffer.Length < 16) { throw new ArgumentException(); }
            return new DanmakuProtocol()
            {
                PacketLength = EndianBitConverter.BigEndian.ToInt32(buffer, 0),
                HeaderLength = EndianBitConverter.BigEndian.ToInt16(buffer, 4),
                Version = EndianBitConverter.BigEndian.ToInt16(buffer, 6),
                Action = EndianBitConverter.BigEndian.ToInt32(buffer, 8),
                Parameter = EndianBitConverter.BigEndian.ToInt32(buffer, 12),
            };
        }
    }
}
