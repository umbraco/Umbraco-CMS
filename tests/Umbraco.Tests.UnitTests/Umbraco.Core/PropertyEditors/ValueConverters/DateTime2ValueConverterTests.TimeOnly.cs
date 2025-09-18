using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

[TestFixture]
public partial class DateTime2ValueConverterTests
{
    [TestCase(Constants.PropertyEditors.Aliases.TimeOnly, true)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeUnspecified, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTime, false)]
    public void TimeOnly_IsConverter_For(string propertyEditorAlias, bool expected)
    {
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == propertyEditorAlias);
        var converter = new TimeOnlyValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict));

        var result = converter.IsConverter(propertyType);

        Assert.AreEqual(expected, result);
    }

    [TestCase(DateTime2Configuration.TimeZoneMode.None)]
    [TestCase(DateTime2Configuration.TimeZoneMode.All)]
    [TestCase(DateTime2Configuration.TimeZoneMode.Custom)]
    [TestCase(DateTime2Configuration.TimeZoneMode.Local)]
    public void TimeOnly_GetPropertyValueType_ReturnsExpectedType(DateTime2Configuration.TimeZoneMode timeZoneMode)
    {
        var converter = new TimeOnlyValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict));
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTime2Configuration
                {
                    TimeZones = new DateTime2Configuration.TimeZonesConfiguration { Mode = timeZoneMode },
                }));
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var result = converter.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(TimeOnly?), result);
    }

    [TestCaseSource(nameof(_convertToIntermediateCases))]
    public void TimeOnly_Can_Convert_To_Intermediate_Value(string? input, object? expected)
    {
        var result = new TimeOnlyValueConverter(_jsonSerializer).ConvertSourceToIntermediate(null!, null!, input, false);
        if (expected is null)
        {
            Assert.IsNull(result);
            return;
        }

        Assert.IsNotNull(result);
        Assert.IsInstanceOf<DateTime2ValueConverterBase.DateTime2Dto>(result);
        var dateTime = (DateTime2ValueConverterBase.DateTime2Dto)result;
        Assert.IsInstanceOf<DateTime2ValueConverterBase.DateTime2Dto>(dateTime);
        Assert.AreEqual(((DateTime2ValueConverterBase.DateTime2Dto)expected).Date, dateTime.Date);
        Assert.AreEqual(((DateTime2ValueConverterBase.DateTime2Dto)expected).TimeZone, dateTime.TimeZone);
    }

    private static object[] _timeOnlyConvertToObjectCases =
    [
        new object[] { null, DateTime2Configuration.TimeZoneMode.All, null },
        new object[] { _convertToObjectInputDate, DateTime2Configuration.TimeZoneMode.None, TimeOnly.Parse("17:30") },
    ];

    [TestCaseSource(nameof(_timeOnlyConvertToObjectCases))]
    public void TimeOnly_Can_Convert_To_Object(
        object? input,
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
                    TimeZones = new DateTime2Configuration.TimeZonesConfiguration { Mode = timeZoneMode },
                }));

        var propertyType = new Mock<IPublishedPropertyType>(MockBehavior.Strict);
        propertyType.SetupGet(x => x.DataType)
            .Returns(dataType);

        var result = new TimeOnlyValueConverter(_jsonSerializer)
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
