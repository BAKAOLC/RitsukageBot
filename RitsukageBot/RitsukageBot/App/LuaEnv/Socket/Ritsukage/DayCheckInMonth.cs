using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Socket.Ritsukage
{
    public class DayCheckInMonth
    {
        /// <summary>
        /// 记录年份
        /// </summary>
        public int Year = DateTime.Now.Year;

        /// <summary>
        /// 记录月份
        /// </summary>
        public int Month = DateTime.Now.Month;

        /// <summary>
        /// 该月标记总数
        /// </summary>
        public int Count;

        /// <summary>
        /// 该月标记记录
        /// </summary>
        public bool[] Record;

        public DayCheckInMonth(int year, int month, int count = 0, bool[] record = null)
        {
            Year = year;
            Month = month;
            Count = count;
            if (record == null)
                Record = new bool[DateTime.DaysInMonth(year, month)];
            else
                Record = record;
        }
    }
}
