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

[TestFixture]
public class TimeOnlyValueConverterTests
{
    private readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private static readonly DateTimeValueConverterBase.DateTimeDto _convertToObjectInputDate = new()
    {
        Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-1)),
        TimeZone = "Europe/Copenhagen",
    };

    [TestCase(Constants.PropertyEditors.Aliases.TimeOnly, true)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeUnspecified, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTime, false)]
    public void IsConverter_For(string propertyEditorAlias, bool expected)
    {
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == propertyEditorAlias);
        var converter = new TimeOnlyValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict), Mock.Of<ILogger<TimeOnlyValueConverter>>());

        var result = converter.IsConverter(propertyType);

        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetPropertyValueType_ReturnsExpectedType()
    {
        var converter = new TimeOnlyValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict), Mock.Of<ILogger<TimeOnlyValueConverter>>());
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

        Assert.AreEqual(typeof(TimeOnly?), result);
    }

    private static readonly object[] _convertToIntermediateCases =
    [
        new object[] { null, null },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":null}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = null } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":"Europe/Copenhagen"}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = "Europe/Copenhagen" } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000-05:00","timeZone":"Europe/Copenhagen"}""", new DateTimeValueConverterBase.DateTimeDto { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-5)), TimeZone = "Europe/Copenhagen" } },
    ];

    [TestCaseSource(nameof(_convertToIntermediateCases))]
    public void Can_Convert_To_Intermediate_Value(string? input, object? expected)
    {
        var result = new TimeOnlyValueConverter(_jsonSerializer, Mock.Of<ILogger<TimeOnlyValueConverter>>()).ConvertSourceToIntermediate(null!, null!, input, false);
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

    private static object[] _timeOnlyConvertToObjectCases =
    [
        new object[] { null, null },
        new object[] { _convertToObjectInputDate, TimeOnly.Parse("17:30") },
    ];

    [TestCaseSource(nameof(_timeOnlyConvertToObjectCases))]
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

        var result = new TimeOnlyValueConverter(_jsonSerializer, Mock.Of<ILogger<TimeOnlyValueConverter>>())
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
