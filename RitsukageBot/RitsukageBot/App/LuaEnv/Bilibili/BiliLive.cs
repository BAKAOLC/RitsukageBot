using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Native.Csharp.App.LuaEnv.Bilibili.BilibiliLiveDanmaku_Events;

namespace Native.Csharp.App.LuaEnv.Bilibili
{
    class BiliLive
    {
        private const string PostDanmakuUrl = "http://api.live.bilibili.com/msg/send";
        private const string StartLiveUrl = "https://api.live.bilibili.com/room/v1/Room/startLive";
        private const string StopLiveUrl = "https://api.live.bilibili.com/room/v1/Room/stopLive";
        private const string InfoUpdateUrl = "https://api.live.bilibili.com/room/v1/Room/update";
        private const string LiveRoomUrl = "https://live.bilibili.com";

        private static ConcurrentDictionary<int, BilibiliLiveDanmaku_Socket> SocketList = new ConcurrentDictionary<int, BilibiliLiveDanmaku_Socket>();

        private static readonly object ListLock = new object();

        private static async void AsyncConnect(BilibiliLiveDanmaku_Socket socket)
        {
            await socket.ConnectAsync();
        }

        public static void Connect(int roomid)
        {
            lock (ListLock)
            {
                if (!SocketList.ContainsKey(roomid))
                {
                    SocketList[roomid] = new BilibiliLiveDanmaku_Socket(roomid);
                    SocketList[roomid].Connected += SocketConnected;
                    SocketList[roomid].ReceivedDanmaku += SocketReceivedDanmaku;
                    SocketList[roomid].ReceivedUserCount += SocketReceivedUserCount;
                    SocketList[roomid].Disconnected += SocketDisconnected;
                    SocketList[roomid].LogMessage += SocketLogMessage;
                }
                try
                {
                    AsyncConnect(SocketList[roomid]);
                }
                catch
                {
                }
            }
        }

        public static void Disconnect(int roomid)
        {
            lock (ListLock)
            {
                if (SocketList.ContainsKey(roomid))
                {
                    try
                    {
                        SocketList.TryRemove(roomid, out BilibiliLiveDanmaku_Socket socket);
                        socket?.Disconnect();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static string SendDanmaku(int roomid, string msg, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(PostDanmakuUrl);
                Bilibili.SetHeaders(request, "pc", cookie);
                request.Host = "api.live.bilibili.com";
                request.Referer = "https://live.bilibili.com/" + roomid;
                request.Headers.Add("Origin", "https://live.bilibili.com");
                long t = Bilibili.GetTimeStamp();
                string jct = Bilibili.GetJCT(cookie);
                string content = $"color=16777215&fontsize=25&mode=1&bubble=0&msg={UrlEncode(msg)}&rnd={t}&roomid={roomid}&csrf={jct}&csrf_token={jct}";
                return Bilibili.POST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Common.AppData.CQLog.Error("lua插件错误", $"post错误：{e.Message}");
            }
            return "";
        }

        public static string StartLive(int roomid, int area, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(StartLiveUrl);
                Bilibili.SetHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", "https://link.bilibili.com");
                request.Referer = "https://link.bilibili.com/p/center/index";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&platform=pc&area_v2={area}&csrf_token={jct}&csrf={jct}";
                return Bilibili.POST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Common.AppData.CQLog.Error("lua插件错误", $"post错误：{e.Message}");
            }
            return "";
        }
        public static string StopLive(int roomid, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(StopLiveUrl);
                Bilibili.SetHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", "https://link.bilibili.com");
                request.Referer = "https://link.bilibili.com/p/center/index";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&platform=pc&csrf_token={jct}&csrf={jct}";
                return Bilibili.POST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Common.AppData.CQLog.Error("lua插件错误", $"post错误：{e.Message}");
            }
            return "";
        }
        public static string UpdateLiveArea(int roomid, int area, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(InfoUpdateUrl);
                Bilibili.SetHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", LiveRoomUrl);
                request.Referer = $"{LiveRoomUrl}/{roomid}";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&area_id={area}&platform=pc&csrf_token={jct}&csrf={jct}&visit_id=";
                return Bilibili.POST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Common.AppData.CQLog.Error("lua插件错误", $"post错误：{e.Message}");
            }
            return "";
        }
        public static string UpdateLiveTitle(int roomid, string title, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(InfoUpdateUrl);
                Bilibili.SetHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", LiveRoomUrl);
                request.Referer = $"{LiveRoomUrl}/{roomid}";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&title={UrlEncode(title)}&platform=pc&csrf_token={jct}&csrf={jct}&visit_id=";
                return Bilibili.POST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Common.AppData.CQLog.Error("lua插件错误", $"post错误：{e.Message}");
            }
            return "";
        }

