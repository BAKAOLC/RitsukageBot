using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket
{
    class HTTPListener
    {
        private static HttpListener _listener;

        public static bool Running { private set; get; } = false;

        public static string Host { private set; get; } = "+";

        public static string Sub { private set; get; } = "";

        public static int Port { private set; get; } = -1;

        public static void Start(string host = "+", string sub = "", int port = 80)
        {
            if (Running) return;
            Running = true;
            host = host.Trim();
            host = string.IsNullOrWhiteSpace(host) ? "+" : host;
            Host = host;
            sub = sub.Trim();
            sub = string.IsNullOrWhiteSpace(sub) ? "" : (sub.EndsWith("/") ? sub : sub + "/");
            Sub = sub;
            Port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{host}:{port}/{sub}");
            _listener.Start();
            _listener.BeginGetContext(Result, null);
            Common.AppData.CQLog.Info("Http Listener", $"Open at http://{host}:{port}/{sub}");
        }

        public static void Stop()
        {
            if (!Running) return;

            Common.AppData.CQLog.Info("Http Listener", "Close");
            Running = false;
            Host = "+";
            Sub = "";
            Port = -1;
            _listener.Stop();
            _listener = null;
        }

        private static readonly Regex paramKVMatch = new Regex(@"(?<=\?).+");

        private static void Result(IAsyncResult ar)
        {
            _listener.BeginGetContext(Result, null);
            var guid = Guid.NewGuid().ToString();
            var context = _listener.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            response.StatusCode = 200;
            response.ContentType = "text/plain;charset=UTF-8";
            response.AddHeader("Content-type", "text/plain");
            response.ContentEncoding = Encoding.UTF8;
            switch (request.HttpMethod)
            {
                case "GET":
                    NameValueCollection query = new NameValueCollection();
                    string p = paramKVMatch.Match(request.RawUrl).Value;
                    string[] param = p.Split('&');
                    for (int i = 0; i < param.Length; i++)
                    {
                        string[] kv = param[i].Split('=');
                        query.Add(System.Web.HttpUtility.UrlDecode(kv[0], Encoding.UTF8),
                            System.Web.HttpUtility.UrlDecode(kv[1], Encoding.UTF8));
                    }
                    LuaEnv.LuaStates.Run("http", "HttpGet", new
                    {
                        request,
                        query,
                        response,
                    });
                    break;
                case "POST":
                    Stream stream = context.Request.InputStream;
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string data = reader.ReadToEnd();
                    LuaEnv.LuaStates.Run("http", "HttpPost", new
                    {
                        request,
                        data,
                        response,
                    });
                    break;
            }
        }

        public static void Response(HttpListenerResponse response, string content = "")
        {
            using StreamWriter writer = new StreamWriter(response.OutputStream, Encoding.UTF8);
            writer.Write(content);
            writer.Close();
            response.Close();
        }
    }
}
