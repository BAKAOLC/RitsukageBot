using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

namespace Native.Csharp.App.LuaEnv.Socket
{
    class MCSocket : IDisposable
    {
        private readonly System.Net.Sockets.Socket _socket = new System.Net.Sockets.Socket(SocketType.Stream, ProtocolType.Tcp);
        private NetworkStream NetStream = null;

        public string Host { private set; get; }
        public int Port { private set; get; }
        public bool Connected { get { return _socket.Connected; } }

        public string Info { private set; get; } = string.Empty;
        public long PingTimeStamp { private set; get; } = 0;

        public MCSocket(string host, int port = 25565)
        {
            Host = host;
            Port = port;
        }

        public Exception Connect()
        {
            try
            {
                _socket.Connect(Host, Port);
            }
            catch (Exception e)
            {
                return e;
            }

            NetStream = new NetworkStream(_socket);

            if (HandShake())
            {
                try
                {
                    Ping();
                }
                catch (Exception e)
                {
                    Info = e.Message;
                    PingTimeStamp = 0;
                    _socket.Disconnect(true);
                }
            }

            return null;
        }

        public async Task<Exception> AsyncConnect() => await Task.Run(Connect);

        public Exception Disconnect()
        {
            NetStream?.Dispose();
            NetStream = null;

            try
            {
                _socket?.Disconnect(true);
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }

        public async Task<Exception> AsyncDisconnect() => await Task.Run(Disconnect);

        private bool HandShake()
        {
            using var ms = new MemoryStream();
            ms.WriteByte(0x00);
            ms.WriteVarInt(4);
            ms.WriteString(Host);
            ms.WriteAsync(BitConverter.GetBytes(Port).ToBE(), 0, 2);
            ms.WriteVarInt(1);
            Send(ms.ToArray());
            return true;
        }

        public bool Ping()
        {
            NetStream.WriteByte(0x01);
            NetStream.WriteByte(0x00);
            NetStream.FlushAsync();
            NetStream.ReadVarInt();
            int id = NetStream.ReadVarInt();
            if (id == -1)
                throw new IOException("Premature end of stream.");
            else if (id != 0x00)
                throw new IOException("Invalid packetID");
            int length = NetStream.ReadVarInt();
            if (length == -1)
                throw new IOException("Premature end of stream.");
            else if (length == 0)
                throw new IOException("Invalid string length.");
            byte[] js = new byte[length];
            NetStream.ReadAsync(js, 0, length);
            Info = Encoding.UTF8.GetString(js);
            var now = BitConverter.GetBytes(GetTimeStamp()).ToBE();
            NetStream.WriteByte(0x09);
            NetStream.WriteByte(0x01);
            NetStream.WriteAsync(now, 0, now.Length);
            NetStream.FlushAsync();
            NetStream.ReadVarInt();
            id = NetStream.ReadVarInt();
            if (id == -1)
                throw new IOException("Premature end of stream.");
            else if (id != 0x01)
                throw new IOException("Invalid packetID");
            var tbf = new byte[8];
            NetStream.ReadAsync(tbf, 0, 8);
            PingTimeStamp = BitConverter.ToInt64(tbf.Reverse().ToArray(), 0);
            return true;
        }

        private void Send(byte[] buffer)
        {
            NetStream.WriteVarInt(buffer.Length);
            NetStream.WriteAsync(buffer, 0, buffer.Length);
            NetStream.FlushAsync();
        }

        public static long GetTimeStamp()
            => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

        ~MCSocket() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {}
                Disconnect();
                _socket?.Dispose();
                m_disposed = true;
            }
        }
        private bool m_disposed;
    }

    public static class MCStreamUtils
    {

        public static int ReadVarInt(this Stream stream)
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                read = (byte)stream.ReadByte();
                int value = (read & 0b01111111);
                result |= value << (7 * numRead);
                numRead++;
                if (numRead > 5)
                    throw new Exception("VarInt is too big");
            } while ((read & 0b10000000) != 0);
            return result;
        }

        public static long ReadVarLong(this Stream stream)
        {
            int numRead = 0;
            long result = 0;
            byte read;
            do
            {
                read = (byte)stream.ReadByte();
                int value = (read & 0b01111111);
                result |= value << (7 * numRead);
                numRead++;
                if (numRead > 10)
                    throw new Exception("VarInt is too big");
            } while ((read & 0b10000000) != 0);
            return result;
        }

        public static string ReadString(this Stream stream, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            int length = stream.ReadVarInt();
            var buffer = new byte[length];
            stream.ReadAsync(buffer, 0, length);
            return encoding.GetString(buffer);
        }

        public static void WriteVarInt(this Stream stream, int value)
        {
            do
            {
                byte temp = (byte)(value & 0b01111111);
                value = value.RightMove(7);
                if (value != 0)
                {
                    temp |= 0b10000000;
                }
                stream.WriteByte(temp);
            } while (value != 0);
        }

        public static void WriteVarLong(this Stream stream, long value)
        {
            do
            {
                byte temp = (byte)(value & 0b01111111);
                value = value.RightMove(7);
                if (value != 0)
                {
                    temp |= 0b10000000;
                }
                stream.WriteByte(temp);
            } while (value != 0);
        }

        public static void WriteString(this Stream stream, string text, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var buffer = encoding.GetBytes(text);
            stream.WriteVarInt(buffer.Length);
            stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public static class BitUtils
    {
        public static byte[] ToBE(this byte[] b)
        {
            if (BitConverter.IsLittleEndian)
                return b.Reverse().ToArray();
            else
                return b;
        }

        public static int RightMove(this int value, int pos)
        {
            if (pos != 0)
            {
                int mask = int.MaxValue;
                value >>= 1;
                value &= mask;
                value >>= pos - 1;
            }
            return value;
        }

        public static long RightMove(this long value, int pos)
        {
            if (pos != 0)
            {
                long mask = long.MaxValue;
                value >>= 1;
                value &= mask;
                value >>= pos - 1;
            }
            return value;
        }
    }
}
