using System.Data;
using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
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
            Assert.That(fromEditor, Is.Null, message: $"Failed for: {value}");
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
            Assert.That(toEditor, Is.Null, message: $"Failed for: {value}");
        }
    }

    [Test]
    public void Can_Parse_Single_Value_From_Editor()
    {
        var value = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid());
        var editorValue = $"[{{\"type\" :\"{value.EntityType}\",\"unique\":\"{value.Guid}\"}}]";
        var fromEditor =
            FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory())) as string;
        Assert.That(fromEditor, Is.EqualTo(value.ToString()));
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

        var fromEditor = FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory())) as string;
        Assert.That(fromEditor, Is.EqualTo(string.Join(",", values.Select(v => v.ToString()))));
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

        var fromEditor = FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory())) as string;
        Assert.That(fromEditor, Is.EqualTo(string.Join(",", expectedValues.Select(v => v.ToString()))));
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
            FromEditor(JsonNode.Parse(editorValue), jsonSerializer: new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory())));
    }

    [Test]
    public void Can_Parse_Single_Value_To_Editor()
    {
        var value = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid());
        var toEditor = ToEditor(value.ToString()) as IEnumerable<MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.EditorEntityReference>;

        Assert.Multiple(() =>
        {
            Assert.That(toEditor, Is.Not.Null);
            Assert.That(toEditor.Count(), Is.EqualTo(1));
            Assert.That(toEditor.First().Type, Is.EqualTo(EditorEntityReferenceFromUdi(value).Type));
            Assert.That(toEditor.First().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(value).Unique));
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
            Assert.That(toEditor, Is.Not.Null);
            Assert.That(toEditor.Count(), Is.EqualTo(3));
            Assert.That(toEditor.First().Type, Is.EqualTo(EditorEntityReferenceFromUdi(values[0]).Type));
            Assert.That(toEditor.First().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(values[0]).Unique));
            Assert.That(toEditor.Skip(1).First().Type, Is.EqualTo(EditorEntityReferenceFromUdi(values[1]).Type));
            Assert.That(toEditor.Skip(1).First().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(values[1]).Unique));
            Assert.That(toEditor.Last().Type, Is.EqualTo(EditorEntityReferenceFromUdi(values[2]).Type));
            Assert.That(toEditor.Last().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(values[2]).Unique));
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
            Assert.That(toEditor, Is.Not.Null);
            Assert.That(toEditor.Count(), Is.EqualTo(3));
            Assert.That(toEditor.First().Type, Is.EqualTo(EditorEntityReferenceFromUdi(values[0]).Type));
            Assert.That(toEditor.First().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(values[0]).Unique));
            Assert.That(toEditor.Skip(1).First().Type, Is.EqualTo(EditorEntityReferenceFromUdi(values[1]).Type));
            Assert.That(toEditor.Skip(1).First().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(values[1]).Unique));
            Assert.That(toEditor.Last().Type, Is.EqualTo(EditorEntityReferenceFromUdi(values[2]).Type));
            Assert.That(toEditor.Last().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(values[2]).Unique));
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
            Assert.That(toEditor, Is.Not.Null);
            Assert.That(toEditor.Count(), Is.EqualTo(2));
            Assert.That(toEditor.First().Type, Is.EqualTo(EditorEntityReferenceFromUdi(firstGuid).Type));
            Assert.That(toEditor.First().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(firstGuid).Unique));
            Assert.That(toEditor.Last().Type, Is.EqualTo(EditorEntityReferenceFromUdi(secondGuid).Type));
            Assert.That(toEditor.Last().Unique, Is.EqualTo(EditorEntityReferenceFromUdi(secondGuid).Unique));
        });
    }

    [Test]
    public void Null_From_Editor_Yields_Null()
    {
        var result = FromEditor(null);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Null_To_Editor_Yields_Null()
    {
        var result = ToEditor(null) as IEnumerable<string>;
        Assert.That(result, Is.Null);
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
        var mockScope = new Mock<IScope>();
        var mockScopeProvider = new Mock<ICoreScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(mockScope.Object);

        var valueEditor = new MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            jsonSerializer ?? Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IEntityService>(),
            mockScopeProvider.Object,
            Mock.Of<IContentService>(),
            Mock.Of<IMediaService>(),
            Mock.Of<IMemberService>());
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
