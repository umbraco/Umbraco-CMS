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
                return new DateTime(dt.Year, 1, 1);
            if (truncateTo == DateTruncate.Month)
                return new DateTime(dt.Year, dt.Month, 1);
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

        /// <summary>
        /// Calculates the number of minutes from a date time, on a rolling daily basis (so if
        /// date time is before the time, calculate onto next day)
        /// </summary>
        /// <param name="fromDateTime">Date to start from</param>
        /// <param name="scheduledTime">Time to compare against (in Hmm form, e.g. 330, 2200)</param>
        /// <returns></returns>
        public static int PeriodicMinutesFrom(this DateTime fromDateTime, string scheduledTime)
        {
            // Ensure time provided is 4 digits long
            if (scheduledTime.Length == 3)
            {
                scheduledTime = "0" + scheduledTime;
            }

            var scheduledHour = int.Parse(scheduledTime.Substring(0, 2));
            var scheduledMinute = int.Parse(scheduledTime.Substring(2));

            DateTime scheduledDateTime;
            if (IsScheduledInRemainingDay(fromDateTime, scheduledHour, scheduledMinute))
            {
                scheduledDateTime = new DateTime(fromDateTime.Year, fromDateTime.Month, fromDateTime.Day, scheduledHour, scheduledMinute, 0);
            }
            else
            {
                var nextDay = fromDateTime.AddDays(1);
                scheduledDateTime = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, scheduledHour, scheduledMinute, 0);
            }

            return (int)(scheduledDateTime - fromDateTime).TotalMinutes;
        }

        private static bool IsScheduledInRemainingDay(DateTime fromDateTime, int scheduledHour, int scheduledMinute)
        {
            return scheduledHour > fromDateTime.Hour || (scheduledHour == fromDateTime.Hour && scheduledMinute >= fromDateTime.Minute);
        }
    }
}
