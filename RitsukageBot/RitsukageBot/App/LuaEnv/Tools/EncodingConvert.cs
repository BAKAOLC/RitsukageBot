using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Tools
{
    class EncodingConvert
    {
        public static string Convert(Encoding srcEncoding, Encoding dstEncoding, string text)
            => dstEncoding.GetString(Encoding.Convert(srcEncoding, dstEncoding, srcEncoding.GetBytes(text)));

        public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
            => Encoding.Convert(srcEncoding, dstEncoding, bytes);
    }
}
