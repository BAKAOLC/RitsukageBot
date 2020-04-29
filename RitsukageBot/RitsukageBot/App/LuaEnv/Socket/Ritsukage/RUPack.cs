using Native.Csharp.App.LuaEnv.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket.Ritsukage
{
    public class RUPack
    {
        public int DataLength
        {
            get
            {
                return Encode().Length;
            }
        }

        public const short HeadLength = 18;
        public short Version = 1;
        public long TimeStamp = 0;
        public RUOpCode OpCode = RUOpCode.Notification;
        public string JsonData = "";

        public RUPack(string data = null)
        {
            if (data != null)
            {
                byte[] bs = Encoding.UTF8.GetBytes(data);
                int dataLength = ReadInt(bs, 0);
                Version = ReadShort(bs, 6);
                TimeStamp = ReadLong(bs, 8);
                OpCode = (RUOpCode)ReadShort(bs, 16);
                JsonData = data.Substring(HeadLength, dataLength - HeadLength);
            }
        }

        public RUPack(byte[] bs)
        {
            int dataLength = ReadInt(bs, 0);
            Version = ReadShort(bs, 6);
            TimeStamp = ReadLong(bs, 8);
            OpCode = (RUOpCode)ReadShort(bs, 16);
            JsonData = Encoding.UTF8.GetString(bs, HeadLength, dataLength - HeadLength);
        }

        public byte[] Encode()
        {
            ByteDataBuilder data = new ByteDataBuilder();
            data.Write(0);
            data.Write(HeadLength);
            data.Write(Version);
            data.Write(TimeStamp);
            data.Write((short)OpCode);
            data.Write(JsonData);
            data.WritePointer = 0;
            data.Write(data.Length);
            return data.GetData();
        }

        public override string ToString()
        {
            string ret = string.Format("数据包长度:{0} 数据包版本: {1} 任务标记: {2} 操作类型: {3}",
                    DataLength, Version, TimeStamp, OpCode);
            if (JsonData != "")
            {
                ret += "\nJson数据: " + JsonData;
            }
            return ret;
        }

        private short ReadShort(byte[] data, int pos)
        {
            return (short)((data[pos] & 0xff) << 0 | (data[pos + 1] & 0xff) << 8);
        }

        private int ReadInt(byte[] data, int pos)
        {
            return (data[pos] & 0xff) << 0 | (data[pos + 1] & 0xff) << 8 | (data[pos + 2] & 0xff) << 16 | (data[pos + 3] & 0xff) << 24;
        }

        private long ReadLong(byte[] data, int pos)
        {
            return ((data[pos] & 0xffL) << 0) | (data[pos + 1] & 0xffL) << 8 | (data[pos + 2] & 0xffL) << 16 | (data[pos + 3] & 0xffL) << 24 | (data[pos + 4] & 0xffL) << 32 | (data[pos + 5] & 0xffL) << 40 | (data[pos + 6] & 0xffL) << 48 | (data[pos + 7] & 0xffL) << 56;
        }
    }
}
