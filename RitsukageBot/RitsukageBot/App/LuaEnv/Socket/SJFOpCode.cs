using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket
{
	/// <summary>
	/// 正邪Bot交互操作类型
	/// </summary>
    [DefaultValue (Notification)]
	public enum SJFOpCode
	{
		// 小律影→正邪  小律影发送 正邪接收
		// 正邪→小律影  正邪发送 小律影接收 (其实是广播方式发送, 所有连接的客户端都可以收到
		// 小律影↔正邪  都可以发送和接收

		/// <summary>
		/// 小律影↔正邪
		/// 普通通知,大部分情况下没什么用
		/// s1:通知文本
		/// </summary>
		Notification = 0,

		/// <summary>
		/// 小律影→正邪
		/// 用于身份验证，暂未使用
		/// n1:qq号(setConnect中设置的qq号)
		/// </summary>
		Verify = 1,

		/// <summary>
		/// 小律影→正邪
		/// 获取正在直播列表
		/// 不需要body
		/// </summary>
		GetLiveList = 2,

		/// <summary>
		/// 正邪→小律影
		/// 返回2的查询结果
		/// json数组
		/// 例:[{"name":"闲者","qq":"877247145","bid":12007285,"bliveRoom":1954885,"tipIn":[],"tip":[true,true,false]},{"name":"懒瘦","qq":"496276037","bid":15272850,"bliveRoom":3144622,"tipIn":[],"tip":[true,true,false]}]
		/// </summary>
		ReturnLiveList = 3,

		/// <summary>
		/// 正邪→小律影
		/// 主播开始直播
		/// n1:直播间号 s1:主播称呼
		/// </summary>
		LiveStart = 4,

		/// <summary>
		/// 正邪→小律影
		/// 主播停止直播
		/// n1:直播间号 s1:主播称呼
		/// </summary>
		LiveStop = 5,

		/// <summary>
		/// 正邪→小律影
		/// 直播观看者在直播间发送的弹幕
		/// n1:直播间号 s1:主播称呼 n2:说话者BID s2:说话者称呼,如果配置文件中没有就是用户名 s3:说话内容
		/// </summary>
		SpeakInLiveRoom = 6,

		/// <summary>
		/// 正邪→小律影
		/// up主发布新视频
		/// s1:用户名 s2:视频名 n1:AV号
		/// </summary>
		NewVideo = 7,

		/// <summary>
		/// 正邪→小律影
		/// up主发布新专栏
		/// s1:用户名 s2:专栏名 n1:CV号
		/// </summary>
		NewArtical = 8,

		/// <summary>
		/// 小律影→正邪
		/// 从称呼获得人员信息(完全匹配方式查找)
		/// s1:称呼
		/// </summary>
		GetPersonInfoByName = 9,

		/// <summary>
		/// 小律影→正邪
		/// 从qq获得人员信息(完全匹配方式查找)
		/// n1:qq号
		/// </summary>
		GetPersonInfoByQQ = 10,

		/// <summary>
		/// 小律影→正邪
		/// 从bid获得人员信息(完全匹配方式查找)
		/// n1:BID
		/// </summary>
		GetPersonInfoByBid = 11,

		/// <summary>
		/// 小律影→正邪
		/// 从直播间号获得人员信息(完全匹配方式查找)
		/// n1:直播间号
		/// </summary>
		GetPersonInfoByBiliLive = 12,

		/// <summary>
		/// 正邪→小律影
		/// 返回9|10|11|12的查询结果
		/// 例:{"name":"闲者","qq":"877247145","bid":12007285,"bliveRoom":1954885,"tipIn":[],"tip":[true,true,false]}
		/// </summary>
		ReturnPersonInfo = 13,

		/// <summary>
		/// 正邪→小律影
		/// 给指定qq号添加幻币
		/// n1:幻币数量 n2:目标qq号
		/// </summary>
		CoinsAdd = 14,

		/// <summary>
		/// 小律影↔正邪
		/// qq群中禁言
		/// n1:群号 n2:QQ号 n3:时间(秒)
		/// </summary>
		GroupBan = 15,

		/// <summary>
		/// 小律影↔正邪
		/// 踢出qq群
		/// n1:群号 n2:QQ号 n3:是否永久拒绝 0为否 1为是
		/// </summary>
		GroupKick = 16,

		/// <summary>
		/// 小律影→正邪
		/// 心跳，不需要body
		/// 返回一个操作类型为0的通知
		/// </summary>
		HeartBeat = 17,

		/// <summary>
		/// 小律影→正邪
		/// 同在QQ中的"findInAll"指令
		/// n1:qq号
		/// </summary>
		FindInAll = 18,

		/// <summary>
		/// 正邪→小律影
		/// 返回18的结果
		/// 例:[296376859,251059118]
		/// </summary>
		ReturnFind = 19,

		/// <summary>
		/// 小律影→正邪
		/// 获得"精神支柱"表情包
		/// n1:qq号
		/// </summary>
		Pic = 20,

		/// <summary>
		/// 正邪→小律影
		/// 返回20生成的jpg文件
		/// 数据部分直接保存到磁盘即可
		/// </summary>
		ReturnPic = 21,

		/// <summary>
		/// 小律影→正邪
		/// 获得"神触"表情包
		/// n1:qq号
		/// </summary>
		Pic2 = 22,

		/// <summary>
		/// 正邪→小律影
		/// 返回_22生成的jpg文件
		/// 数据部分直接保存到磁盘即可
		/// </summary>
		ReturnPic2 = 23,

		/// <summary>
		/// 小律影→正邪
		/// 获得八云蓝".jrrp"中的计算结果
		/// n1:qq号
		/// </summary>
		MD5Random = 24,

		/// <summary>
		/// 正邪→小律影
		/// 返回24的结果
		/// n1:计算结果 (0-10000的整数)
		/// </summary>
		ReturnMD5Random = 25,

		/// <summary>
		/// 小律影→正邪
		/// 获得八云蓝".draw neta"中的计算结果
		/// n1:qq号
		/// </summary>
		MD5neta = 26,

		/// <summary>
		/// 正邪→小律影
		/// 返回26的结果
		/// s1:计算结果
		/// </summary>
		ReturnMD5neta = 27,

		/// <summary>
		/// 小律影→正邪
		/// 获得八云蓝".draw music"中的计算结果
		/// n1:qq号
		/// </summary>
		MD5music = 28,

		/// <summary>
		/// 正邪→小律影
		/// 返回28的结果
		/// s1:计算结果
		/// </summary>
		ReturnMD5music = 29,

		/// <summary>
		/// 小律影→正邪
		/// 获得八云蓝".draw grandma"中的计算结果
		/// n1:qq号
		/// </summary>
		MD5grandma = 30,

		/// <summary>
		/// 正邪→小律影
		/// 返回30的结果
		/// s1:计算结果
		/// </summary>
		ReturnMD5grandma = 31,

		/// <summary>
		/// 小律影→正邪
		/// 获得八云蓝"。jrrp"中的计算结果
		/// n1:qq号
		/// </summary>
		MD5overSpell = 32,

		/// <summary>
		/// 正邪→小律影
		/// 返回32的结果
		/// s1:计算结果
		/// </summary>
		ReturnMD5overSpell = 33,

		/// <summary>
		/// 小律影→正邪
		/// 发送直播间弹幕
		/// s1:弹幕内容 s2:屑站账号cookie n1:直播间号
		/// </summary>
		SendDanmaku = 34,

		/// <summary>
		/// 小律影→正邪
		/// 设置群名片
		/// n1:群号 n2:目标qq号 s1:名片内容
		/// </summary>
		SetGroupName = 37,

		/// <summary>
		/// 小律影→正邪
		/// 设置群头衔
		/// n1:群号 n2:目标qq号 n3:有效时间，单位为秒，无限期写-1 s1:头衔内容
		/// </summary>
		SetSpecialTitle = 38,

		/// <summary>
		/// 小律影→正邪
		/// 加群审核
		/// n1:加群申请id n2:群号 n3:qq号
		/// </summary>
		GroupAdd = 35,

		/// <summary>
		/// 正邪→小律影
		/// 回复加群审核
		/// n1:加群申请id n2:是否同意(0拒绝 1同意) s1:同意或拒绝的理由 s2:此人的称呼(如果有)(优先使用.nn设置的称呼)
		/// </summary>
		ReturnGroupAdd = 36,
	}
}
