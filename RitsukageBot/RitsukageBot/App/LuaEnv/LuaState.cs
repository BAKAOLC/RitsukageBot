using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    public class LuaState : IDisposable
    {
        public readonly NLua.Lua lua = new NLua.Lua();

        public LuaState(object input = null)
        {
            lua.State.Encoding = Encoding.UTF8;
            lua.LoadCLRPackage();
            if (input != null)
                lua["input"] = input;
            lua["@this"] = this;
            Init();
        }

        public event EventHandler<Exception> ErrorHandler;

        private readonly object _lock = new object();

        private int asyncId = 0;
        private int GenerateId()
        {
            if (asyncId >= int.MaxValue - 100)
                asyncId = 0;
            return ++asyncId;
        }

        private readonly ConcurrentBag<LuaStateEventData> _holdingEvent = new ConcurrentBag<LuaStateEventData>();
        public void TriggerEvent(string type, object data)
        {
            if (!m_disposed)
            {
                _holdingEvent.Add(new LuaStateEventData { Type = type, Data = data });
                if (Monitor.TryEnter(_lock))
                {
                    Monitor.Exit(_lock);
                    Run();
                }
            }
        }

        private void Run()
        {
            lock (_lock)
            {
                while (_holdingEvent.Count > 0)
                {
                    try
                    {
                        _holdingEvent.TryTake(out LuaStateEventData task);
                        lua.GetFunction("EventTrigger").Call(task.Type, task.Data);
                    }
                    catch (Exception e)
                    {
                        ErrorHandler?.Invoke(lua, e);
                    }
                    if (m_disposed)
                        return;
                }
            }
        }

        private readonly ConcurrentDictionary<int, CancellationTokenSource> _timerPool = new ConcurrentDictionary<int, CancellationTokenSource>();
        public int CreateTimerTrigger(int time)
        {
            int id = GenerateId();
            CancellationTokenSource timerToken = new CancellationTokenSource();
            _timerPool.TryAdd(id, timerToken);
            var timer = new System.Timers.Timer(time);
            timer.Elapsed += (sender, e) =>
            {
                if (timerToken == null || timerToken.IsCancellationRequested || m_disposed)
                    return;
                _timerPool.TryRemove(id, out _);
                TriggerEvent("TimerTrigger", new { Id = id });
                ((System.Timers.Timer)sender).Dispose();
            };
            timer.AutoReset = false;
            timer.Start();
            return id;
        }
        public void ShutdownTimerTrigger(int id)
        {
            if (_timerPool.ContainsKey(id))
            {
                try
                {
                    _timerPool.TryRemove(id, out CancellationTokenSource tc);
                    tc.Cancel();
                }
                catch
                {
                }
            }
        }

        public object[] DoString(string s)
        {
            try
            {
                return lua.DoString(s);
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(lua, e);
                throw e;
            }
        }
        public object[] DoFile(string s)
        {
            try
            {
                return lua.DoFile(s);
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(lua, e);
                throw e;
            }
        }

        public int AsyncRun(string assembly, string type, params object[] data)
        {
            int id = GenerateId();
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.Load(assembly);
                string className = type.Substring(0, type.LastIndexOf("."));
                Type t = asm.GetType(className);
                new Thread(() =>
                {
                    try
                    {
                        string method = type.Substring(type.LastIndexOf(".") + 1,
                            type.Length - type.LastIndexOf(".") - 1);
                        List<Type> ft = new List<Type>();
                        for (int i = 0; i < data.Length; i++)
                            ft.Add(data[i].GetType());
                        object r = t.GetMethod(method, ft.ToArray()).Invoke(null, data);
                        TriggerEvent("AsyncRun", new { Id = id, Success = true, Result = r });
                    }
                    catch (Exception e)
                    {
                        ErrorHandler?.Invoke(this, e);
                        TriggerEvent("AsyncRun", new { Id = id, Success = false, Exception = e });
                    }
                }).Start();
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(this, e);
                return -1;
            }
            return id;
        }

        ~LuaState() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {}
                lua?.Dispose();
                m_disposed = true;
            }
        }
        private bool m_disposed;

        private void Init()
        {
            DoString(@"
---@type LuaState
local this = _G[""@this""]

local TimerThreadRecord = {}
local TimerIdRecord = {}
local AsyncRecord = {}
local WaitingRecord = {}
local TriggerRecord = {}

---判断任务是否存活
---@param thread thread
---@return boolean
function CheckTaskAlive(thread)
    return coroutine.status(thread) ~= ""dead""
end

---建立并执行一个协程
---@param taskData Task.InitData
---@return thread
function CreateTask(taskData, ...)
    local co = coroutine.create(taskData.func)
    if taskData.hook and taskData.hook.func and taskData.hook.mask then
        debug.sethook(co, taskData.hook.func, taskData.hook.mask, taskData.hook.count or 1)
    end
    assert(coroutine.resume(co, ...))
    return co
end

---创建延时任务
---@param ms number
---@param timerFunction function
function StartTimer(ms, timerFunction, ...)
    return CreateTask({
        func = function(...)
            TaskSleep(ms)
            timerFunction(...)
        end
    }, ...)
end

---创建循环延时任务
---@param ms number
---@param timerFunction function
function StartLoopTimer(ms, timerFunction, ...)
    return CreateTask({
        func = function(...)
            while true do
                TaskSleep(ms)
                timerFunction(...)
            end
        end
    }, ...)
end

---结束定时器任务
function TimerStop(thread)
    if TimerThreadRecord[thread] then
        this:ShutdownTimerTrigger(TimerThreadRecord[thread])
        TimerIdRecord[TimerThreadRecord[thread]] = nil
        TimerIdRecord[thread] = nil
    end
end

---结束所有定时器任务
function TimerStopAll()
    for thread in pairs(TimerThreadRecord) do
        TimerStop(thread)
    end
end

---线程休眠
---@param ms number @等待时长，最大等待126322567毫秒
---@return any
function TaskSleep(ms)
    assert(ms > 0 and math.tointeger(ms), ""等待时长必须为正整数"")
    local id = this:CreateTimerTrigger(ms)
    TimerThreadRecord[coroutine.running()] = id
    TimerIdRecord[id] = coroutine.running()
    return coroutine.yield()
end

---线程休眠并等待结果
---@param flag string
---@param ms number
---@return any
function WaitUntil(flag, ms)
    WaitingRecord[flag] = WaitingRecord[flag] or {}
    WaitingRecord[flag][coroutine.running()] = true
    if ms then
        return TaskSleep(ms)
    else
        return coroutine.yield()
    end
end

---推送Flag以激活等待线程
---@param flag string
function PushFlag(flag, ...)
    local record = WaitingRecord[flag]
    WaitingRecord[flag] = nil
    if record then
        for callback in pairs(record) do
            if type(callback) == ""function"" then
                callback(...)
            elseif type(callback) == ""thread"" then
                assert(coroutine.resume(callback, ...))
            end
        end
    end
end

---设置Event回调函数
---@param event string
---@param callback function
function SetEventTrigger(event, callback)
    TriggerRecord[event] = callback
end

---触发Event
---@param event string
---@param data object
function EventTrigger(event, data)
    if event == ""TimerTrigger"" then
        local thread = TimerIdRecord[data.Id]
        if thread then
            TimerThreadRecord[thread] = nil
            TimerIdRecord[data.Id] = nil
            coroutine.resume(thread)
        end
    elseif event == ""AsyncRun"" then
        if AsyncRecord[data.Id] then
            AsyncRecord[data.Id](data.Success, data.Success and data.Result or data.Exception)
            AsyncRecord[data.Id] = nil
        end
    elseif TriggerRecord[event] then
        TriggerRecord[event](data)
    end
end

---异步执行C#函数
---@param assembly string @程序集名称
---@param method string @函数完整路径
---@param data object @参数
---@param callback function @任务回调函数
---@return any
---@overload fun(assembly:string, method:string, data:object):any
---@overload fun(assembly:string, method:string):any
function AsyncRun(assembly, method, data, callback)
    local id = -1
    if type(data) == ""table"" then
        id = this:AsyncRun(assembly, method, table.unpack(data))
    elseif data == nil then
        id = this:AsyncRun(assembly, method)
    else
        id = this:AsyncRun(assembly, method, data)
    end
    if type(callback) == ""function"" then
        if id >= 0 then
            AsyncRecord[id] = callback
        else
            callback(false, ""load C# function fail"")
        end
    end
end

---@class Task.InitData.HookData
---@field func function
---@field mask string|'""c""'|'""r""'|'""l""'
---@field count number
local TaskInitData_HookData = {
    func = function()
    end,
    mask = ""l"",
    count = 1,
}

---@class Task.InitData
---@field func function
---@field hook Task.InitData.HookData
local TaskInitData = {
    func = function() end,
    hook = TaskInitData_HookData,
}
");
        }
    }

    public class LuaStateEventData
    {
        public string Type;
        public object Data;
    }
}
