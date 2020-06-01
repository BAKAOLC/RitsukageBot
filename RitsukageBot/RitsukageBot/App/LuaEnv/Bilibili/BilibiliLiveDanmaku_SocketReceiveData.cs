using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Bilibili
{
    public enum BilibiliLiveDanmaku_SocketReceiveDataType
    {
        /// <summary>
        /// 弹幕
        /// </summary>
        Comment,

        /// <summary>
        /// 礼物
        /// </summary>
        GiftSend,

        /// <summary>
        /// 礼物排行
        /// </summary>
        GiftTop,

        /// <summary>
        /// 欢迎老爷
        /// </summary>
        Welcome,

        /// <summary>
        /// 直播开始
        /// </summary>
        LiveStart,

        /// <summary>
        /// 直播结束
        /// </summary>
        LiveEnd,

        /// <summary>
        /// 其他
        /// </summary>
        Unknown,

        /// <summary>
        /// 欢迎船员
        /// </summary>
        WelcomeGuard,

        /// <summary>
        /// 购买船票
        /// </summary>
        GuardBuy,

        /// <summary>
        /// SC
        /// </summary>
        SuperChat,
    }

    class BilibiliLiveDanmaku_SocketReceiveData
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public BilibiliLiveDanmaku_SocketReceiveDataType Type { get; set; }

        /// <summary>
        /// 弹幕内容
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Comment"/></item>
        /// </list></para>
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// 消息触发者用户名
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Comment"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GiftSend"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Welcome"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.WelcomeGuard"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GuardBuy"/></item>
        /// </list></para>
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 消息触发者用户ID
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Comment"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GiftSend"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Welcome"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.WelcomeGuard"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GuardBuy"/></item>
        /// </list></para>
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 用户舰队等级
        /// <para>0 为非船员 1 为总督 2 为提督 3 为舰长</para>
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Comment"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.WelcomeGuard"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GuardBuy"/></item>
        /// </list></para>
        /// </summary>
        public int UserGuardLevel { get; set; }

        /// <summary>
        /// 礼物名称
        /// </summary>
        public string GiftName { get; set; }

        /// <summary>
        /// 礼物数量
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GiftSend"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GuardBuy"/></item>
        /// </list></para>
        /// <para>此字段也用于标识上船 <see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GuardBuy"/> 的数量（月数）</para>
        /// </summary>
        public int GiftCount { get; set; }

        /// <summary>
        /// SC价格
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.SuperChat"/></item>
        /// </list></para>
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// SC持续时间
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.SuperChat"/></item>
        /// </list></para>
        /// </summary>
        public int SCKeepTime { get; set; }

        /// <summary>
        /// 礼物排行
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GiftTop"/></item>
        /// </list></para>
        /// </summary>
        public List<BilibiliLiveDanmaku_GiftRank> GiftRanking { get; set; }

        /// <summary>
        /// 该用户是否为房管(包括主播)
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Comment"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.GiftSend"/></item>
        /// </list></para>
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 是否VIP用戶(老爷)
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Comment"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.Welcome"/></item>
        /// </list></para>
        /// </summary>
        public bool IsVIP { get; set; }

        /// <summary>
        /// 事件对应的房间号
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.LiveStart"/></item>
        /// <item><see cref="BilibiliLiveDanmaku_SocketReceiveDataType.LiveEnd"/></item>
        /// </list></para>
        /// </summary>
        public string RoomID { get; set; }

        /// <summary>
        /// 内部用, JSON数据版本号 通常应该是2
        /// </summary>
        public int JSON_Version { get; set; }

        /// <summary>
        /// 原始数据, 高级开发用
        /// </summary>
        [Obsolete("除非确实有需要, 请使用 RawDataJToken 避免多次解析JSON导致性能问题")]
        public string RawData { get; set; }

        /// <summary>
        /// 原始数据, 高级开发用, 如果需要用原始的JSON数据, 建议使用这个而不是用RawData
        /// </summary>
        public JToken RawDataJToken { get; set; }

        public BilibiliLiveDanmaku_SocketReceiveData()
        {
        }

        public BilibiliLiveDanmaku_SocketReceiveData(string JSON, int version = 1)
        {
#pragma warning disable CS0618 // 类型或成员已过时
            RawData = JSON;
#pragma warning restore CS0618 // 类型或成员已过时
            JSON_Version = version;
            switch (version)
            {
                case 1:
                    {
                        var obj = JArray.Parse(JSON);
                        CommentText = obj[1].ToString();
                        UserName = obj[2][1].ToString();
                        Type = BilibiliLiveDanmaku_SocketReceiveDataType.Comment;
                        RawDataJToken = obj;
                        break;
                    }
                case 2:
                    {
                        var obj = JObject.Parse(JSON);
                        RawDataJToken = obj;
                        string cmd = obj["cmd"].ToString();
                        switch (cmd)
                        {
                            case "LIVE":
                                Type = BilibiliLiveDanmaku_SocketReceiveDataType.LiveStart;
                                RoomID = obj["RoomID"].ToString();
                                break;
                            case "PREPARING":
                                Type = BilibiliLiveDanmaku_SocketReceiveDataType.LiveEnd;
                                RoomID = obj["RoomID"].ToString();
                                break;
                            case "DANMU_MSG":
                                Type = BilibiliLiveDanmaku_SocketReceiveDataType.Comment;
                                CommentText = obj["info"][1].ToString();
                                UserID = obj["info"][2][0].ToObject<int>();
                                UserName = obj["info"][2][1].ToString();
                                IsAdmin = obj["info"][2][2].ToString() == "1";
                                IsVIP = obj["info"][2][3].ToString() == "1";
                                UserGuardLevel = obj["info"][7].ToObject<int>();
                                break;
                            case "SEND_GIFT":
                                Type = BilibiliLiveDanmaku_SocketReceiveDataType.GiftSend;
                                GiftName = obj["data"]["giftName"].ToString();
                                UserName = obj["data"]["uname"].ToString();
                                UserID = obj["data"]["uid"].ToObject<int>();
                                // Giftrcost = obj["data"]["rcost"].ToString();
                                GiftCount = obj["data"]["num"].ToObject<int>();
                                break;
                            case "GIFT_TOP":
                                {
                                    Type = BilibiliLiveDanmaku_SocketReceiveDataType.GiftTop;
                                    var alltop = obj["data"].ToList();
                                    GiftRanking = new List<BilibiliLiveDanmaku_GiftRank>();
                                    foreach (var v in alltop)
                                    {
                                        GiftRanking.Add(new BilibiliLiveDanmaku_GiftRank()
                                        {
                                            UID = v.Value<int>("uid"),
                                            UserName = v.Value<string>("uname"),
                                            Coin = v.Value<decimal>("coin")

                                        });
                                    }

                                    break;
                                }
                            case "WELCOME":
                                {
                                    Type = BilibiliLiveDanmaku_SocketReceiveDataType.Welcome;
                                    UserName = obj["data"]["uname"].ToString();
                                    UserID = obj["data"]["uid"].ToObject<int>();
                                    IsVIP = true;
                                    IsAdmin = obj["data"]["isadmin"]?.ToString() == "1";
                                    break;

                                }
                            case "WELCOME_GUARD":
                                {
                                    Type = BilibiliLiveDanmaku_SocketReceiveDataType.WelcomeGuard;
                                    UserName = obj["data"]["username"].ToString();
                                    UserID = obj["data"]["uid"].ToObject<int>();
                                    UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                                    break;
                                }
                            case "GUARD_BUY":
                                {
                                    Type = BilibiliLiveDanmaku_SocketReceiveDataType.GuardBuy;
                                    UserID = obj["data"]["uid"].ToObject<int>();
                                    UserName = obj["data"]["username"].ToString();
                                    UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                                    GiftName = UserGuardLevel == 3 ? "舰长" :
                                        UserGuardLevel == 2 ? "提督" :
                                        UserGuardLevel == 1 ? "总督" : "";
                                    GiftCount = obj["data"]["num"].ToObject<int>();
                                    break;
                                }
                            case "SUPER_CHAT_MESSAGE":
                                {
                                    Type = BilibiliLiveDanmaku_SocketReceiveDataType.SuperChat;
                                    CommentText = obj["data"]["message"]?.ToString();
                                    UserID = obj["data"]["uid"].ToObject<int>();
                                    UserName = obj["data"]["user_info"]["uname"].ToString();
                                    Price = obj["data"]["price"].ToObject<decimal>();
                                    SCKeepTime = obj["data"]["time"].ToObject<int>();
                                    break;
                                }

                            default:
                                {
                                    if (cmd.StartsWith("DANMU_MSG")) // "高考"fix
                                    {
                                        Type = BilibiliLiveDanmaku_SocketReceiveDataType.Comment;
                                        CommentText = obj["info"][1].ToString();
                                        UserID = obj["info"][2][0].ToObject<int>();
                                        UserName = obj["info"][2][1].ToString();
                                        IsAdmin = obj["info"][2][2].ToString() == "1";
                                        IsVIP = obj["info"][2][3].ToString() == "1";
                                        UserGuardLevel = obj["info"][7].ToObject<int>();
                                        break;
                                    }
                                    else
                                    {
                                        Type = BilibiliLiveDanmaku_SocketReceiveDataType.Unknown;
                                    }

                                    break;
                                }
                        }
                        if (cmd.StartsWith("DANMU_MSG")) // "高考"fix
                        {
                            Type = BilibiliLiveDanmaku_SocketReceiveDataType.Comment;
                            CommentText = obj["info"][1].ToString();
                            UserID = obj["info"][2][0].ToObject<int>();
                            UserName = obj["info"][2][1].ToString();
                            IsAdmin = obj["info"][2][2].ToString() == "1";
                            IsVIP = obj["info"][2][3].ToString() == "1";
                            UserGuardLevel = obj["info"][7].ToObject<int>();
                            break;
                        }
                        break;
                    }
                default:
                    throw new Exception();
            }
        }
    }
}
