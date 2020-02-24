using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class SJFDataPack
    {
        public int DataLength
        {
            get
            {
                return Encode().Length;
            }
        }

        public const short HeadLength = 26;
        public short Version = 1;
        public long TimeStamp = 0;
        public long SendTo = 0;
        public SJFOpCode OpCode = SJFOpCode.Notification;
        public string JsonData = "";

        public SJFDataPack(string data = null)
        {
            if (data != null)
            {
                byte[] bs = Encoding.UTF8.GetBytes(data);
                int dataLength = ReadInt(bs, 0);
                Version = ReadShort(bs, 6);
                TimeStamp = ReadLong(bs, 8);
                SendTo = ReadLong(bs, 16);
                OpCode = (SJFOpCode)ReadShort(bs, 24);
                JsonData = data.Substring(HeadLength, dataLength - HeadLength);
            }
        }

        public SJFDataPack(byte[] bs)
        {
            int dataLength = ReadInt(bs, 0);
            Version = ReadShort(bs, 6);
            TimeStamp = ReadLong(bs, 8);
            SendTo = ReadLong(bs, 16);
            OpCode = (SJFOpCode)ReadShort(bs, 24);
            JsonData = Encoding.UTF8.GetString(bs, HeadLength, dataLength - HeadLength);
        }

        public byte[] Encode()
        {
            ByteDataBuilder data = new ByteDataBuilder();
            data.Write(0);
            data.Write(HeadLength);
            data.Write(Version);
            data.Write(TimeStamp);
            data.Write(SendTo);
            data.Write((short)OpCode);
            data.Write(JsonData);
            data.WritePointer = 0;
            data.Write(data.Length);
            return data.GetData();
        }

        public override string ToString()
        {
            string ret = string.Format("数据包长度:{0} 数据包版本: {1} 任务标记: {2} 发送目标: {3} 操作类型: {4}",
                    DataLength, Version, TimeStamp, SendTo, OpCode);
            if (JsonData != "")
            {
                ret += string.Format("\nJson数据: {0}", JsonData);
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

    class SJFDataPack_Json
    {
        public string s1 = "";
        public string s2 = "";
        public string s3 = "";
        public long n1 = 0;
        public long n2 = 0;
        public long n3 = 0;

        public SJFDataPack_Json(string json = null)
        {
            if (json != null)
            {
                SJFDataPack_Json data = JsonConvert.DeserializeObject<SJFDataPack_Json>(json);
                s1 = data.s1;
                s2 = data.s2;
                s3 = data.s3;
                n1 = data.n1;
                n2 = data.n2;
                n3 = data.n3;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
