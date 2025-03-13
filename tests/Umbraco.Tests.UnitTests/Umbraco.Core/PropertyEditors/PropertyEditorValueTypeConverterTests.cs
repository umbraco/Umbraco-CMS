using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class PropertyEditorValueTypeConverterTests
{
    [TestCase("2023-09-26 13:14:15", true)]
    [TestCase("2023-09-26T13:14:15", true)]
    [TestCase("2023-09-26T00:00:00", true)]
    [TestCase("2023-09-26", true)]
    [TestCase("", false)]
    [TestCase("Hello, world!", false)]
    [TestCase(123456, false)]
    [TestCase(null, false)]
    public void CanConvertDateValueTypePropertyEditor(object? date, bool expectedSuccess)
    {
        var expectedResult = expectedSuccess ? DateTime.Parse((date as string)!) : DateTime.MinValue;
        var supportedValueTypes = new[] { ValueTypes.DateTime, ValueTypes.Date };
        foreach (var valueType in supportedValueTypes)
        {
            var converter = new DateTimeValueTypeConverter(ValueTypePropertyEditorCollection(valueType));
            var propertyType = PropertyType();

            Assert.IsTrue(converter.IsConverter(propertyType));

            var result = converter.ConvertSourceToIntermediate(null, propertyType, date, false);

            Assert.AreEqual(expectedResult, result);
        }
    }

    [TestCase("1", 1)]
    [TestCase("0", 0)]
    [TestCase(null, 0)]
    [TestCase("", 0)]
    [TestCase("Hello, world!", 0)]
    [TestCase("-1", -1)]
    [TestCase("1.65", 1.65)]
    [TestCase("-1.65", -1.65)]
    public void CanConvertDecimalValueTypePropertyEditor(object value, decimal expected)
    {
        var propertyEditors = ValueTypePropertyEditorCollection(ValueTypes.Decimal);
        var propertyType = PropertyType();

        var converter = new DecimalValueTypeConverter(propertyEditors);
        var inter = converter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), propertyType, value, false);

        Assert.IsTrue(converter.IsConverter(propertyType));

        var result = converter.ConvertIntermediateToObject(Mock.Of<IPublishedElement>(), propertyType, PropertyCacheLevel.Element, inter, false);
        Assert.IsTrue(result is decimal);
        Assert.AreEqual(expected, result);
    }

    [TestCase("100", 100)]
    [TestCase("0", 0)]
    [TestCase(null, 0)]
    [TestCase("", 0)]
    [TestCase("Hello, world!", 0)]
    [TestCase("-100", -100)]
    [TestCase("1.65", 2)]
    [TestCase("-1.65", -2)]
    public void CanConvertIntegerValueTypePropertyEditor(object value, int expected)
    {
        var propertyEditors = ValueTypePropertyEditorCollection(ValueTypes.Integer);
        var propertyType = PropertyType();

        var converter = new IntegerValueTypeConverter(propertyEditors);
        var inter = converter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), propertyType, value, false);

        Assert.IsTrue(converter.IsConverter(propertyType));

        var result = converter.ConvertIntermediateToObject(Mock.Of<IPublishedElement>(), propertyType, PropertyCacheLevel.Element, inter, false);
        Assert.IsTrue(result is int);
        Assert.AreEqual(expected, result);
    }

    [TestCase("100", 100)]
    [TestCase("0", 0)]
    [TestCase(null, 0)]
    [TestCase("", 0)]
    [TestCase("Hello, world!", 0)]
    [TestCase("-100", -100)]
    [TestCase("1.65", 2)]
    [TestCase("-1.65", -2)]
    public void CanConvertBigintValueTypePropertyEditor(object value, long expected)
    {
        var propertyEditors = ValueTypePropertyEditorCollection(ValueTypes.Bigint);
        var propertyType = PropertyType();

        var converter = new BigintValueTypeConverter(propertyEditors);
        var inter = converter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), propertyType, value, false);

        Assert.IsTrue(converter.IsConverter(propertyType));

        var result = converter.ConvertIntermediateToObject(Mock.Of<IPublishedElement>(), propertyType, PropertyCacheLevel.Element, inter, false);
        Assert.IsTrue(result is long);
        Assert.AreEqual(expected, result);
    }

    [TestCase("100", "100")]
    [TestCase("0", "0")]
    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("Hello, world!", "Hello, world!")]
    [TestCase(-100, null)]
    [TestCase(1.65, null)]
    public void CanConvertTextAndStringValueTypePropertyEditor(object? value, string? expected)
    {
        var scenarios = new[] { ValueTypes.Text, ValueTypes.String };
        foreach (var scenario in scenarios)
        {
            var propertyEditors = ValueTypePropertyEditorCollection(scenario);
            var propertyType = PropertyType();

            var converter = new TextStringValueTypeConverter(propertyEditors);
            var inter = converter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), propertyType, value, false);

            Assert.IsTrue(converter.IsConverter(propertyType));

            var result = converter.ConvertIntermediateToObject(Mock.Of<IPublishedElement>(), propertyType, PropertyCacheLevel.Element, inter, false);
            Assert.AreEqual(expected, result);
        }
    }

    [TestCase("2023-01-01T03:04:00Z","03:04:00")]
    [TestCase("2023-01-01T13:14:00Z", "13:14:00")]
    [TestCase("2023-01-01T13:14:15Z", "13:14:15")]
    [TestCase("2023-01-01T13:14:15.678Z", "13:14:15.678")]
    [TestCase("", null)]
    [TestCase("Hello, world!", null)]
    [TestCase(123456, null)]
    [TestCase(null, null)]
    public void CanConvertTimeValueTypePropertyEditor(object? value, object? expectedTime)
    {
        var sourceValue = expectedTime is not null ? DateTime.Parse((value as string)!) : value;
        TimeSpan? expectedResult = expectedTime is not null ? TimeSpan.Parse((expectedTime as string)!) : null;
        var propertyEditors = ValueTypePropertyEditorCollection(ValueTypes.Time);
        var converter = new TimeValueTypeConverter(propertyEditors);
        var propertyType = PropertyType();

        Assert.IsTrue(converter.IsConverter(propertyType));

        var result = converter.ConvertSourceToIntermediate(null, propertyType, sourceValue, false);
        Assert.AreEqual(expectedResult, result);
    }

    [TestCase("<root>test</root>", true)]
    [TestCase("<root><child>child 1</child><child>child 2</child></root>", true)]
    [TestCase("<root><child>malformed XML<child><root>", false)]
    [TestCase("", false)]
    [TestCase("Hello, world!", false)]
    [TestCase(123456, false)]
    [TestCase(null, false)]
    public void CanConvertXmlValueTypePropertyEditor(object? value, bool expectsSuccess)
    {
        var propertyEditors = ValueTypePropertyEditorCollection(ValueTypes.Xml);
        var converter = new XmlValueTypeConverter(propertyEditors);
        var propertyType = PropertyType();

        Assert.IsTrue(converter.IsConverter(propertyType));

        var result = converter.ConvertSourceToIntermediate(null, propertyType, value, false) as XDocument;
        if (expectsSuccess)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(value, result.ToString(SaveOptions.DisableFormatting));
        }
        else
        {
            Assert.IsNull(result);
        }
    }

    [TestCase("{\"message\":\"Hello, JSON\"}", true)]
    [TestCase("{\"nested\":{\"message\":\"Hello, Nested\"}}", true)]
    [TestCase("{\"nested\":{\"invalid JSON", false)]
    [TestCase("", false)]
    [TestCase("Hello, world!", false)]
    [TestCase(123456, false)]
    [TestCase(null, false)]
    public void CanConvertJsonValueTypePropertyEditor(object? source, bool expectsSuccess)
    {
        var propertyEditors = ValueTypePropertyEditorCollection(ValueTypes.Json);
        var converter = new JsonValueConverter(propertyEditors, Mock.Of<ILogger<JsonValueConverter>>());
        var propertyType = PropertyType();

        Assert.IsTrue(converter.IsConverter(propertyType));

        var result = converter.ConvertSourceToIntermediate(null, propertyType, source, false) as JToken;
        if (expectsSuccess)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(source, result.ToString(Formatting.None));
        }
        else
        {
            Assert.IsNull(result);
        }
    }

    private static PropertyEditorCollection ValueTypePropertyEditorCollection(string valueType)
    {
        var valueEditor = Mock.Of<IDataValueEditor>(x => x.ValueType == valueType);
        var dataEditor = Mock.Of<IDataEditor>(x => x.GetValueEditor() == valueEditor && x.Alias == "My.Custom.Alias" && x.Type == EditorType.PropertyValue);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => new[] { dataEditor }));
        return propertyEditors;
    }

    private static IPublishedPropertyType PropertyType() => Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == "My.Custom.Alias");
}
