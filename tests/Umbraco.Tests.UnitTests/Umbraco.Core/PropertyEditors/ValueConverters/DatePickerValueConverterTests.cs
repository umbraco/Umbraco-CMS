using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

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

    [TestCaseSource(nameof(_parseDateTimeValueCases))]
    public void Can_Parse_DateTime_Value(object? input, DateTime expected)
    {
        var result = DatePickerValueConverter.ParseDateTimeValue(input);
        Assert.AreEqual(expected, result);
        Assert.AreEqual(DateTimeKind.Unspecified, result.Kind);
    }
}
