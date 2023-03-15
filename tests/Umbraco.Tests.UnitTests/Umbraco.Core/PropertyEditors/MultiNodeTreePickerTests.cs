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
public class MultiNodeTreePickerTests
{
    [Test]
    public void Can_Handle_Invalid_Values_From_Editor()
    {
        // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
        var invalidValues = new List<object?>
        {
            123m,
            123,
            -123,
            123.45d,
            "123.45",
            "1.234,56",
            "1.2.3.4",
            "something",
            true,
            new object(),
            Guid.NewGuid(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
        };

        foreach (var value in invalidValues)
        {
            var fromEditor = FromEditor(value);
            Assert.IsNull(fromEditor, message: $"Failed for: {value}");
        }
    }

    [Test]
    public void Can_Handle_Invalid_Values_To_Editor()
    {
        // annoyingly we can't use decimals etc. in attributes, so we can't turn these into test cases :(
        var invalidValues = new List<object?>
        {
            123m,
            123,
            -123,
            123.45d,
            true,
            new object(),
            Guid.NewGuid(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
        };

        foreach (var value in invalidValues)
        {
            var toEditor = ToEditor(value);
            Assert.IsNull(toEditor, message: $"Failed for: {value}");
        }
    }

    [Test]
    public void Can_Parse_Single_Value_From_Editor()
    {
        var value = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString();
        var fromEditor = FromEditor(new[] { value }) as string;
        Assert.AreEqual(value, fromEditor);
    }

    [Test]
    public void Can_Parse_Multi_Value_From_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString()
        };

        var fromEditor = FromEditor(values) as string;
        Assert.AreEqual(string.Join(",", values), fromEditor);
    }

    [Test]
    public void Can_Parse_Different_Entity_Types_From_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Member, Guid.NewGuid()).ToString()
        };

        var fromEditor = FromEditor(values) as string;
        Assert.AreEqual(string.Join(",", values), fromEditor);
    }

    [Test]
    public void Can_Skip_Invalid_Values_From_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            "Invalid Value",
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString()
        };

        var fromEditor = FromEditor(values) as string;
        Assert.AreEqual(string.Join(",", values.First(), values.Last()), fromEditor);
    }

    [Test]
    public void Can_Parse_Single_Value_To_Editor()
    {
        var value = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString();
        var toEditor = ToEditor(value) as IEnumerable<string>;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(1, toEditor.Count());
        Assert.AreEqual(value, toEditor.First());
    }

    [Test]
    public void Can_Parse_Multi_Value_To_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString()
        };
        var toEditor = ToEditor(string.Join(",", values)) as IEnumerable<string>;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(3, toEditor.Count());
        Assert.AreEqual(values[0], toEditor.First());
        Assert.AreEqual(values[1], toEditor.Skip(1).First());
        Assert.AreEqual(values[2], toEditor.Last());
    }

    [Test]
    public void Can_Parse_Different_Entity_Types_To_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString(),
            new GuidUdi(Constants.UdiEntityType.Member, Guid.NewGuid()).ToString()
        };
        var toEditor = ToEditor(string.Join(",", values)) as IEnumerable<string>;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(3, toEditor.Count());
        Assert.AreEqual(values[0], toEditor.First());
        Assert.AreEqual(values[1], toEditor.Skip(1).First());
        Assert.AreEqual(values[2], toEditor.Last());
    }

    [Test]
    public void Can_Skip_Invalid_Values_To_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString(),
            "Invalid Value",
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()).ToString()
        };
        var toEditor = ToEditor(string.Join(",", values)) as IEnumerable<string>;
        Assert.IsNotNull(toEditor);
        Assert.AreEqual(2, toEditor.Count());
        Assert.AreEqual(values[0], toEditor.First());
        Assert.AreEqual(values[2], toEditor.Last());
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
        var result = ToEditor(null) as IEnumerable<string>;
        Assert.IsNull(result);
    }

    private static object? FromEditor(object? value, int max = 0)
        => CreateValueEditor().FromEditor(new ContentPropertyData(value, new MultipleTextStringConfiguration { Max = max }), null);

    private static object? ToEditor(object? value)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);

        return CreateValueEditor().ToEditor(property.Object);
    }

    private static MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor CreateValueEditor()
    {
        var valueEditor = new MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor(
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias", "name", "view"));
        return valueEditor;
    }
}
