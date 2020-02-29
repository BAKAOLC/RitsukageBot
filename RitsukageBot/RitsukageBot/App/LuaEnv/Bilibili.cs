using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class Bilibili
    {
        private const string PostDynamicUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create";

        private static Regex MatchJCT = new Regex("(?<=bili_jct=)[^;]+");
        public static string SendDynamic(string msg, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(PostDynamicUrl);
                request.Method = "POST";
                request.Host = "api.vc.bilibili.com";
                request.Accept = "application/json, text/plain, */*";
                request.Headers.Add("Origin", "https://t.bilibili.com");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Referer = "https://t.bilibili.com/";
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                request.Headers.Add("cookie", cookie);
                long t = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / 1000;
                string jct = MatchJCT.Match(cookie).Value;
                string content = "dynamic_id=0&type=4&rid=0&content=" + UrlEncode(msg)
                    + "&extension=%7B%22emoji_type%22%3A1%7D&at_uids=&ctrl=%5B%5D&csrf_token=" + jct;
                request.ContentLength = content.Length;
                byte[] byteResquest = Encoding.UTF8.GetBytes(content);
                using Stream stream = request.GetRequestStream();
                stream.Write(byteResquest, 0, byteResquest.Length);
                stream.Close();
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码
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
    }
}
