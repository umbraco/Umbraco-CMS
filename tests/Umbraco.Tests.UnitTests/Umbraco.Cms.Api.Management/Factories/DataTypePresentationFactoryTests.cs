using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class DataTypePresentationFactoryTests
{
    private Mock<IDataTypeContainerService> _dataTypeContainerService = null!;
    private Mock<IDataValueEditorFactory> _dataValueEditorFactory = null!;
    private Mock<IConfigurationEditorJsonSerializer> _configurationEditorJsonSerializer = null!;

    [SetUp]
    public void SetUp()
    {
        _dataTypeContainerService = new Mock<IDataTypeContainerService>();
        _dataValueEditorFactory = new Mock<IDataValueEditorFactory>();
        _configurationEditorJsonSerializer = new Mock<IConfigurationEditorJsonSerializer>();
    }

    [Test]
    public async Task CreateAsync_LabelEditorWithTextValueType_SetsDatabaseTypeToNtext()
    {
        // Arrange
        var configureValueType = Mock.Of<IConfigureValueType>(x => x.ValueType == ValueTypes.Text);
        IDataEditor editor = CreateMockEditor("Umbraco.Label", configureValueType);
        DataTypePresentationFactory factory = CreateFactory(editor);

        var requestModel = new CreateDataTypeRequestModel
        {
            Name = "Label (long string)",
            EditorAlias = "Umbraco.Label",
            EditorUiAlias = "Umb.PropertyEditorUi.Label",
            Values =
            [
                new DataTypePropertyPresentationModel { Alias = Constants.PropertyEditors.ConfigurationKeys.DataValueType, Value = ValueTypes.Text },
            ],
        };

        // Act
        var result = await factory.CreateAsync(requestModel);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ValueStorageType.Ntext, result.Result.DatabaseType);
    }

    [Test]
    public async Task CreateAsync_LabelEditorWithDefaultStringValueType_SetsDatabaseTypeToNvarchar()
    {
        // Arrange
        var configureValueType = Mock.Of<IConfigureValueType>(x => x.ValueType == ValueTypes.String);
        IDataEditor editor = CreateMockEditor("Umbraco.Label", configureValueType);
        DataTypePresentationFactory factory = CreateFactory(editor);

        var requestModel = new CreateDataTypeRequestModel
        {
            Name = "Label (string)",
            EditorAlias = "Umbraco.Label",
            EditorUiAlias = "Umb.PropertyEditorUi.Label",
            Values =
            [
                new DataTypePropertyPresentationModel { Alias = Constants.PropertyEditors.ConfigurationKeys.DataValueType, Value = ValueTypes.String },
            ],
        };

        // Act
        var result = await factory.CreateAsync(requestModel);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ValueStorageType.Nvarchar, result.Result.DatabaseType);
    }

    [Test]
    public async Task CreateAsync_EditorWithoutConfigureValueType_UsesEditorDefaultValueType()
    {
        // Arrange - editor whose configuration object does NOT implement IConfigureValueType.
        IDataEditor editor = CreateMockEditor("Umbraco.TextBox", configurationObject: new object(), defaultValueType: ValueTypes.String);
        DataTypePresentationFactory factory = CreateFactory(editor);

        var requestModel = new CreateDataTypeRequestModel
        {
            Name = "Textstring",
            EditorAlias = "Umbraco.TextBox",
            EditorUiAlias = "Umb.PropertyEditorUi.TextBox",
            Values = [],
        };

        // Act
        var result = await factory.CreateAsync(requestModel);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ValueStorageType.Nvarchar, result.Result.DatabaseType);
    }

    [Test]
    public async Task CreateAsync_ConfigurationWithCollectionStoredAsJsonString_DoesNotThrowAndFallsBackToValueEditorType()
    {
        // Arrange - a real configuration editor whose typed configuration has a collection property,
        // fed a value stored as a JSON string (as a custom multi-value config editor does). This is the
        // shape from https://github.com/umbraco/Umbraco-CMS/issues/23057: round-tripping the configuration
        // dictionary into the typed configuration throws because the string can't bind to List<T>.
        IConfigurationEditorJsonSerializer serializer =
            new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var configurationEditor = new TestCollectionConfigurationEditor(Mock.Of<IIOHelper>());

        IDataEditor editor = CreateMockEditorWithConfigurationEditor(
            "Test.CollectionEditor",
            configurationEditor,
            defaultValueType: ValueTypes.Json);
        DataTypePresentationFactory factory = CreateFactory(editor, serializer);

        var requestModel = new CreateDataTypeRequestModel
        {
            Name = "Collection editor",
            EditorAlias = "Test.CollectionEditor",
            EditorUiAlias = "Test.CollectionEditorUi",
            Values =
            [
                new DataTypePropertyPresentationModel { Alias = "buttons", Value = "[{\"value\":\"a\"}]" },
            ],
        };

        // Act
        var result = await factory.CreateAsync(requestModel);

        // Assert - the save succeeds and falls back to the value editor's value type.
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ValueStorageType.Ntext, result.Result.DatabaseType);
    }

    [Test]
    public async Task CreateAsync_Update_LabelEditorWithTextValueType_SetsDatabaseTypeToNtext()
    {
        // Arrange
        var configureValueType = Mock.Of<IConfigureValueType>(x => x.ValueType == ValueTypes.Text);
        IDataEditor editor = CreateMockEditor("Umbraco.Label", configureValueType);
        DataTypePresentationFactory factory = CreateFactory(editor);

        var current = new DataType(editor, _configurationEditorJsonSerializer.Object)
        {
            Name = "Label (string)",
            DatabaseType = ValueStorageType.Nvarchar,
        };

        var requestModel = new UpdateDataTypeRequestModel
        {
            Name = "Label (long string)",
            EditorAlias = "Umbraco.Label",
            EditorUiAlias = "Umb.PropertyEditorUi.Label",
            Values =
            [
                new DataTypePropertyPresentationModel { Alias = Constants.PropertyEditors.ConfigurationKeys.DataValueType, Value = ValueTypes.Text },
            ],
        };

        // Act
        var result = await factory.CreateAsync(requestModel, current);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ValueStorageType.Ntext, result.Result.DatabaseType);
    }

    private DataTypePresentationFactory CreateFactory(IDataEditor editor)
        => CreateFactory(editor, _configurationEditorJsonSerializer.Object);

    private DataTypePresentationFactory CreateFactory(IDataEditor editor, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        var propertyEditorCollection = new PropertyEditorCollection(
            new DataEditorCollection(() => [editor]));

        return new DataTypePresentationFactory(
            _dataTypeContainerService.Object,
            propertyEditorCollection,
            _dataValueEditorFactory.Object,
            configurationEditorJsonSerializer,
            TimeProvider.System,
            NullLogger<DataTypePresentationFactory>.Instance);
    }

    private IDataEditor CreateMockEditorWithConfigurationEditor(string alias, IConfigurationEditor configurationEditor, string defaultValueType = ValueTypes.String)
    {
        var mockValueEditor = new Mock<IDataValueEditor>();
        mockValueEditor.Setup(v => v.ValueType).Returns(defaultValueType);

        var mockEditor = new Mock<IDataEditor>();
        mockEditor.Setup(e => e.Alias).Returns(alias);
        mockEditor.Setup(e => e.GetConfigurationEditor()).Returns(configurationEditor);
        mockEditor.Setup(e => e.GetValueEditor()).Returns(mockValueEditor.Object);

        return mockEditor.Object;
    }

    private IDataEditor CreateMockEditor(string alias, object configurationObject, string defaultValueType = ValueTypes.String)
    {
        var mockConfigEditor = new Mock<IConfigurationEditor>();
        mockConfigEditor.Setup(c => c.DefaultConfiguration)
            .Returns(new Dictionary<string, object>());
        mockConfigEditor.Setup(c => c.FromConfigurationEditor(It.IsAny<IDictionary<string, object>>()))
            .Returns((IDictionary<string, object> config) => config);
        mockConfigEditor.Setup(c => c.ToConfigurationObject(
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IConfigurationEditorJsonSerializer>()))
            .Returns(configurationObject);

        var mockValueEditor = new Mock<IDataValueEditor>();
        mockValueEditor.Setup(v => v.ValueType).Returns(defaultValueType);

        var mockEditor = new Mock<IDataEditor>();
        mockEditor.Setup(e => e.Alias).Returns(alias);
        mockEditor.Setup(e => e.GetConfigurationEditor()).Returns(mockConfigEditor.Object);
        mockEditor.Setup(e => e.GetValueEditor()).Returns(mockValueEditor.Object);

        return mockEditor.Object;
    }

    private sealed class TestCollectionConfigurationEditor : ConfigurationEditor<TestCollectionConfiguration>
    {
        public TestCollectionConfigurationEditor(IIOHelper ioHelper)
            : base(ioHelper)
        {
        }
    }

    private sealed class TestCollectionConfiguration
    {
        [ConfigurationField("buttons")]
        public List<TestCollectionItem> Buttons { get; set; } = [];
    }

    private sealed class TestCollectionItem
    {
        public string Value { get; set; } = string.Empty;
    }
}
