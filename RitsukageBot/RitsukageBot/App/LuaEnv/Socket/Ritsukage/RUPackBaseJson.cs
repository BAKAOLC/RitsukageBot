using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket.Ritsukage
{
    public class RUPackBaseJson
    {
        public string s1 = "";
        public string s2 = "";
        public string s3 = "";
        public long n1 = 0;
        public long n2 = 0;
        public long n3 = 0;

        public RUPackBaseJson(string json = null)
        {
            if (json != null)
            {
                RUPackBaseJson data = JsonConvert.DeserializeObject<RUPackBaseJson>(json);
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
