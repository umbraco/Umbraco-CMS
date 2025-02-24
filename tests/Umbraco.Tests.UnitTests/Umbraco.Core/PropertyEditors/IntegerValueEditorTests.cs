using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class IntegerValueEditorTests
{
    // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
    private Dictionary<object?,object?> _valuesAndExpectedResults = new();

    [SetUp]
    public void SetUp() => _valuesAndExpectedResults = new Dictionary<object?, object?>
    {
        { 123m, 123 },
        { 123, 123 },
        { -123, -123 },
        { 123.45d, null },
        { "123.45", null },
        { "1234.56", null },
        { "123,45", null },
        { "1.234,56", null },
        { "123 45", null },
        { "something", null },
        { true, null },
        { new object(), null },
        { new List<string> { "some", "values" }, null },
        { Guid.NewGuid(), null },
        { new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()), null }
    };

    [Test]
    public void Can_Parse_Values_From_Editor()
    {
        foreach (var (value, expected) in _valuesAndExpectedResults)
        {
            var fromEditor = FromEditor(value);
            Assert.AreEqual(expected, fromEditor, message: $"Failed for: {value}");
        }
    }

    [Test]
    public void Can_Parse_Values_To_Editor()
    {
        foreach (var (value, expected) in _valuesAndExpectedResults)
        {
            var toEditor = ToEditor(value);
            Assert.AreEqual(expected, toEditor, message: $"Failed for: {value}");
        }
    }

    [Test]
    public void Null_From_Editor_Yields_Null()
    {
        var result = FromEditor(null);
        Assert.IsNull(result);
    }

    [Test]
    public void Null_To_Editor_Yields_Null()
    {
        var result = ToEditor(null);
        Assert.IsNull(result);
    }

    [TestCase("x", false)]
    [TestCase(10, true)]
    public void Validates_Is_Integer(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual(validationResult.ErrorMessage, $"The value {value} is not a valid integer");
        }
    }

    [TestCase(8, false)]
    [TestCase(10, true)]
    [TestCase(12, true)]
    public void Validates_Is_Greater_Than_Or_Equal_To_Configured_Min(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual(validationResult.ErrorMessage, $"The value {value} is less than the allowed minimum value of 10");
        }
    }

    [TestCase(18, true)]
    [TestCase(20, true)]
    [TestCase(22, false)]
    public void Validates_Is_Less_Than_Or_Equal_To_Configured_Max(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual(validationResult.ErrorMessage, $"The value {value} is greater than the allowed maximum value of 20");
        }
    }

    [TestCase(17, false)]
    [TestCase(18, true)]
    public void Validates_Matches_Configured_Step(object value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual(validationResult.ErrorMessage, $"The value {value} does not correspond with the configured step value of 2");
        }
    }

    private static object? FromEditor(object? value)
        => CreateValueEditor().FromEditor(new ContentPropertyData(value, null), null);

    private static object? ToEditor(object? value)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);

        return CreateValueEditor().ToEditor(property.Object);
    }

    private static IntegerPropertyEditor.IntegerPropertyValueEditor CreateValueEditor()
    {
        var valueEditor = new IntegerPropertyEditor.IntegerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"));
        valueEditor.ConfigurationObject = new Dictionary<string, object>
        {
            { "min", 10 },
            { "max", 20 },
            { "step", 2 }
        };
        return valueEditor;
    }
}
