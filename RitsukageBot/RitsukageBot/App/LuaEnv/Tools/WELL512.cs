using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Tools
{
    class WELL512
    {
        private uint _seed;
        private uint _index = 0;
        private uint[] _state = new uint[16];

        private uint _getMillisecond()
        {
            return (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public WELL512(uint seed) => SetSeed(seed);
        public WELL512() => SetSeed(_getMillisecond());

        const uint mask = ~0u;
        public void SetSeed(uint seed)
        {
            _seed = seed;
            _index = 0;

            _state[0] = seed & mask;
            for (uint i = 1; i < 16; ++i)
            {
                _state[i] = (uint)((1812433253UL * (_state[i - 1] ^ (_state[i - 1] >> 30)) + i) & mask);
            }
        }
        public uint GetSeed() => _seed;

        public uint GetRandUInt()
        {
            uint a, b, c, d;
            a = _state[_index];
            c = _state[(_index + 13) & 15];
            b = a ^ c ^ (a << 16) ^ (c << 15);
            c = _state[(_index + 9) & 15];
            c ^= (c >> 11);
            a = _state[_index] = b ^ c;
            d = (uint)(a ^ ((a << 5) & 0xDA442D24UL));
            _index = (_index + 15) & 15;
            a = _state[_index];
            _state[_index] = a ^ b ^ d ^ (a << 2) ^ (b << 18) ^ (c << 28);
            return _state[_index];
        }
        public uint GetRandUInt(uint max) => GetRandUInt() % (max + 1);

        public float GetRandFloat() => GetRandUInt(1000000) / (float)1000000;
        public float GetRandFloat(float min, float max) => GetRandFloat() * (max - min) + min;
    }
}
