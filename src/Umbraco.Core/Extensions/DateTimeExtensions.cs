// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;

namespace Umbraco.Extensions;

public static class DateTimeExtensions
{
    public enum DateTruncate
    {
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second,
    }

    /// <summary>
    ///     Returns the DateTime as an ISO formatted string that is globally expectable
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string ToIsoString(this DateTime dt) =>
        dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public static DateTime TruncateTo(this DateTime dt, DateTruncate truncateTo)
    {
        if (truncateTo == DateTruncate.Year)
        {
            return new DateTime(dt.Year, 1, 1);
        }

        if (truncateTo == DateTruncate.Month)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        if (truncateTo == DateTruncate.Day)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }

        if (truncateTo == DateTruncate.Hour)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
        }

        if (truncateTo == DateTruncate.Minute)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
        }

        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
    }
}
