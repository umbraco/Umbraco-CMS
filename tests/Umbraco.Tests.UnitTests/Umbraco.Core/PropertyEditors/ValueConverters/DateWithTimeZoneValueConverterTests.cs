using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

[TestFixture]
public class DateWithTimeZoneValueConverterTests
{
    private readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private static object[] _convertToIntermediateCases =
    [
        new object[] { null, null },
        new object[] { "{\"date\":\"2025-08-20T16:30:00.0000000Z\",\"timeZone\":null}", new DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = null } },
        new object[] { "{\"date\":\"2025-08-20T16:30:00.0000000Z\",\"timeZone\":\"Europe/Copenhagen\"}", new DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.Zero), TimeZone = "Europe/Copenhagen" } },
        new object[] { "{\"date\":\"2025-08-20T16:30:00.0000000-05:00\",\"timeZone\":\"Europe/Copenhagen\"}", new DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone { Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-5)), TimeZone = "Europe/Copenhagen" } },
    ];

    [TestCaseSource(nameof(_convertToIntermediateCases))]
    public void Can_Convert_To_Intermediate_Value(string? input, DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone? expected)
    {
        var result = new DateTimeWithTimeZoneValueConverter(_jsonSerializer).ConvertSourceToIntermediate(null!, null!, input, false);
        if (expected is null)
        {
            Assert.IsNull(result);
            return;
        }

        Assert.IsNotNull(result);
        Assert.IsInstanceOf<DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone>(result);
        var dateWithTimeZone = (DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone)result;
        Assert.AreEqual(expected.Date, dateWithTimeZone.Date);
        Assert.AreEqual(expected.TimeZone, dateWithTimeZone.TimeZone);
    }

    private static DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone _convertToObjectInputDate = new()
    {
        Date = new DateTimeOffset(2025, 08, 20, 16, 30, 0, TimeSpan.FromHours(-1)),
        TimeZone = "Europe/Copenhagen",
    };

    private static object[] _convertToObjectCases =
    [
        new object[] { null, DateTimeWithTimeZoneFormat.DateTime, DateTimeWithTimeZoneMode.All, null },
        new object[] { _convertToObjectInputDate, DateTimeWithTimeZoneFormat.DateTime, DateTimeWithTimeZoneMode.All, DateTimeOffset.Parse("2025-08-20T16:30:00-01:00") },
        new object[] { _convertToObjectInputDate, DateTimeWithTimeZoneFormat.DateTime, DateTimeWithTimeZoneMode.None, DateTime.Parse("2025-08-20T17:30:00") },
        new object[] { _convertToObjectInputDate, DateTimeWithTimeZoneFormat.TimeOnly, DateTimeWithTimeZoneMode.None, TimeOnly.Parse("17:30:00") },
        new object[] { _convertToObjectInputDate, DateTimeWithTimeZoneFormat.DateOnly, DateTimeWithTimeZoneMode.None, DateOnly.Parse("2025-08-20") },
    ];

    [TestCaseSource(nameof(_convertToObjectCases))]
    public void Can_Convert_To_Object(
        DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone? input,
        DateTimeWithTimeZoneFormat format,
        DateTimeWithTimeZoneMode timeZoneMode,
        object? expected)
    {
        var dataType = new PublishedDataType(0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTimeWithTimeZoneConfiguration
                {
                    Format = format,
                    TimeZones = new DateTimeWithTimeZoneTimeZones { Mode = timeZoneMode },
                }));

        var propertyType = new Mock<IPublishedPropertyType>(MockBehavior.Strict);
        propertyType.SetupGet(x => x.DataType)
            .Returns(dataType);

        var result = new DateTimeWithTimeZoneValueConverter(_jsonSerializer)
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
