using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Tools.Manager
{
    class GroupInfo
    {
        private readonly object _lock = new object();

        public Group Group { private set; get; }

        public QQ Owner { private set; get; }

        private ConcurrentBag<QQ> _Manager;
        public QQ[] Manager { get => _Manager.ToArray(); }

        private readonly ConcurrentDictionary<long, GroupMemberInfo> Member = new ConcurrentDictionary<long, GroupMemberInfo>();

        public long Id { get => Group.Id; }
        public string Name { get => Common.AppData.CQApi.GetGroupInfo(Group, true).Name; }
        public int MemberCount { get => Member.Count; }
        public int MaxMemberCount { get => Common.AppData.CQApi.GetGroupInfo(Group, true).MaxMemberCount; }
        public GroupMemberInfo[] MemberList
        {
            get {
                lock (_lock)
                {
                    var array = new GroupMemberInfo[MemberCount];
                    int i = 0;
                    foreach (var pop in Member)
                        array[i++] = pop.Value;
                    return array;
                }
            }
        }

        public GroupInfo(long id)
        {
            Group = new Group(Common.AppData.CQApi, id);
            Update();
        }

        public GroupInfo(Group group)
        {
            Group = group;
            Update();
        }

        public void Update()
        {
            lock (_lock)
            {
                Member.Clear();
                _Manager = new ConcurrentBag<QQ>();
                var m = Group.GetGroupMemberList();
                foreach (var pop in m)
                {
                    Member.TryAdd(pop.QQ.Id, pop);
                    switch (pop.MemberType)
                    {
                        case QQGroupMemberType.Creator:
                            Owner = pop.QQ;
                            break;
                        case QQGroupMemberType.Manage:
                            _Manager.Add(pop.QQ);
                            break;
                    }
                }
            }
        }

        public GroupMemberInfo GetUserInfo(long id) => Member[id];
        public GroupMemberInfo GetUserInfo(QQ qq) => GetUserInfo(qq.Id);

        public bool HasUser(long id) => GetUserInfo(id) != null;
        public bool HasUser(QQ qq) => HasUser(qq.Id);

        public bool IsManager(long id)
        {
            lock (_lock)
            {
                if (!HasUser(id))
                    return false;

                bool result = false;
                foreach (var pop in _Manager)
                {
                    if (pop.Id == id)
                    {
                        result = true;
                        break;
                    }
                }
                return result;
            }
        }
        public bool IsManager(QQ qq) => IsManager(qq.Id);

        public int GetUserLevel(long id)
        {
            if (!HasUser(id))
                return 0;
            else if (Owner.Id == id)
                return 3;
            else if (IsManager(id))
                return 2;
            else
                return 1;

        }
        public int GetUserLevel(QQ qq) => GetUserLevel(qq.Id);
    }
}
