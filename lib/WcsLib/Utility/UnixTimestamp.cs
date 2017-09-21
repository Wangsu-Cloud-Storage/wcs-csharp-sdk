using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wangsu.WcsLib.Utility
{
    public sealed class UnixTimestamp
    {
        /// <summary>
        /// 日期时间转换为时间戳
        /// </summary>
        /// <param name="dt">日期时间</param>
        /// <returns>时间戳</returns>
        public static long ConvertToTimestamp(DateTime dt)
        {
#if WINDOWS_UWP
            DateTimeOffset offset = dt;
            return offset.ToUnixTimeSeconds();
#else
            TimeSpan ts = dt.ToUniversalTime().Subtract(dtBase);
            return ts.Ticks / TimeSpan.TicksPerSecond;
#endif
        }

        /// <summary>
        /// 从UNIX时间戳转换为DateTime
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>日期时间</returns>
        public static DateTime ConvertToDateTime(long timestamp)
        {
            long ticks = timestamp * TimeSpan.TicksPerSecond;
            return dtBase.AddTicks(ticks);
        }

        /// <summary>
        /// 从 UNIX 时间戳转换为DateTime
        /// </summary>
        /// <param name="timestamp">时间戳字符串</param>
        /// <returns>日期时间</returns>
        public static DateTime ConvertToDateTime(string timestamp)
        {
            return ConvertToDateTime(long.Parse(timestamp));
        }

        /// <summary>
        /// 从现在(调用此函数时刻)起若干秒以后那个时间点的时间戳
        /// </summary>
        /// <param name="secondsAfterNow">从现在起多少秒以后</param>
        /// <returns>Unix时间戳</returns>
        public static long GetUnixTimestamp(long secondsAfterNow)
        {
            DateTime dt = DateTime.Now.AddSeconds(secondsAfterNow);
            return ConvertToTimestamp(dt);
        }

        /// <summary>
        /// 基准时间(UTC)
        /// </summary>
        private static DateTime dtBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
