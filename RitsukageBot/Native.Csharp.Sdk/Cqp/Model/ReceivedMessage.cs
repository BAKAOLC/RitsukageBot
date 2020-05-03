using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
    public class ReceivedMessage
    {
        public Group Group = null;

        public QQ QQ = null;

        public QQMessage Message = null;

        public string Type = string.Empty;

        public ReceivedMessage(CQApi api, long qqId, int id, string msg, bool isRegex = false)
        {
            Type = "private";
            QQ = new QQ(api, qqId);
            Message = new QQMessage(api, id, msg, isRegex);
        }

        public ReceivedMessage(CQApi api, long groupId, long qqId, int id, string msg, bool isRegex = false)
        {
            Type = "group";
            Group = new Group(api, groupId);
            QQ = new QQ(api, qqId);
            Message = new QQMessage(api, id, msg, isRegex);
        }
    }
}
