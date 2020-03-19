using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class Rand
    {
        private readonly WELL512 rand = new WELL512();

        public uint Seed() => rand.GetSeed();

        public void Seed(uint seed) => rand.SetSeed(seed);

        public int Int(int a, int b)
            => (a < b) ? a + (int)rand.GetRandUInt((uint)(b - a))
            : (a == b) ? a : b + (int)rand.GetRandUInt((uint)(a - b));

        public float Float() => rand.GetRandFloat();
        public float Float(float a, float b) => rand.GetRandFloat(a, b);

        public int Sign() => (int)rand.GetRandUInt(1) * 2 - 1;
    }
}
