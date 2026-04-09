using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Services.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Services;

[TestFixture]
public partial class PropertyEditorSchemaServiceTests
{
    private Mock<IDataTypeService> _dataTypeServiceMock = null!;
    private List<IDataEditor> _dataEditors = null!;
    private PropertyEditorSchemaService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _dataTypeServiceMock = new Mock<IDataTypeService>();
        _dataEditors = [];
        var dataEditorCollection = new DataEditorCollection(() => _dataEditors);
        _sut = new PropertyEditorSchemaService(_dataTypeServiceMock.Object, dataEditorCollection);
    }

    [Test]
    public void SupportsSchema_Returns_True_For_Editor_Implementing_IValueSchemaProvider()
    {
        // Arrange
        var schemaProviderEditor = new MockSchemaProviderEditor();
        SetupDataEditors(schemaProviderEditor);

        // Act
        var result = _sut.SupportsSchema("test.schemaProvider");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SupportsSchema_Returns_False_For_Editor_Not_Implementing_IValueSchemaProvider()
    {
        // Arrange
        var regularEditor = new MockRegularEditor();
        SetupDataEditors(regularEditor);

        // Act
        var result = _sut.SupportsSchema("test.regular");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SupportsSchema_Returns_False_For_Unknown_Editor()
    {
        // Arrange
        SetupDataEditors();

        // Act
        var result = _sut.SupportsSchema("unknown.editor");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetValueType_Returns_Type_From_Provider()
    {
        // Arrange
        var schemaProviderEditor = new MockSchemaProviderEditor();
        SetupDataEditors(schemaProviderEditor);

        // Act
        var result = _sut.GetValueType("test.schemaProvider", null);

        // Assert
        Assert.That(result, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GetValueType_Returns_Null_For_NonProvider_Editor()
    {
        // Arrange
        var regularEditor = new MockRegularEditor();
        SetupDataEditors(regularEditor);

        // Act
        var result = _sut.GetValueType("test.regular", null);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetValueSchema_Returns_Schema_From_Provider()
    {
        // Arrange
        var schemaProviderEditor = new MockSchemaProviderEditor();
        SetupDataEditors(schemaProviderEditor);

        // Act
        var result = _sut.GetValueSchema("test.schemaProvider", null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!["type"]?.GetValue<string>(), Is.EqualTo("string"));
    }

    [Test]
    public void GetValueSchema_Returns_Null_For_NonProvider_Editor()
    {
        // Arrange
        var regularEditor = new MockRegularEditor();
        SetupDataEditors(regularEditor);

        // Act
        var result = _sut.GetValueSchema("test.regular", null);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetSchemaAsync_Returns_Success_With_Both_Type_And_Schema()
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var schemaProviderEditor = new MockSchemaProviderEditor();
        SetupDataEditors(schemaProviderEditor);
        SetupDataType(dataTypeKey, "test.schemaProvider");

        // Act
        var result = await _sut.GetSchemaAsync(dataTypeKey);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));
        Assert.That(result.Result.ValueType, Is.EqualTo(typeof(string)));
        Assert.That(result.Result.JsonSchema, Is.Not.Null);
        Assert.That(result.Result.JsonSchema!["type"]?.GetValue<string>(), Is.EqualTo("string"));
    }

    [Test]
    public async Task GetSchemaAsync_Returns_DataTypeNotFound_When_DataType_Not_Found()
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        _dataTypeServiceMock.Setup(x => x.GetAsync(dataTypeKey)).ReturnsAsync((IDataType?)null);

        // Act
        var result = await _sut.GetSchemaAsync(dataTypeKey);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.DataTypeNotFound));
    }

    [Test]
    public async Task GetSchemaAsync_Returns_SchemaNotSupported_When_Editor_Does_Not_Support_Schema()
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var regularEditor = new MockRegularEditor();
        SetupDataEditors(regularEditor);
        SetupDataType(dataTypeKey, "test.regular");

        // Act
        var result = await _sut.GetSchemaAsync(dataTypeKey);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.SchemaNotSupported));
    }

    private void SetupDataEditors(params IDataEditor[] editors)
    {
        _dataEditors.Clear();
        _dataEditors.AddRange(editors);
    }

    private void SetupDataType(Guid key, string editorAlias, object? configuration = null)
    {
        var dataType = Mock.Of<IDataType>(dt =>
            dt.Key == key &&
            dt.EditorAlias == editorAlias &&
            dt.ConfigurationObject == configuration);

        _dataTypeServiceMock.Setup(x => x.GetAsync(key)).ReturnsAsync(dataType);
    }

    private class MockSchemaProviderEditor : IDataEditor, IValueSchemaProvider
    {
        public string Alias => "test.schemaProvider";
        public string Name => "Test Schema Provider";
        public string Icon => "icon-test";
        public string? Group => null;
        public bool IsDeprecated => false;
        public IDictionary<string, object>? DefaultConfiguration => null;
        public IPropertyIndexValueFactory PropertyIndexValueFactory => null!;

        public IDataValueEditor GetValueEditor() => null!;
        public IDataValueEditor GetValueEditor(object? configuration) => null!;
        public IConfigurationEditor GetConfigurationEditor() => null!;

        public Type? GetValueType(object? configuration) => typeof(string);

        public JsonObject? GetValueSchema(object? configuration) => new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = "string",
        };
    }

    private class MockRegularEditor : IDataEditor
    {
        public string Alias => "test.regular";
        public string Name => "Test Regular";
        public string Icon => "icon-test";
        public string? Group => null;
        public bool IsDeprecated => false;
        public IDictionary<string, object>? DefaultConfiguration => null;
        public IPropertyIndexValueFactory PropertyIndexValueFactory => null!;

        public IDataValueEditor GetValueEditor() => null!;
        public IDataValueEditor GetValueEditor(object? configuration) => null!;
        public IConfigurationEditor GetConfigurationEditor() => null!;
    }
}
