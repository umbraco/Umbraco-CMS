using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Contains unit tests for the <see cref="DateTimeWithTimeZoneValueConverter"/> class, verifying its behavior and correctness.
/// </summary>
[TestFixture]
public class DateTimeWithTimeZoneValueConverterTests
{
    private readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private static readonly DateTimeValueConverterBase.DateTimeDto _convertToObjectInputDate = new()
    {
        Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-1)),
        TimeZone = "Europe/Copenhagen",
    };

    /// <summary>
    /// Tests whether the DateTimeWithTimeZoneValueConverter correctly identifies if it can convert a given property editor alias.
    /// </summary>
    /// <param name="propertyEditorAlias">The alias of the property editor to test.</param>
    /// <param name="expected">The expected result indicating if the converter should handle the given alias.</param>
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone, true)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeUnspecified, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.TimeOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTime, false)]
    public void IsConverter_For(string propertyEditorAlias, bool expected)
    {
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == propertyEditorAlias);
        var converter = new DateTimeWithTimeZoneValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict), Mock.Of<ILogger<DateTimeWithTimeZoneValueConverter>>());

        var result = converter.IsConverter(propertyType);

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeWithTimeZoneValueConverter.GetPropertyValueType"/> returns nullable <see cref="DateTimeOffset"/> as the property value type
    /// for the specified <paramref name="timeZoneMode"/> in the data type configuration.
    /// </summary>
    /// <param name="timeZoneMode">The <see cref="DateTimeConfiguration.TimeZoneMode"/> to test.</param>
    [TestCase(DateTimeConfiguration.TimeZoneMode.All)]
    [TestCase(DateTimeConfiguration.TimeZoneMode.Custom)]
    [TestCase(DateTimeConfiguration.TimeZoneMode.Local)]
    public void GetPropertyValueType_ReturnsExpectedType(DateTimeConfiguration.TimeZoneMode timeZoneMode)
    {
        var converter = new DateTimeWithTimeZoneValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict), Mock.Of<ILogger<DateTimeWithTimeZoneValueConverter>>());
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTimeConfiguration
                {
                    TimeZones = new DateTimeConfiguration.TimeZonesConfiguration { Mode = timeZoneMode },
                }));
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var result = converter.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(DateTimeOffset?), result);
    }

    private static readonly object[] _convertToIntermediateCases =
    [
        new object[] { null, null },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":null}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = null } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":"Europe/Copenhagen"}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = "Europe/Copenhagen" } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000-05:00","timeZone":"Europe/Copenhagen"}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-5)), TimeZone = "Europe/Copenhagen" } },
    ];

    /// <summary>
    /// Verifies that the <see cref="DateTimeWithTimeZoneValueConverter"/> correctly converts a string input to an intermediate <see cref="DateTimeValueConverterBase.DateTimeDto"/> value.
    /// </summary>
    /// <param name="input">The input string representing a date and time, which may be <c>null</c>.</param>
    /// <param name="expected">The expected <see cref="DateTimeValueConverterBase.DateTimeDto"/> result, or <c>null</c> if the conversion should yield <c>null</c>.</param>
    [TestCaseSource(nameof(_convertToIntermediateCases))]
    public void Can_Convert_To_Intermediate_Value(string? input, object? expected)
    {
        var result = new DateTimeWithTimeZoneValueConverter(_jsonSerializer, Mock.Of<ILogger<DateTimeWithTimeZoneValueConverter>>()).ConvertSourceToIntermediate(null!, null!, input, false);
        if (expected is null)
        {
            Assert.IsNull(result);
            return;
        }

        Assert.IsNotNull(result);
        Assert.IsInstanceOf<DateTimeValueConverterBase.DateTimeDto>(result);
        var dateTime = (DateTimeValueConverterBase.DateTimeDto)result;
        Assert.IsInstanceOf<DateTimeValueConverterBase.DateTimeDto>(dateTime);
        Assert.AreEqual(((DateTimeValueConverterBase.DateTimeDto)expected).Date, dateTime.Date);
        Assert.AreEqual(((DateTimeValueConverterBase.DateTimeDto)expected).TimeZone, dateTime.TimeZone);
    }

    private static object[] _dateTimeWithTimeZoneConvertToObjectCases =
    [
        new object[] { null, DateTimeConfiguration.TimeZoneMode.All, null },
        new object[] { _convertToObjectInputDate, DateTimeConfiguration.TimeZoneMode.All, _convertToObjectInputDate.Date },
        new object[] { _convertToObjectInputDate, DateTimeConfiguration.TimeZoneMode.Local, _convertToObjectInputDate.Date },
        new object[] { _convertToObjectInputDate, DateTimeConfiguration.TimeZoneMode.Custom, _convertToObjectInputDate.Date },
    ];

    /// <summary>
    /// Unit test that verifies conversion of an input value to an object using the specified time zone mode.
    /// </summary>
    /// <param name="input">The value to be converted.</param>
    /// <param name="timeZoneMode">The time zone mode applied during conversion.</param>
    /// <param name="expected">The expected result after conversion, used for assertion.</param>
    [TestCaseSource(nameof(_dateTimeWithTimeZoneConvertToObjectCases))]
    public void Can_Convert_To_Object(
        object? input,
        DateTimeConfiguration.TimeZoneMode timeZoneMode,
        object? expected)
    {
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTimeConfiguration
                {
                    TimeZones = new DateTimeConfiguration.TimeZonesConfiguration { Mode = timeZoneMode },
                }));

        var propertyType = new Mock<IPublishedPropertyType>(MockBehavior.Strict);
        propertyType.SetupGet(x => x.DataType)
            .Returns(dataType);

        var result = new DateTimeWithTimeZoneValueConverter(_jsonSerializer, Mock.Of<ILogger<DateTimeWithTimeZoneValueConverter>>())
            .ConvertIntermediateToObject(null!, propertyType.Object, PropertyCacheLevel.Unknown, input, false);
        if (expected is null)
        {
            Assert.IsNull(result);
            return;
        }

        Assert.IsNotNull(result);
        Assert.AreEqual(expected, result);
    }
}
