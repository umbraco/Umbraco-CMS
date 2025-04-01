// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;

namespace Umbraco.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Defines the levels to truncate a date to.
    /// </summary>
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

    /// <summary>
    /// Truncates the date to the specified level, i.e. if you pass in DateTruncate.Hour it will truncate the date to the hour.
    /// </summary>
    /// <param name="dt">The date.</param>
    /// <param name="truncateTo">The level to truncate the date to.</param>
    /// <returns>The truncated date.</returns>
    public static DateTime TruncateTo(this DateTime dt, DateTruncate truncateTo)
    {
        if (truncateTo == DateTruncate.Year)
        {
            return new DateTime(dt.Year, 1, 1, 0, 0, 0, dt.Kind);
        }

        if (truncateTo == DateTruncate.Month)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
        }

        if (truncateTo == DateTruncate.Day)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Kind);
        }

        if (truncateTo == DateTruncate.Hour)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind);
        }

        if (truncateTo == DateTruncate.Minute)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
        }

        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind);
    }
}
