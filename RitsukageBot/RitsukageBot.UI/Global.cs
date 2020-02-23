using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitsukageBot.UI
{
    public class Global
    {
        //设置对象
        public static object Settings;

        //初始化git的回调
        public static event EventHandler<bool> GitInitial;

        public static void InitialScript()
        {
            GitInitial?.Invoke(null,true);
        }

        //初始化lua虚拟机的回调
        public static event EventHandler<bool> LuaInitial;

        public static void InitialLua()
        {
            LuaInitial?.Invoke(null, true);
        }

        //初始化xml的回调
        public static event EventHandler<bool> XmlInitial;

        public static void InitialXml()
        {
            XmlInitial?.Invoke(null, true);
        }

    }
}
