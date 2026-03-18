// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="PropertyEditorValueConverter"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class PropertyEditorValueConverterTests
{
    // see notes in the converter
    // only ONE date format is expected here

    // [TestCase("2012-11-10", true)]
    // [TestCase("2012/11/10", true)]
    // [TestCase("10/11/2012", true)]
    // [TestCase("11/10/2012", false)]
    // [TestCase("Sat 10, Nov 2012", true)]
    // [TestCase("Saturday 10, Nov 2012", true)]
    // [TestCase("Sat 10, November 2012", true)]
    // [TestCase("Saturday 10, November 2012", true)]
    // [TestCase("2012-11-10 13:14:15", true)]
    [TestCase("2012-11-10 13:14:15", true)]
    [TestCase("2012-11-10T13:14:15", true)]
    [TestCase("", false)]
    public void CanConvertDatePickerPropertyEditor(string date, bool expected)
    {
        var converter = new DatePickerValueConverter();
        var dateTime = new DateTime(2012, 11, 10, 13, 14, 15);
        var result = converter.ConvertSourceToIntermediate(null, null, date, false); // does not use type for conversion

        if (expected)
        {
            Assert.AreEqual(dateTime.Date, ((DateTime)result).Date);
        }
        else
        {
            Assert.AreNotEqual(dateTime.Date, ((DateTime)result).Date);
        }
    }

    [TestCase("TRUE", true)]
    [TestCase("True", true)]
    [TestCase("true", true)]
    [TestCase("1", true)]
    [TestCase(1, true)]
    [TestCase(true, true)]
    [TestCase("FALSE", false)]
    [TestCase("False", false)]
    [TestCase("false", false)]
    [TestCase("0", false)]
    [TestCase(0, false)]
    [TestCase(false, false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase("blah", false)]
    public void CanConvertYesNoPropertyEditor(object value, bool expected)
    {
        var converter = new YesNoValueConverter();
        var result =
            converter.ConvertSourceToIntermediate(null, null, value, false); // does not use type for conversion

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the CheckboxListValueConverter correctly converts the given JSON string value
    /// to the expected list of strings.
    /// </summary>
    /// <param name="value">The JSON string representing the selected checkbox values.</param>
    /// <param name="expected">The expected list of strings after conversion.</param>
    [TestCase("[\"apples\"]", new[] { "apples" })]
    [TestCase("[\"apples\",\"oranges\"]", new[] { "apples", "oranges" })]
    [TestCase("[\"apples\",\"oranges\",\"pears\"]", new[] { "apples", "oranges", "pears" })]
    [TestCase("", new string[] { })]
    [TestCase(null, new string[] { })]
    public void CanConvertCheckboxListPropertyEditor(object value, IEnumerable<string> expected)
    {
        var converter = new CheckboxListValueConverter(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));
        var result = converter.ConvertIntermediateToObject(null, null, PropertyCacheLevel.Unknown, value, false);

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that the FlexibleDropdownPropertyValueConverter correctly converts values from a dropdown list property editor configured for multiple selections.
    /// </summary>
    /// <param name="value">The input value to convert, typically a JSON array string representing the selected items, or null/empty for no selection.</param>
    /// <param name="expected">The expected collection of string values after conversion.</param>
    [TestCase("[\"apples\"]", new[] { "apples" })]
    [TestCase("[\"apples\",\"oranges\"]", new[] { "apples", "oranges" })]
    [TestCase("[\"apples\",\"oranges\",\"pears\"]", new[] { "apples", "oranges", "pears" })]
    [TestCase("", new string[] { })]
    [TestCase(null, new string[] { })]
    public void CanConvertDropdownListMultiplePropertyEditor(object value, IEnumerable<string> expected)
    {
        var mockPublishedContentTypeFactory = new Mock<IPublishedContentTypeFactory>();
        mockPublishedContentTypeFactory.Setup(x => x.GetDataType(123))
            .Returns(new PublishedDataType(123, "test", "test", new Lazy<object>(() => new DropDownFlexibleConfiguration { Multiple = true })));

        var publishedPropType = new PublishedPropertyType(
            new PublishedContentType(
                Guid.NewGuid(),
                1234,
                "test",
                PublishedItemType.Content,
                Enumerable.Empty<string>(),
                Enumerable.Empty<PublishedPropertyType>(),
                ContentVariation.Nothing),
            new PropertyType(Mock.Of<IShortStringHelper>(), "test", ValueStorageType.Nvarchar) { DataTypeId = 123 },
            new PropertyValueConverterCollection(() => Enumerable.Empty<IPropertyValueConverter>()),
            Mock.Of<IPublishedModelFactory>(),
            mockPublishedContentTypeFactory.Object);

        var converter = new FlexibleDropdownPropertyValueConverter(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));
        var inter = converter.ConvertSourceToIntermediate(null, publishedPropType, value, false);
        var result =
            converter.ConvertIntermediateToObject(null, publishedPropType, PropertyCacheLevel.Unknown, inter, false);

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that <see cref="DecimalValueConverter"/> correctly converts various string and null inputs to their expected decimal values.
    /// This test ensures that string representations of numbers, as well as null values, are handled as expected by the converter.
    /// </summary>
    /// <param name="value">The input value to convert, which may be a string or null.</param>
    /// <param name="expected">The expected decimal result after conversion.</param>
    [TestCase("1", 1)]
    [TestCase("0", 0)]
    [TestCase(null, 0)]
    [TestCase("-1", -1)]
    [TestCase("1.65", 1.65)]
    [TestCase("-1.65", -1.65)]
    public void CanConvertDecimalAliasPropertyEditor(object value, decimal expected)
    {
        var converter = new DecimalValueConverter();
        var inter = converter.ConvertSourceToIntermediate(null, null, value, false);
        var result = converter.ConvertIntermediateToObject(null, null, PropertyCacheLevel.Unknown, inter, false);

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the IntegerValueConverter correctly converts various input values to the expected integer output.
    /// </summary>
    /// <param name="value">The input value to convert.</param>
    /// <param name="expected">The expected integer result after conversion.</param>
    [TestCase("100", 100)]
    [TestCase("0", 0)]
    [TestCase(null, 0)]
    [TestCase("-100", -100)]
    [TestCase("1.65", 2)]
    [TestCase("-1.65", -2)]
    [TestCase("something something", 0)]
    public void CanConvertIntegerAliasPropertyEditor(object value, int expected)
    {
        var converter = new IntegerValueConverter();
        var inter = converter.ConvertSourceToIntermediate(null, null, value, false);
        var result = converter.ConvertIntermediateToObject(null, null, PropertyCacheLevel.Unknown, inter, false);

        Assert.AreEqual(expected, result);
    }
}
