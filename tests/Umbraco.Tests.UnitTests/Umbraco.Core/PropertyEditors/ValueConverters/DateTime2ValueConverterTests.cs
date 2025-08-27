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
public class DateTime2ValueConverterTests
{
    private readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [TestCase(Constants.PropertyEditors.Aliases.DateTime2, true)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTime, false)]
    public void IsConverter_For(string propertyEditorAlias, bool expected)
    {
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == propertyEditorAlias);
        var converter = new DateTime2ValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict));

        var result = converter.IsConverter(propertyType);

        Assert.AreEqual(expected, result);
    }

    [TestCase(DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.None, typeof(DateOnly?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.All, typeof(DateOnly?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.Custom, typeof(DateOnly?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.Local, typeof(DateOnly?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.TimeOnly, DateTime2Configuration.TimeZoneMode.None, typeof(TimeOnly?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.None, typeof(DateTime?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.All, typeof(DateTimeOffset?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.Custom, typeof(DateTimeOffset?))]
    [TestCase(DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.Local, typeof(DateTimeOffset?))]
    public void GetPropertyValueType_ReturnsExpectedType(DateTime2Configuration.DateTimeFormat format, DateTime2Configuration.TimeZoneMode timeZoneMode, Type expectedType)
    {
        var converter = new DateTime2ValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict));
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTime2Configuration
                {
                    Format = format,
                    TimeZones = new DateTime2Configuration.TimeZonesConfiguration { Mode = timeZoneMode },
                }));
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var result = converter.GetPropertyValueType(propertyType);

        Assert.AreEqual(expectedType, result);
    }

    private static object[] _convertToIntermediateCases =
    [
        new object[] { null, null },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":null}""", new DateTime2ValueConverter.DateTime2 { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = null } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000Z","timeZone":"Europe/Copenhagen"}""", new DateTime2ValueConverter.DateTime2 { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = "Europe/Copenhagen" } },
        new object[] { """{"date":"2025-08-20T16:30:00.0000000-05:00","timeZone":"Europe/Copenhagen"}""", new DateTime2ValueConverter.DateTime2 { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-5)), TimeZone = "Europe/Copenhagen" } },
    ];

    [TestCaseSource(nameof(_convertToIntermediateCases))]
    public void Can_Convert_To_Intermediate_Value(string? input, DateTime2ValueConverter.DateTime2? expected)
    {
        var result = new DateTime2ValueConverter(_jsonSerializer).ConvertSourceToIntermediate(null!, null!, input, false);
        if (expected is null)
        {
            Assert.IsNull(result);
            return;
        }

        Assert.IsNotNull(result);
        Assert.IsInstanceOf<DateTime2ValueConverter.DateTime2>(result);
        var dateTime = (DateTime2ValueConverter.DateTime2)result;
        Assert.AreEqual(expected.Date, dateTime.Date);
        Assert.AreEqual(expected.TimeZone, dateTime.TimeZone);
    }

    private static readonly DateTime2ValueConverter.DateTime2 _convertToObjectInputDate = new()
    {
        Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-1)),
        TimeZone = "Europe/Copenhagen",
    };

    private static object[] _convertToObjectCases =
    [
        new object[] { null, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.All, null },
        new object[] { _convertToObjectInputDate, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.All, DateTimeOffset.Parse("2025-08-20T16:30:00-01:00") },
        new object[] { _convertToObjectInputDate, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.None, DateTime.Parse("2025-08-20T17:30:00") },
        new object[] { _convertToObjectInputDate, DateTime2Configuration.DateTimeFormat.TimeOnly, DateTime2Configuration.TimeZoneMode.None, TimeOnly.Parse("17:30:00") },
        new object[] { _convertToObjectInputDate, DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.None, DateOnly.Parse("2025-08-20") },
    ];

    [TestCaseSource(nameof(_convertToObjectCases))]
    public void Can_Convert_To_Object(
        DateTime2ValueConverter.DateTime2? input,
        DateTime2Configuration.DateTimeFormat format,
        DateTime2Configuration.TimeZoneMode timeZoneMode,
        object? expected)
    {
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTime2Configuration
                {
                    Format = format,
                    TimeZones = new DateTime2Configuration.TimeZonesConfiguration { Mode = timeZoneMode },
                }));

        var propertyType = new Mock<IPublishedPropertyType>(MockBehavior.Strict);
        propertyType.SetupGet(x => x.DataType)
            .Returns(dataType);

        var result = new DateTime2ValueConverter(_jsonSerializer)
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
