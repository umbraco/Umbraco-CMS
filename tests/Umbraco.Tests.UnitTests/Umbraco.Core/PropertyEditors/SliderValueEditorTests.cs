using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class SliderValueEditorTests
{
    public static object[] InvalidCaseData = new object[]
    {
        123m,
        123,
        -123,
        "something",
        true,
        new object(),
        new List<string> { "some", "values" },
        Guid.NewGuid(),
        new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
    };

    [TestCaseSource(nameof(InvalidCaseData))]
    public void Can_Handle_Invalid_Values_From_Editor(object value)
    {
        var fromEditor = FromEditor(value);
        Assert.IsNull(fromEditor);
    }

    [TestCase("1", 1)]
    [TestCase("0", 0)]
    [TestCase("-1", -1)]
    [TestCase("123456789", 123456789)]
    [TestCase("123.45", 123.45)]
    public void Can_Parse_Single_Value_To_Editor(string value, decimal expected)
    {
        var toEditor = ToEditor(value) as SliderPropertyEditor.SliderPropertyValueEditor.SliderRange;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(toEditor.From, expected);
        Assert.AreEqual(toEditor.To, expected);
    }

    [TestCase("1,1", 1, 1)]
    [TestCase("0,0", 0, 0)]
    [TestCase("-1,-1", -1, -1)]
    [TestCase("10,123456789", 10, 123456789)]
    [TestCase("1.234,56", 1.234, 56)]
    [TestCase("4,6.234", 4, 6.234)]
    [TestCase("10.45,15.3", 10.45, 15.3)]
    public void Can_Parse_Range_Value_To_Editor(string value, decimal expectedFrom, decimal expectedTo)
    {
        var toEditor = ToEditor(value) as SliderPropertyEditor.SliderPropertyValueEditor.SliderRange;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(toEditor.From, expectedFrom);
        Assert.AreEqual(toEditor.To, expectedTo);
    }

    [TestCase(0, 10, "0,10")]
    [TestCase(10, 10, "10")]
    [TestCase(0, 0, "0")]
    [TestCase(-10, -10, "-10")]
    [TestCase(10, 123456789, "10,123456789")]
    [TestCase(1.5, 1.5, "1.5")]
    [TestCase(0, 0.5, "0,0.5")]
    [TestCase(5, 5.4, "5,5.4")]
    [TestCase(0.5, 0.6, "0.5,0.6")]
    public void Can_Parse_Valid_Value_From_Editor(decimal from, decimal to, string expectedResult)
    {
        var value = JsonNode.Parse($"{{\"from\": {from}, \"to\": {to}}}");
        var fromEditor = FromEditor(value) as string;
        Assert.AreEqual(expectedResult, fromEditor);
    }

    [TestCaseSource(nameof(InvalidCaseData))]
    public void Can_Handle_Invalid_Values_To_Editor(object value)
    {
        var toEditor = ToEditor(value);
        Assert.IsNull(toEditor, message: $"Failed for: {value}");
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

    private static SliderPropertyEditor.SliderPropertyValueEditor CreateValueEditor()
    {
        var valueEditor = new SliderPropertyEditor.SliderPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"));
        return valueEditor;
    }
}
