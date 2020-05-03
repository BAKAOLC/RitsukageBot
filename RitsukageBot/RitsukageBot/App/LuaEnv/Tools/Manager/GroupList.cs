using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Tools.Manager
{
    class GroupList
    {
        private readonly object _lock = new object();

        private ConcurrentBag<GroupInfo> _List;
        public GroupInfo[] List { get => _List.ToArray(); }
        public int Count { get => _List.Count; }

        public GroupList() => Task.Factory.StartNew(Update);

        public void Update()
        {
            lock (_lock)
            {
                Common.AppData.CQLog.Info("GroupList", "开始更新群组列表");
                _List = new ConcurrentBag<GroupInfo>();
                var list = Common.AppData.CQApi.GetGroupList();
                foreach (var pop in list)
                {
                    _List.Add(new GroupInfo(pop.Group));
                }
                Common.AppData.CQLog.Info("GroupList", $"更新完成，共 {_List.Count} 个群组");
            }
        }

        public GroupInfo GetGroup(long id)
        {
            lock (_lock)
            {
                foreach (var pop in _List)
                {
                    if (pop.Id == id)
                        return pop;
                }
                return null;
            }
        }
        public GroupInfo GetGroup(Group group) => GetGroup(group.Id);

        public bool InGroup(long id) => GetGroup(id) != null;
        public bool InGroup(Group group) => InGroup(group.Id);

        public GroupInfo[] SearchUser(long id)
        {
            lock (_lock)
            {
                ConcurrentBag<GroupInfo> record = new ConcurrentBag<GroupInfo>();
                foreach (var pop in _List)
                {
                    if (pop.HasUser(id))
                        record.Add(pop);
                }
                return record.ToArray();
            }
        }
        public GroupInfo[] SearchUser(QQ qq) => SearchUser(qq.Id);

        public int GetUserLevel(long groupId, long userId)
        {
            if (InGroup(groupId))
                return GetGroup(groupId).GetUserLevel(userId);
            else
                return -1;
        }
        public int GetUserLevel(Group group, QQ qq) => GetUserLevel(group.Id, qq.Id);
    }
}
