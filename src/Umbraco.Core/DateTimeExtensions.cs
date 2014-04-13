using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
    public static class DateTimeExtensions
    {

        /// <summary>
        /// Returns the DateTime as an ISO formatted string that is globally expectable
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToIsoString(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static DateTime TruncateTo(this DateTime dt, DateTruncate truncateTo)
        {
            if (truncateTo == DateTruncate.Year)
                return new DateTime(dt.Year, 0, 0);
            if (truncateTo == DateTruncate.Month)
                return new DateTime(dt.Year, dt.Month, 0);
            if (truncateTo == DateTruncate.Day)
                return new DateTime(dt.Year, dt.Month, dt.Day);
            if (truncateTo == DateTruncate.Hour)
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            if (truncateTo == DateTruncate.Minute)
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        public enum DateTruncate
        {
            Year,
            Month,
            Day,
            Hour,
            Minute,
            Second
        }

    }
}
