using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class Bilibili
    {
        private const string LoginPage = "https://passport.bilibili.com/login";
        private const string GetLoginUrl= "https://passport.bilibili.com/qrcode/getLoginUrl";
        private const string GetLoginInfoUrl = "https://passport.bilibili.com/qrcode/getLoginInfo";

        public static string NewLoginRequest() => Utils.HttpGet(GetLoginUrl);

        public static string GetLoginInfo(string oauthKey)
        {
            string content = $"oauthKey={oauthKey}&gourl=https%3A%2F%2Fwww.bilibili.com%2F";
            return Utils.HttpPost(GetLoginInfoUrl, content);
        }


        private const string PostDynamicUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create";
        public static string SendDynamic(string msg, string cookie = "")
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(PostDynamicUrl);
                SetHeaders(request, "app", cookie);
                request.Host = "api.vc.bilibili.com";
                request.Referer = "https://t.bilibili.com/";
                request.Headers.Add("Origin", "https://t.bilibili.com");
                long t = GetTimeStamp();
                string jct = GetJCT(cookie);
                string content = $"dynamic_id=0&type=4&rid=0&content={UrlEncode(msg)}&extension=%7B%22emoji_type%22%3A1%7D&at_uids=&ctrl=%5B%5D&csrf_token={jct}";
                return POST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Common.AppData.CQLog.Error("lua插件错误", $"post错误：{e.Message}");
            }
            return "";
        }


        public static long GetTimeStamp()
            => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        private readonly static Regex RemoveEmptyChars = new Regex(@"\s");
        private readonly static Regex MatchJCT = new Regex("(?<=bili_jct=)[^;]+");
        public static string GetJCT(string cookie = "")
            => MatchJCT.Match(RemoveEmptyChars.Replace(cookie, "")).Value;

        private static string UrlEncode(string url)
        {
            return System.Web.HttpUtility.UrlEncode(url, Encoding.UTF8);
        }

        public static void SetHeaders(HttpWebRequest request, string os = "app", string cookie = "")
        {
            request.Accept = "application/json, text/plain, */*";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.UserAgent = os switch
            {
                "app" => "Mozilla/5.0 BiliDroid/5.51.1 (bbcallen@gmail.com)",
                "pc" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/82.0.4056.0 Safari/537.36 Edg/82.0.431.0",
                _ => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36",
            };
            if (cookie != "")
                request.Headers.Add("cookie", cookie);
        }

        public static string GET(HttpWebRequest request)
        {
            request.Method = "GET";
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8";
            }
            using Stream myResponseStream = response.GetResponseStream();
            using StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding(encoding));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myStreamReader.Dispose();
            myResponseStream.Close();
            myResponseStream.Dispose();
            request.Abort();
            return retString;
        }

        public static string POST(HttpWebRequest request, string content = "")
        {
            request.Method = "POST";
            request.ContentLength = content.Length;
            byte[] byteResquest = Encoding.UTF8.GetBytes(content);
            using Stream stream = request.GetRequestStream();
            stream.Write(byteResquest, 0, byteResquest.Length);
            stream.Close();
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8";
            }
            using StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            response.Close();
            response.Dispose();
            request.Abort();
            return retString;
        }
    }
}