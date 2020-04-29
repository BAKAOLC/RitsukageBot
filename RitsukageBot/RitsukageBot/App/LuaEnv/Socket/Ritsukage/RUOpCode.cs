using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket.Ritsukage
{
    /// <summary>
    /// 律影数据包类型
    /// </summary>
    [DefaultValue(Notification)]
    public enum RUOpCode
    {
        // SD: 指代该类型数据包的可传输方向
        // 1    Client → Server
        // 2    Server → Client
        // 3    Client ↔ Server
        // DT: 指代该类型数据包的Json格式类型
        // 1    BaseJson(即n1|n2|n3|s1|s2|s3数据)(注：布尔类型值占用n1|n2|n3数据位，0为假，1为真)
        // 2    MonthData(DayCheckInMonth)
        // ...  待添加

        /// <summary>
        /// SD: 3
        /// 广播消息
        /// DT: 1
        /// - s1: 消息内容文本
        /// </summary>
        Notification = 0,

        /// <summary>
        /// SD: 1
        /// 客户端身份验证包
        /// DT: 1
        /// - n1: QQ
        /// - s1: Code
        /// </summary>
        Verify = 10,

        /// <summary>
        /// SD: 2
        /// 服务端认证结果
        /// DT: 1
        /// - n1: 是否验证通过(Boolean)
        /// - s1: 认证信息
        /// </summary>
        VerifyResult = 11,

        /// <summary>
        /// SD: 2
        /// 用户信息更新
        /// DT: 1
        /// - s1: 用户昵称
        /// - s2: 用户头像图像链接
        /// - s3: 用户性别
        /// </summary>
        UserInfo = 12,

        /// <summary>
        /// SD: 2
        /// 签到日期表
        /// DT: 2
        /// </summary>
        SignUpCalendar = 100,
    }
}
