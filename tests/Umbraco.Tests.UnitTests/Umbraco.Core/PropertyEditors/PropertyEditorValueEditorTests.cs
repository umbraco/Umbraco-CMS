// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="PropertyEditorValueEditor"/> class in Umbraco.
/// </summary>
[TestFixture]
public class PropertyEditorValueEditorTests
{
    /// <summary>
    /// Tests whether the value editor correctly converts a given string value to a JSON object for use in the editor interface.
    /// The test asserts that valid JSON objects and arrays are converted, while invalid JSON or plain strings are not.
    /// </summary>
    /// <param name="value">The input string value to attempt to convert to a JSON object.</param>
    /// <param name="isOk">True if the conversion is expected to succeed (i.e., the input is valid JSON), otherwise false.</param>
    [TestCase("{\"prop1\": \"val1\", \"prop2\": \"val2\"}", true)]
    [TestCase("{1,2,3,4}", false)]
    [TestCase("[1,2,3,4]", true)]
    [TestCase("hello world", false)]
    public void Value_Editor_Can_Convert_To_Json_Object_For_Editor(string value, bool isOk)
    {
        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), "test", ValueStorageType.Nvarchar));
        prop.SetValue(value);

        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.String);

        var result = valueEditor.ToEditor(prop);
        Assert.AreEqual(isOk, !(result is string));
    }

    /// <summary>
    /// Tests that the value editor can convert a string value to the specified CLR type.
    /// </summary>
    /// <param name="valueType">The type of the value to convert (e.g., STRING, INT, DATETIME).</param>
    /// <param name="val">The string value to convert.</param>
    /// <param name="expected">The expected result after conversion.</param>
    [TestCase("STRING", "hello", "hello")]
    [TestCase("TEXT", "hello", "hello")]
    [TestCase("INT", "123", 123)]
    [TestCase("INT", "", null)] // test empty string for int
    [TestCase("DATETIME", "", null)] // test empty string for date
    public void Value_Editor_Can_Convert_To_Clr_Type(string valueType, string val, object expected)
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(valueType);

        var result = valueEditor.TryConvertValueToCrlType(val);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expected, result.Result);
    }

    // The following decimal tests have not been added as [TestCase]s
    // as the decimal type cannot be used as an attribute parameter
    /// <summary>
    /// Tests that the value editor can successfully convert a string representation of a decimal to the decimal CLR type.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Convert_To_Decimal_Clr_Type()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var result = valueEditor.TryConvertValueToCrlType("12.34");
        Assert.IsTrue(result.Success);
        Assert.AreEqual(12.34M, result.Result);
    }

    /// <summary>
    /// Tests that the value editor can convert a decimal value string with a comma as the decimal separator to the decimal CLR type.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Convert_To_Decimal_Clr_Type_With_Other_Separator()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var result = valueEditor.TryConvertValueToCrlType("12,34");
        Assert.IsTrue(result.Success);
        Assert.AreEqual(12.34M, result.Result);
    }

    /// <summary>
    /// Tests that the value editor can convert an empty string to a nullable decimal CLR type.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Convert_To_Decimal_Clr_Type_With_Empty_String()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var result = valueEditor.TryConvertValueToCrlType(string.Empty);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Result);
    }

    /// <summary>
    /// Tests that the value editor can convert a string representation of a date to a DateTime CLR type.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Convert_To_Date_Clr_Type()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Date);

        var result = valueEditor.TryConvertValueToCrlType("2010-02-05");
        Assert.IsTrue(result.Success);
        Assert.AreEqual(new DateTime(2010, 2, 5), result.Result);
    }

    [TestCase(ValueTypes.String, "hello", "hello")]
    [TestCase(ValueTypes.Text, "hello", "hello")]
    [TestCase(ValueTypes.Integer, 123, "123")]
    [TestCase(ValueTypes.Integer, "", "")] // test empty string for int
    [TestCase(ValueTypes.DateTime, "", "")] // test empty string for date
    public void Value_Editor_Can_Serialize_Value(string valueType, object val, string expected)
    {
        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), "test", ValueStorageType.Nvarchar));
        prop.SetValue(val);

        var valueEditor = MockedValueEditors.CreateDataValueEditor(valueType);

        var result = valueEditor.ToEditor(prop);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that the value editor can correctly serialize a decimal value.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Serialize_Decimal_Value()
    {
        var value = 12.34M;
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), "test", ValueStorageType.Decimal));
        prop.SetValue(value);

        var result = valueEditor.ToEditor(prop);
        Assert.AreEqual("12.34", result);
    }

    /// <summary>
    /// Tests that the value editor can serialize a decimal value when the value is an empty string.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Serialize_Decimal_Value_With_Empty_String()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), "test", ValueStorageType.Decimal));
        prop.SetValue(string.Empty);

        var result = valueEditor.ToEditor(prop);
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that the value editor can correctly serialize a date value.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Serialize_Date_Value()
    {
        var now = DateTime.UtcNow;
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Date);

        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), "test", ValueStorageType.Date));
        prop.SetValue(now);

        var result = valueEditor.ToEditor(prop);
        Assert.AreEqual(now.ToIsoString(), result);
    }
}
