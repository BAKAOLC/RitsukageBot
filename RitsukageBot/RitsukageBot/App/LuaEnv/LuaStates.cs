using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class LuaStates
    {
        //虚拟机池子
        private static ConcurrentDictionary<string, LuaState> states =
            new ConcurrentDictionary<string, LuaState>();
        //池子操作锁
        private static object stateLock = new object();

        /// <summary>
        /// 添加一个触发事件
        /// 如果虚拟机不存在，则自动新建
        /// </summary>
        /// <param name="name">虚拟机名称</param>
        /// <param name="type">触发类型名</param>
        /// <param name="data">回调数据</param>
        public static void Run(string name, string type, object data)
        {
            //检查文件是否存在
            if (!File.Exists(Common.AppData.CQApi.AppDirectory + "lua/main.lua"))
            {
                Common.AppData.CQLog.Error(
                    "Lua插件报错",
                    $"错误虚拟机名称：{name}"
                );
                Common.AppData.CQLog.Error(
                    "Lua插件报错",
                    $"没有找到入口脚本文件。文件路径应在{Common.AppData.CQApi.AppDirectory}lua/main.lua"
                );
                Common.AppData.CQLog.Error(
                    "Lua插件报错",
                    $"你也可以打开插件设置页面，下载默认代码直接使用"
                );
                return;
            }
            lock (stateLock)
            {
                if (!states.ContainsKey(name))//没有的话就初始化池子
                {
                    states[name] = new LuaState();
                    try
                    {
                        states[name].lua.LoadCLRPackage();
                        states[name].lua["LuaEnvName"] = name;
                        states[name].DoFile(Common.AppData.CQApi.AppDirectory + "lua/main.lua");
                        states[name].ErrorHandler += (e, err) =>
                        {
                            Common.AppData.CQLog.Error(
                                "Lua插件报错",
                                $"虚拟机运行时错误。名称：{name},错误信息：{err.Message}"
                            );
                        };
                    }
                    catch (Exception e)
                    {
                        states[name].Dispose();
                        states.TryRemove(name, out _);
                        Common.AppData.CQLog.Error(
                            "Lua插件报错",
                            $"虚拟机启动时错误。名称：{name},错误信息：{e.Message}"
                        );
                        return;
                    }
                }
                //Common.AppData.CQLog.Debug("lua插件", $"触发事件{type}");
                states[name].TriggerEvent(type, data);
            }
        }
        public static void Run(long name, string type, object data)
            => Run(name.ToString(), type, data);

        /// <summary>
        /// 清空池子
        /// </summary>
        public static void Clear()
        {
            lock (stateLock)
            {
                foreach (string k in states.Keys)
                {
                    Common.AppData.CQLog.Info("Lua插件", "已释放虚拟机" + k);
                    states.TryRemove(k, out LuaState l);
                    l.Dispose();
                }
                Common.AppData.CQLog.Info("Lua插件", "所有虚拟机均已释放");
            }
        }

        public static string[] GetList()
        {
            lock (stateLock)
            {
                return states.Keys.ToArray();
            }
        }
    }
}
