using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class NumericValueEditorTests
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
        return valueEditor;
    }
}
