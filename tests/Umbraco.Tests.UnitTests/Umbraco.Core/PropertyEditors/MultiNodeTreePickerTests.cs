using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X500;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

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
        var value = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid());
        var editorValue = $"[{{\"type\" :\"{value.EntityType}\",\"unique\":\"{value.Guid}\"}}]";
        var fromEditor =
            FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer()) as string;
        Assert.AreEqual(value.ToString(), fromEditor);
    }

    [Test]
    public void Can_Parse_Multi_Value_From_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
        };

        var editorValue =
            $"[{{\"type\" :\"{values[0].EntityType}\",\"unique\":\"{values[0].Guid}\"}},{{\"type\" :\"{values[1].EntityType}\",\"unique\":\"{values[1].Guid}\"}},{{\"type\" :\"{values[2].EntityType}\",\"unique\":\"{values[2].Guid}\"}}]";

        var fromEditor = FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer()) as string;
        Assert.AreEqual(string.Join(",", values.Select(v => v.ToString())), fromEditor);
    }

    [Test]
    public void Can_Parse_Different_Entity_Types_From_Editor()
    {
        var expectedValues = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Media, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Member, Guid.NewGuid())
        };

        var editorValue =
            $"[{{\"type\" :\"{expectedValues[0].EntityType}\",\"unique\":\"{expectedValues[0].Guid}\"}},{{\"type\" :\"{expectedValues[1].EntityType}\",\"unique\":\"{expectedValues[1].Guid}\"}},{{\"type\" :\"{expectedValues[2].EntityType}\",\"unique\":\"{expectedValues[2].Guid}\"}}]";

        var fromEditor = FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer()) as string;
        Assert.AreEqual(string.Join(",", expectedValues.Select(v => v.ToString())), fromEditor);
    }

    [Test]
    public void From_Editor_Throws_Error_On_Invalid_Json()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
        };

        var editorValue =
            $"[{{\"type\" :\"{values[0].EntityType}\",\"unique\":\"{values[0].Guid}\"}},{{\"invalidProperty\" :\"nonsenseValue\",\"otherWeirdProperty\":\"definitelyNotAGuid\"}},{{\"type\" :\"{values[1].EntityType}\",\"unique\":\"{values[1].Guid}\"}}]";

        Assert.Catch<System.Text.Json.JsonException>(() =>
            FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer()));
    }

    [Test]
    public void Can_Parse_Single_Value_To_Editor()
    {
        var value = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid());
        var toEditor = ToEditor(value.ToString()) as IEnumerable<MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.EditorEntityReference>;

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(toEditor);
            Assert.AreEqual(1, toEditor.Count());
            Assert.AreEqual(EditorEntityReferenceFromUdi(value).Type, toEditor.First().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(value).Unique, toEditor.First().Unique);
        });

    }

    [Test]
    public void Can_Parse_Multi_Value_To_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid())
        };
        var toEditor = ToEditor(string.Join(",", values.Select(v => v.ToString()))) as IEnumerable<MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.EditorEntityReference>;

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(toEditor);
            Assert.AreEqual(3, toEditor.Count());
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[0]).Type, toEditor.First().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[0]).Unique, toEditor.First().Unique);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[1]).Type, toEditor.Skip(1).First().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[1]).Unique, toEditor.Skip(1).First().Unique);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[2]).Type, toEditor.Last().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[2]).Unique, toEditor.Last().Unique);
        });
    }

    [Test]
    public void Can_Parse_Different_Entity_Types_To_Editor()
    {
        var values = new[]
        {
            new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Media, Guid.NewGuid()),
            new GuidUdi(Constants.UdiEntityType.Member, Guid.NewGuid())
        };
        var toEditor = ToEditor(string.Join(",", values.Select(v => v.ToString()))) as IEnumerable<MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.EditorEntityReference>;
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(toEditor);
            Assert.AreEqual(3, toEditor.Count());
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[0]).Type, toEditor.First().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[0]).Unique, toEditor.First().Unique);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[1]).Type, toEditor.Skip(1).First().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[1]).Unique, toEditor.Skip(1).First().Unique);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[2]).Type, toEditor.Last().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(values[2]).Unique, toEditor.Last().Unique);
        });

    }

    [Test]
    public void Can_Skip_Invalid_Values_To_Editor()
    {
        var firstGuid = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid());
        var secondGuid = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid());
        var values = new[]
        {
            firstGuid.ToString(),
            "Invalid Value",
            secondGuid.ToString(),
        };
        var toEditor = ToEditor(string.Join(",", values)) as IEnumerable<MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.EditorEntityReference>;
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(toEditor);
            Assert.AreEqual(2, toEditor.Count());
            Assert.AreEqual(EditorEntityReferenceFromUdi(firstGuid).Type, toEditor.First().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(firstGuid).Unique, toEditor.First().Unique);
            Assert.AreEqual(EditorEntityReferenceFromUdi(secondGuid).Type, toEditor.Last().Type);
            Assert.AreEqual(EditorEntityReferenceFromUdi(secondGuid).Unique, toEditor.Last().Unique);
        });
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

    private static object? FromEditor(object? value, int max = 0, IJsonSerializer? jsonSerializer = null)
        => CreateValueEditor(jsonSerializer)
            .FromEditor(new ContentPropertyData(value, new MultipleTextStringConfiguration { Max = max }), null);

    private static object? ToEditor(object? value, IJsonSerializer? jsonSerializer = null)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);

        return CreateValueEditor(jsonSerializer).ToEditor(property.Object);
    }

    private static MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor CreateValueEditor(
        IJsonSerializer? jsonSerializer = null)
    {
        var valueEditor = new MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            jsonSerializer ?? Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"));
        return valueEditor;
    }

    private static MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.EditorEntityReference
        EditorEntityReferenceFromUdi(GuidUdi udi) =>
        new()
        {
            Type = udi.EntityType,
            Unique = udi.Guid,
        };
}
