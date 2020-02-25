using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Native.Csharp.App.LuaEnv.BilibiliLiveDanmaku_Events;

namespace Native.Csharp.App.LuaEnv
{
    class BiliLive
    {
        private const string PostDanmakuUrl = "http://api.live.bilibili.com/msg/send";

        private static ConcurrentDictionary<int, BilibiliLiveDanmaku_Socket> SocketList = new ConcurrentDictionary<int, BilibiliLiveDanmaku_Socket>();

        private static object ListLock = new object();

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

        private static Regex MatchJCT = new Regex("(?<=bili_jct=)[^;]+");
        public static void SendDanmaku(int roomid, string msg, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(PostDanmakuUrl);
                request.Method = "POST";
                request.Host = "api.live.bilibili.com";
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                request.Headers.Add("Origin", "https://live.bilibili.com");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:28.0) Gecko/20100101 Firefox/28.0";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Referer = "https://live.bilibili.com/" + roomid;
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                request.Headers.Add("cookie", cookie);
                long t = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / 1000;
                string jct = MatchJCT.Match(cookie).Value;
                string content = "color=16777215&" + "fontsize=25&" + "mode=1&" + "bubble=0&"
                    + "msg=" + UrlEncode(msg) + "&rnd=" + t + "&roomid=" + roomid
                    + "&csrf=" + jct + "&csrf_token=" + jct;
                request.ContentLength = content.Length;
                byte[] byteResquest = Encoding.UTF8.GetBytes(content);
                using Stream stream = request.GetRequestStream();
                stream.Write(byteResquest, 0, byteResquest.Length);
                stream.Close();
                request.GetResponse().Close();
            }
            catch (Exception e)
            {
                Common.AppData.CQLog.Error("lua插件错误", $"post错误：{e.Message}");
            }
            finally
            {
                request?.Abort();
            }
        }

        private static string UrlEncode(string url)
        {
            return System.Web.HttpUtility.UrlEncode(url, Encoding.UTF8);
        }

        private static void SocketConnected(object sender, SocketConnectedArgs e)
        {
            Common.AppData.CQLog.Info("Bilibili Live Danmaku", $"Room {e.RoomID} connected");
            LuaEnv.LuaStates.Run("Bilibili Live Danmaku", "SocketConnected", new
            {
                e.RoomID
            });
        }

        private static void SocketReceivedDanmaku(object sender, SocketReceivedDanmakuArgs e)
        {
            if (e.Danmaku.Type == BilibiliLiveDanmaku_SocketReceiveDataType.Comment)
            {
                Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku", $"Room {e.Danmaku.RoomID} received danmaku:");
                Common.AppData.CQLog.InfoReceive("Bilibili Live Danmaku", e.Danmaku.CommentText);
            }
            LuaEnv.LuaStates.Run("Bilibili Live Danmaku", "ReceivedDanmaku", new
            {
                e.RoomID,
                e.Danmaku
            });
        }

        private static void SocketReceivedUserCount(object sender, SocketReceivedUserCountArgs e)
        {
            LuaEnv.LuaStates.Run("Bilibili Live Danmaku", "ReceivedUserCount", new
            {
                e.RoomID,
                e.UserCount
            });
        }

        private static void SocketDisconnected(object sender, SocketDisconnectedArgs e)
        {
            if (e.ByError)
            {
                Common.AppData.CQLog.Error("Bilibili Live Danmaku", $"Error from {e.RoomID}:");
                Common.AppData.CQLog.Error("Bilibili Live Danmaku", e.Error.Message);
            }
            Common.AppData.CQLog.Info("Bilibili Live Danmaku", $"Room {e.RoomID} disconnected");
            LuaEnv.LuaStates.Run("Bilibili Live Danmaku", "SocketDisonnected", new
            {
                e.RoomID,
                e.ByError,
                e.Error
            });
        }

        private static void SocketLogMessage(object sender, SocketLogMessageArgs e)
        {
            Common.AppData.CQLog.Info("Bilibili Live Danmaku", $"Room {e.RoomID}: {e.Message}");
            LuaEnv.LuaStates.Run("Bilibili Live Danmaku", "SocketLogMessage", new
            {
                e.RoomID,
                e.Message
            });
        }
    }
}
