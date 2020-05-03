using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Tools.Manager
{
    class GroupManager
    {
        private static readonly GroupList GroupList = new GroupList();

        public static GroupInfo[] List { get => GroupList.List; }
        public static int Count { get => GroupList.Count; }

        public static GroupInfo GetGroup(long id) => GroupList.GetGroup(id);
        public static GroupInfo GetGroup(Group group) => GroupList.GetGroup(group.Id);

        public static bool InGroup(long id) => GroupList.InGroup(id);
        public static bool InGroup(Group group) => InGroup(group.Id);

        public static GroupInfo[] SearchUser(long id) => GroupList.SearchUser(id);
        public static GroupInfo[] SearchUser(QQ qq) => SearchUser(qq.Id);

        public static int GetUserLevel(long groupId, long userId) => GroupList.GetUserLevel(groupId, userId);
        public static int GetUserLevel(Group group, QQ qq) => GetUserLevel(group.Id, qq.Id);

        public static void Update() => Task.Factory.StartNew(GroupList.Update);
    }
}
