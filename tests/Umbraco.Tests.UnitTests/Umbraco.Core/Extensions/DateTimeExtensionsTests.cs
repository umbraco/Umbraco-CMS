// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Contains unit tests for the <see cref="DateTimeExtensions"/> extension methods.
/// </summary>
[TestFixture]
public class DateTimeExtensionsTests
{
    /// <summary>
    /// Tests that the ToIsoString extension method returns the correct ISO formatted string.
    /// </summary>
    [Test]
    public void ToIsoString_ReturnsCorrectFormat()
    {
        var date = new DateTime(2025, 6, 9, 14, 30, 45, DateTimeKind.Utc);
        var result = date.ToIsoString();
        Assert.AreEqual("2025-06-09 14:30:45", result);
    }

    /// <summary>
    /// Tests that the TruncateTo extension method correctly truncates a DateTime to the specified precision.
    /// </summary>
    /// <param name="year">The year component of the original date.</param>
    /// <param name="month">The month component of the original date.</param>
    /// <param name="day">The day component of the original date.</param>
    /// <param name="hour">The hour component of the original date.</param>
    /// <param name="minute">The minute component of the original date.</param>
    /// <param name="second">The second component of the original date.</param>
    /// <param name="millisecond">The millisecond component of the original date.</param>
    /// <param name="truncateTo">The level to truncate the date to.</param>
    /// <param name="expectedYear">The expected year component after truncation.</param>
    /// <param name="expectedMonth">The expected month component after truncation.</param>
    /// <param name="expectedDay">The expected day component after truncation.</param>
    /// <param name="expectedHour">The expected hour component after truncation.</param>
    /// <param name="expectedMinute">The expected minute component after truncation.</param>
    /// <param name="expectedSecond">The expected second component after truncation.</param>
    /// <param name="expectedMillisecond">The expected millisecond component after truncation.</param>
    [TestCase(2023, 5, 15, 14, 30, 45, 123, DateTimeExtensions.DateTruncate.Year, 2023, 1, 1, 0, 0, 0, 0)]
    [TestCase(2023, 5, 15, 14, 30, 45, 123, DateTimeExtensions.DateTruncate.Month, 2023, 5, 1, 0, 0, 0, 0)]
    [TestCase(2023, 5, 15, 14, 30, 45, 123, DateTimeExtensions.DateTruncate.Day, 2023, 5, 15, 0, 0, 0, 0)]
    [TestCase(2023, 5, 15, 14, 30, 45, 123, DateTimeExtensions.DateTruncate.Hour, 2023, 5, 15, 14, 0, 0, 0)]
    [TestCase(2023, 5, 15, 14, 30, 45, 123, DateTimeExtensions.DateTruncate.Minute, 2023, 5, 15, 14, 30, 0, 0)]
    [TestCase(2023, 5, 15, 14, 30, 45, 123, DateTimeExtensions.DateTruncate.Second, 2023, 5, 15, 14, 30, 45, 0)]
    [TestCase(2023, 5, 15, 14, 30, 45, 123, DateTimeExtensions.DateTruncate.Millisecond, 2023, 5, 15, 14, 30, 45, 123)]
    public void TruncateTo_TruncatesCorrectly(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second,
        int millisecond,
        DateTimeExtensions.DateTruncate truncateTo,
        int expectedYear,
        int expectedMonth,
        int expectedDay,
        int expectedHour,
        int expectedMinute,
        int expectedSecond,
        int expectedMillisecond)
    {
        var date = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
        var expected = new DateTime(expectedYear, expectedMonth, expectedDay, expectedHour, expectedMinute, expectedSecond, expectedMillisecond, DateTimeKind.Utc);

        var result = date.TruncateTo(truncateTo);

        Assert.AreEqual(expected, result);
    }
}
