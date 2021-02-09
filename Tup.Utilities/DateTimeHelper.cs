using System;

namespace Tup.Utilities
{
    /// <summary>
    /// 时间操作 工具类
    /// </summary>
    public static class DateTimeHelper
    {
        #region 时间戳处理/timestamp

        /// <summary>
        /// Local 默认时间 1970-01-01
        /// </summary>
        private static readonly DateTime LocalDateTime19700101 = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

        #region ToLocalTime2

        /// <summary>
        /// 如果 <paramref name="unspecifiedDate"/> 为 <see cref="DateTimeKind.Unspecified"/> ,
        /// 使用 <see cref="DateTime.SpecifyKind(DateTime, DateTimeKind)"/> 将当前 System.DateTime 对象的值转换为本地时间。
        /// </summary>
        /// <param name="unspecifiedDate"></param>
        /// <returns></returns>
        /// <remarks>
        /// <see cref="DateTime.SpecifyKind(DateTime, DateTimeKind)"/>
        /// <see cref="DateTimeKind.Unspecified"/>
        /// </remarks>
        public static DateTime ToLocalTime2(this DateTime unspecifiedDate)
        {
            if (unspecifiedDate.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(unspecifiedDate, DateTimeKind.Local);

            return unspecifiedDate;
        }

        /// <summary>
        /// 如果 <paramref name="unspecifiedDate"/> 为 <see cref="DateTimeKind.Unspecified"/> ,
        /// 使用 <see cref="DateTime.SpecifyKind(DateTime, DateTimeKind)"/> 将当前 System.DateTime 对象的值转换为本地时间。
        /// </summary>
        /// <param name="unspecifiedDate"></param>
        /// <returns></returns>
        /// <remarks>
        /// <see cref="DateTime.SpecifyKind(DateTime, DateTimeKind)"/>
        /// <see cref="DateTimeKind.Unspecified"/>
        /// </remarks>
        public static DateTime? ToLocalTime2(this DateTime? unspecifiedDate)
        {
            if (unspecifiedDate.HasValue)
                return ToLocalTime2(unspecifiedDate.Value);

            return unspecifiedDate;
        }

        #endregion

        #region Javascript 毫秒时间戳

        /// <summary>
        /// 转换 时间 到 时间戳 (Milliseconds 毫秒)
        /// </summary>
        /// <param name="time">待转换时间, 默认 当前时间</param>
        /// <returns></returns>
        public static long GetTimestamp(DateTime? time = null)
        {
            return (long)((time ?? DateTime.Now) - LocalDateTime19700101).TotalMilliseconds;
        }

        /// <summary>
        /// 转换 时间戳 (Milliseconds 毫秒) 到 时间
        /// </summary>
        /// <param name="timestamp">待转换时间戳 (Milliseconds 毫秒)</param>
        /// <returns></returns>
        public static DateTime FromTimestamp(long timestamp)
        {
            return LocalDateTime19700101.AddMilliseconds(timestamp);
        }

        #endregion

        #region Unix-时间戳

        /// <summary>
        /// 转换 时间 到 Unix-时间戳 (Seconds 秒)
        /// </summary>
        /// <param name="time">待转换时间, 默认 当前时间</param>
        /// <returns></returns>
        public static long GetUnixTimestamp(DateTime? time = null)
        {
            return (long)((time ?? DateTime.Now) - LocalDateTime19700101).TotalSeconds;
        }

        /// <summary>
        /// 转换 Unix-时间戳 (Seconds 秒) 到 时间
        /// </summary>
        /// <param name="timestamp">待转换时间戳 (Seconds 秒)</param>
        /// <returns></returns>
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return LocalDateTime19700101.AddSeconds(timestamp);
        }

        #endregion

        #endregion

        #region 时间相关

        /// <summary>
        /// 获取时间的 月开始/本月第一天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime MonthBegin(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// 判断是否 同年同月
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool IsSameMonth(this DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month;
        }

        #endregion

        #region 整天/下一天

        /// <summary>
        /// 整天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <remarks>
        /// 明天 减一滴答
        /// </remarks>
        public static DateTime WholeDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// 明天零点
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime Tomorrow(this DateTime date)
        {
            return date.Date.AddDays(1);
        }

        #endregion
    }
}