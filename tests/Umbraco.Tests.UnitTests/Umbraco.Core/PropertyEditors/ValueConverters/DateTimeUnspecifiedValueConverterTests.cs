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
/// Contains unit tests for the <see cref="DateTimeUnspecifiedValueConverter"/> class, verifying its behavior and correctness.
/// </summary>
[TestFixture]
public class DateTimeUnspecifiedValueConverterTests
{
    private readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private static readonly DateTimeValueConverterBase.DateTimeDto _convertToObjectInputDate = new()
    {
        Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-1)),
        TimeZone = "Europe/Copenhagen",
    };

    /// <summary>
    /// Tests whether the <see cref="DateTimeUnspecifiedValueConverter"/> correctly identifies if it is the converter for the given property editor alias.
    /// </summary>
    /// <param name="propertyEditorAlias">The alias of the property editor to test.</param>
    /// <param name="expected">The expected result indicating if the converter should be used for the given alias.</param>
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeUnspecified, true)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.TimeOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTime, false)]
    public void IsConverter_For(string propertyEditorAlias, bool expected)
    {
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == propertyEditorAlias);
        var converter = new DateTimeUnspecifiedValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict), Mock.Of<ILogger<DateTimeUnspecifiedValueConverter>>());

        var result = converter.IsConverter(propertyType);

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that GetPropertyValueType returns the expected type.
    /// </summary>
    [Test]
    public void GetPropertyValueType_ReturnsExpectedType()
    {
        var converter = new DateTimeUnspecifiedValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict), Mock.Of<ILogger<DateTimeUnspecifiedValueConverter>>());
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTimeConfiguration
                {
                    TimeZones = null,
                }));
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var result = converter.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(DateTime?), result);
    }

    private static readonly object[] _convertToIntermediateCases =
    [
        new object[] { null, null },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":null}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = null } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":"Europe/Copenhagen"}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = "Europe/Copenhagen" } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000-05:00","timeZone":"Europe/Copenhagen"}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-5)), TimeZone = "Europe/Copenhagen" } },
    ];

    /// <summary>
    /// Verifies that a string input is correctly converted to an intermediate <see cref="DateTimeValueConverterBase.DateTimeDto"/> value, or null if conversion fails.
    /// </summary>
    /// <param name="input">The input string to convert to a DateTime DTO; may be null.</param>
    /// <param name="expected">The expected <see cref="DateTimeValueConverterBase.DateTimeDto"/> result, or null if conversion is expected to fail.</param>
    [TestCaseSource(nameof(_convertToIntermediateCases))]
    public void Can_Convert_To_Intermediate_Value(string? input, object? expected)
    {
        var result = new DateTimeUnspecifiedValueConverter(_jsonSerializer, Mock.Of<ILogger<DateTimeUnspecifiedValueConverter>>()).ConvertSourceToIntermediate(null!, null!, input, false);
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

    private static object[] _dateTimeUnspecifiedConvertToObjectCases =
    [
        new object[] { null, null },
        new object[] { _convertToObjectInputDate, DateTime.Parse("2025-08-20T16:30:00") },
    ];

    /// <summary>
    /// Verifies that the <see cref="DateTimeUnspecifiedValueConverter"/> correctly converts various input values to their expected object representations.
    /// </summary>
    /// <param name="input">The value to be converted by the value converter.</param>
    /// <param name="expected">The expected result after conversion, used for assertion.</param>
    /// <remarks>
    /// This test uses a set of test cases to ensure that the value converter handles different input scenarios as expected.
    /// </remarks>
    [TestCaseSource(nameof(_dateTimeUnspecifiedConvertToObjectCases))]
    public void Can_Convert_To_Object(
        object? input,
        object? expected)
    {
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTimeConfiguration
                {
                    TimeZones = null,
                }));

        var propertyType = new Mock<IPublishedPropertyType>(MockBehavior.Strict);
        propertyType.SetupGet(x => x.DataType)
            .Returns(dataType);

        var result = new DateTimeUnspecifiedValueConverter(_jsonSerializer, Mock.Of<ILogger<DateTimeUnspecifiedValueConverter>>())
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
