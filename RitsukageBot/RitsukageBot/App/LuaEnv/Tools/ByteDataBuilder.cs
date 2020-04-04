using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Tools
{
    class ByteDataBuilder
    {
        private List<byte> data = new List<byte>();

		public int WritePointer = 0;

		public int Length
		{
			get
			{
				return data.Count;
			}
		}

		public void Write(short value) => Write(GetBytes(value));
		public void Write(int value) => Write(GetBytes(value));
		public void Write(long value) => Write(GetBytes(value));
		public void Write(string value) => Write(GetBytes(value));

		public byte[] GetData() => data.ToArray<byte>();

		private void Write(byte[] bs)
		{
			for (int i = 0; i < bs.Length; ++i)
			{
				data.Insert(WritePointer++, bs[i]);
			}
		}

        private byte[] GetBytes(short value)
        {
            byte[] bs = new byte[2];
            bs[0] = (byte)((value >> 0) & 0xff);
            bs[1] = (byte)((value >> 8) & 0xff);
            return bs;
		}

		private byte[] GetBytes(int value)
		{
			byte[] bs = new byte[4];
			bs[0] = (byte)((value >> 0) & 0xff);
			bs[1] = (byte)((value >> 8) & 0xff);
			bs[2] = (byte)((value >> 16) & 0xff);
			bs[3] = (byte)((value >> 24) & 0xff);
			return bs;
		}

		private byte[] GetBytes(long value)
		{
			byte[] bs = new byte[8];
			bs[0] = (byte)((value >> 0) & 0xff);
			bs[1] = (byte)((value >> 8) & 0xff);
			bs[2] = (byte)((value >> 16) & 0xff);
			bs[3] = (byte)((value >> 24) & 0xff);
			bs[4] = (byte)((value >> 32) & 0xff);
			bs[5] = (byte)((value >> 40) & 0xff);
			bs[6] = (byte)((value >> 48) & 0xff);
			bs[7] = (byte)((value >> 56) & 0xff);
			return bs;
		}

		private byte[] GetBytes(string value)
		{
			return Encoding.UTF8.GetBytes(value);
		}
	}
}