        private static string UrlEncode(string url)
        {
            return System.Web.HttpUtility.UrlEncode(url, Encoding.UTF8);
        }

        private static void SocketConnected(object sender, SocketConnectedArgs e)
        {
            Common.AppData.CQLog.Info("Bilibili Live Danmaku", $"[Room {e.RoomID}] 已连接");
            LuaStates.Run("Bilibili Live Danmaku", "SocketConnected", new
            {
                e.RoomID
            });
        }

        private static void SocketReceivedDanmaku(object sender, SocketReceivedDanmakuArgs e)
        {
            switch (e.Danmaku.Type)
            {
                case BilibiliLiveDanmaku_SocketReceiveDataType.LiveStart:
                    Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku", $"[Room {e.RoomID}] 直播开始");
                    break;
                case BilibiliLiveDanmaku_SocketReceiveDataType.LiveEnd:
                    Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku", $"[Room {e.RoomID}] 直播结束");
                    break;
                case BilibiliLiveDanmaku_SocketReceiveDataType.Comment:
                    Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku",
                        $"[Room {e.RoomID}] {e.Danmaku.UserName}:{e.Danmaku.CommentText}");
                    break;
                case BilibiliLiveDanmaku_SocketReceiveDataType.GiftSend:
                    Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku",
                        $"[Room {e.RoomID}] {e.Danmaku.UserName} 赠送了 {e.Danmaku.GiftCount} 个 {e.Danmaku.GiftName}");
                    break;
                case BilibiliLiveDanmaku_SocketReceiveDataType.GuardBuy:
                    string type = e.Danmaku.UserGuardLevel switch
                    {
                        1 => "总督",
                        2 => "提督",
                        3 => "舰长",
                        _ => "非船员",
                    };
                    Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku",
                        $"[Room {e.RoomID}] {e.Danmaku.UserName} 赠送了 {e.Danmaku.GiftCount} 个月 {type}");
                    break;
                case BilibiliLiveDanmaku_SocketReceiveDataType.SuperChat:
                    Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku",
                        $"[Room {e.RoomID}][SuperChat][${e.Danmaku.Price}] {e.Danmaku.UserName}:{e.Danmaku.CommentText}");
                    break;
            }
            
            LuaStates.Run("Bilibili Live Danmaku", "ReceivedDanmaku", new
            {
                e.RoomID,
                e.Danmaku
            });
        }

        private static void SocketReceivedUserCount(object sender, SocketReceivedUserCountArgs e)
        {
            LuaStates.Run("Bilibili Live Danmaku", "ReceivedUserCount", new
            {
                e.RoomID,
                e.UserCount
            });
        }

        private static void SocketDisconnected(object sender, SocketDisconnectedArgs e)
        {
            if (e.ByError)
            {
                Common.AppData.CQLog.Error("Bilibili Live Danmaku", $"Error from {e.RoomID}: {e.Error.Message}");
            }
            Common.AppData.CQLog.Info("Bilibili Live Danmaku", $"[Room {e.RoomID}] 已断开连接");
            LuaStates.Run("Bilibili Live Danmaku", "SocketDisonnected", new
            {
                e.RoomID,
                e.ByError,
                e.Error
            });
        }

        private static void SocketLogMessage(object sender, SocketLogMessageArgs e)
        {
            Common.AppData.CQLog.Info("Bilibili Live Danmaku", $"[Room {e.RoomID}] {e.Message}");
            LuaStates.Run("Bilibili Live Danmaku", "SocketLogMessage", new
            {
                e.RoomID,
                e.Message
            });
        }
    }
}
