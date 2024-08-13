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
public class TrueFalsePropertyValueEditorTests
{
    // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
    private Dictionary<object?, bool> _valuesAndExpectedResults = new();

    [SetUp]
    public void SetUp() => _valuesAndExpectedResults = new Dictionary<object?, bool>
    {
        { true, true },
        { 1, true },
        { "1", true },
        { "true", true },
        { "TRUE", true },
        { false, false },
        { 0, false },
        { "0", false },
        { "false", false },
        { "FALSE", false },
        { 123m, false },
        { 123, false },
        { -123, false },
        { 123.45d, false },
        { "123.45", false },
        { "1234.56", false },
        { "123,45", false },
        { "1.234,56", false },
        { "123 45", false },
        { "something", false },
        { new object(), false },
        { new List<string> { "some", "values" }, false },
        { Guid.NewGuid(), false },
        { new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()), false }
    };

    [Test]
    public void Can_Parse_Values_From_Editor()
    {
        foreach (var (value, expected) in _valuesAndExpectedResults)
        {
            // FromEditor returns 1 or 0, not true or false
            var actuallyExpected = expected ? 1 : 0;
            var fromEditor = FromEditor(value);
            Assert.AreEqual(actuallyExpected, fromEditor, message: $"Failed for: {value}");
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
    public void Null_From_Editor_Yields_False()
    {
        var result = FromEditor(null);
        Assert.AreEqual(0, result);
    }

    [Test]
    public void Null_To_Editor_Yields_False()
    {
        var result = ToEditor(null);
        Assert.AreEqual(false, result);
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

    private static TrueFalsePropertyEditor.TrueFalsePropertyValueEditor CreateValueEditor()
    {
        var valueEditor = new TrueFalsePropertyEditor.TrueFalsePropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"));
        return valueEditor;
    }
}
