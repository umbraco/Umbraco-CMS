using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ValueSchemaProviderTests
{
    [Test]
    public void IntegerPropertyEditor_Returns_Integer_Schema()
    {
        // Arrange
        var editor = CreateIntegerPropertyEditor();

        // Act
        var schema = editor.GetValueSchema(null);

        // Assert
        Assert.That(schema, Is.Not.Null);
        Assert.That(schema!["$schema"]?.GetValue<string>(), Is.EqualTo("https://json-schema.org/draft/2020-12/schema"));

        var typeArray = schema["type"] as JsonArray;
        Assert.That(typeArray, Is.Not.Null);
        Assert.That(typeArray!.Select(t => t?.GetValue<string>()), Is.EquivalentTo(new[] { "integer", "null" }));
    }

    [Test]
    public void IntegerPropertyEditor_Returns_ValueType()
    {
        // Arrange
        var editor = CreateIntegerPropertyEditor();

        // Act
        var valueType = editor.GetValueType(null);

        // Assert
        Assert.That(valueType, Is.EqualTo(typeof(int?)));
    }

    [Test]
    public void IntegerPropertyEditor_Includes_MinMax_When_Configured()
    {
        // Arrange
        var editor = CreateIntegerPropertyEditor();
        var config = new Dictionary<string, object>
        {
            { "min", 10 },
            { "max", 100 },
        };

        // Act
        var schema = editor.GetValueSchema(config);

        // Assert
        Assert.That(schema, Is.Not.Null);
        Assert.That(schema!["minimum"]?.GetValue<int>(), Is.EqualTo(10));
        Assert.That(schema["maximum"]?.GetValue<int>(), Is.EqualTo(100));
    }

    [Test]
    public void IntegerPropertyEditor_Includes_Step_When_Greater_Than_One()
    {
        // Arrange
        var editor = CreateIntegerPropertyEditor();
        var config = new Dictionary<string, object>
        {
            { "min", 0 },
            { "step", 5 },
        };

        // Act
        var schema = editor.GetValueSchema(config);

        // Assert
        Assert.That(schema, Is.Not.Null);
        Assert.That(schema!["multipleOf"]?.GetValue<int>(), Is.EqualTo(5));
    }

    [Test]
    public void IntegerPropertyEditor_Omits_Step_When_One()
    {
        // Arrange
        var editor = CreateIntegerPropertyEditor();
        var config = new Dictionary<string, object>
        {
            { "step", 1 },
        };

        // Act
        var schema = editor.GetValueSchema(config);

        // Assert
        Assert.That(schema, Is.Not.Null);
        Assert.That(schema!.ContainsKey("multipleOf"), Is.False);
    }

    [Test]
    public void ContentPickerPropertyEditor_Returns_ValueType_String()
    {
        // Arrange
        var editor = CreateContentPickerPropertyEditor();

        // Act
        var valueType = editor.GetValueType(null);

        // Assert
        Assert.That(valueType, Is.EqualTo(typeof(string)));
    }

    private static IntegerPropertyEditor CreateIntegerPropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.Integer)));

        return new IntegerPropertyEditor(dataValueEditorFactory);
    }

    private static ContentPickerPropertyEditor CreateContentPickerPropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.ContentPicker)));

        return new ContentPickerPropertyEditor(
            dataValueEditorFactory,
            Mock.Of<IIOHelper>());
    }
}
