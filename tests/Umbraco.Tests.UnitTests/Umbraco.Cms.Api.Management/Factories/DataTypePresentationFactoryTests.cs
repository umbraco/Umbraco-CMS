using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Contains unit tests for the <see cref="DataTypePresentationFactory"/> class in the Umbraco CMS API Management Factories namespace.
/// </summary>
[TestFixture]
public class DataTypePresentationFactoryTests
{
    private Mock<IDataTypeContainerService> _dataTypeContainerService = null!;
    private Mock<IDataValueEditorFactory> _dataValueEditorFactory = null!;
    private Mock<IConfigurationEditorJsonSerializer> _configurationEditorJsonSerializer = null!;

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _dataTypeContainerService = new Mock<IDataTypeContainerService>();
        _dataValueEditorFactory = new Mock<IDataValueEditorFactory>();
        _configurationEditorJsonSerializer = new Mock<IConfigurationEditorJsonSerializer>();
    }

    /// <summary>
    /// Tests that creating a data type with a label editor and a text value type sets the database type to Ntext.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that creating a data type with the Label editor and a default string value type sets the database type to Nvarchar.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that when the editor's configuration object does not implement IConfigureValueType,
    /// the editor's default value type is used during creation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that updating a label editor with a text value type sets the database type to Ntext.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
    {
        var propertyEditorCollection = new PropertyEditorCollection(
            new DataEditorCollection(() => [editor]));

        return new DataTypePresentationFactory(
            _dataTypeContainerService.Object,
            propertyEditorCollection,
            _dataValueEditorFactory.Object,
            _configurationEditorJsonSerializer.Object,
            TimeProvider.System);
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
}
