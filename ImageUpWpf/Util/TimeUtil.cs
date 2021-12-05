using System;
using System.Collections.Generic;
using System.Text;

namespace ImageUpWpf.Util
{
    public static class TimeUtil
    {
        /// <summary>
        /// 获取当前 unix 以 s 记的时间戳
        /// </summary>
        /// <returns></returns>
        public static long Timestamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        /// <summary>
        /// 把字符串时间转换为 unix 时间戳
        /// </summary>
        /// <param name="expirationDate"></param>
        /// <returns></returns>
        public static long Parse(string expirationDate)
        {
            return (long)DateTime.Parse(expirationDate).ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string TimeToDate(long? expiredAt)
        {
            if (null == expiredAt)
            {
                return "无";
            }
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)expiredAt);
            var dateTime = dateTimeOffset.UtcDateTime;
            return dateTime.ToString("yyyy-MM-dd");

        }
    }
}
