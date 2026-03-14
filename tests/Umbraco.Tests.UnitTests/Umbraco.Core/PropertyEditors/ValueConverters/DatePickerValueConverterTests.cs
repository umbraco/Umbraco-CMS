using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Contains unit tests for the <see cref="DatePickerValueConverter"/> class, verifying its behavior and value conversion logic.
/// </summary>
[TestFixture]
public class DatePickerValueConverterTests
{
    private static object[] _parseDateTimeValueCases =
    [
        new object[] { null, DateTime.MinValue },
        new object[] { DateTime.MinValue, DateTime.MinValue },
        new object[] { new DateTime(2021, 01, 20, 9, 0, 36), new DateTime(2021, 01, 20, 9, 0, 36) },
        new object[] { "2021-01-20T09:00:36", new DateTime(2021, 01, 20, 9, 0, 36) },
        new object[] { "test", DateTime.MinValue },
    ];

    /// <summary>
    /// Verifies that <see cref="DatePickerValueConverter.ParseDateTimeValue(object?)"/> correctly parses a DateTime value from the specified input and returns the expected result with <see cref="DateTimeKind.Unspecified"/>.
    /// </summary>
    /// <param name="input">The input value to parse, which may be <c>null</c>.</param>
    /// <param name="expected">The expected <see cref="DateTime"/> result after parsing.</param>
    [TestCaseSource(nameof(_parseDateTimeValueCases))]
    public void Can_Parse_DateTime_Value(object? input, DateTime expected)
    {
        var result = DatePickerValueConverter.ParseDateTimeValue(input);
        Assert.AreEqual(expected, result);
        Assert.AreEqual(DateTimeKind.Unspecified, result.Kind);
    }
}
