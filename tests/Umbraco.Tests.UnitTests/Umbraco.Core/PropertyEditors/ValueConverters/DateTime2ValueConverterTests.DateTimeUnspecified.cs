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
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeUnspecified, true)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.TimeOnly, false)]
    [TestCase(Constants.PropertyEditors.Aliases.DateTime, false)]
    public void DateTimeUnspecified_IsConverter_For(string propertyEditorAlias, bool expected)
    {
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == propertyEditorAlias);
        var converter = new DateTimeUnspecifiedValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict));

        var result = converter.IsConverter(propertyType);

        Assert.AreEqual(expected, result);
    }

    [Test]
    public void DateTimeUnspecified_GetPropertyValueType_ReturnsExpectedType()
    {
        var converter = new DateTimeUnspecifiedValueConverter(Mock.Of<IJsonSerializer>(MockBehavior.Strict));
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTime2Configuration
                {
                    TimeZones = null,
                }));
        var propertyType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var result = converter.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(DateTime?), result);
    }

    [TestCaseSource(nameof(_convertToIntermediateCases))]
    public void DateTimeUnspecified_Can_Convert_To_Intermediate_Value(string? input, object? expected)
    {
        var result = new DateTimeUnspecifiedValueConverter(_jsonSerializer).ConvertSourceToIntermediate(null!, null!, input, false);
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

    private static object[] _dateTimeUnspecifiedConvertToObjectCases =
    [
        new object[] { null, null },
        new object[] { _convertToObjectInputDate, DateTime.Parse("2025-08-20T17:30:00") },
    ];

    [TestCaseSource(nameof(_dateTimeUnspecifiedConvertToObjectCases))]
    public void DateTimeUnspecified_Can_Convert_To_Object(
        object? input,
        object? expected)
    {
        var dataType = new PublishedDataType(
            0,
            "test",
            "test",
            new Lazy<object?>(() =>
                new DateTime2Configuration
                {
                    TimeZones = null,
                }));

        var propertyType = new Mock<IPublishedPropertyType>(MockBehavior.Strict);
        propertyType.SetupGet(x => x.DataType)
            .Returns(dataType);

        var result = new DateTimeUnspecifiedValueConverter(_jsonSerializer)
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
