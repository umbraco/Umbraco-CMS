// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the DataTypeService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DataTypeServiceTests : UmbracoIntegrationTest
{
    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IEditorConfigurationParser EditorConfigurationParser => GetRequiredService<IEditorConfigurationParser>();

    [Test]
    public async Task Can_Create_New_DataTypeDefinition()
    {
        // Act
        IDataType dataType =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            };
        var result = await DataTypeService.CreateAsync(dataType);
        Assert.True(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);

        // Assert
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.HasIdentity, Is.True);

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
    }

    [Test]
    public async Task Can_Update_Existing_DataTypeDefinition()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.NotNull(dataType);

        // Act
        dataType.Name += " UPDATED";
        var result = await DataTypeService.UpdateAsync(dataType);
        Assert.True(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);

        // Assert
        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.NotNull(dataType);
        Assert.True(dataType.Name.EndsWith(" UPDATED"));
    }

    [Test]
    public async Task Can_Get_All_By_Editor_Alias()
    {
        // Arrange
        async Task<IDataType> CreateTextBoxDataType()
        {
            IDataType dataType =
                new DataType(new TextboxPropertyEditor(DataValueEditorFactory, IOHelper, EditorConfigurationParser), ConfigurationEditorJsonSerializer)
                {
                    Name = Guid.NewGuid().ToString(),
                    DatabaseType = ValueStorageType.Nvarchar
                };
            var result = await DataTypeService.CreateAsync(dataType);
            Assert.True(result.Success);
            return result.Result;
        }

        IDataType dataType1 = await CreateTextBoxDataType();
        IDataType dataType2 = await CreateTextBoxDataType();
        IDataType dataType3 = await CreateTextBoxDataType();

        // Act
        IEnumerable<IDataType> dataTypes = await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox);

        // Assert
        Assert.True(dataTypes.Count() >= 3);
        Assert.True(dataTypes.All(dataType => dataType.EditorAlias == Constants.PropertyEditors.Aliases.TextBox));
        Assert.NotNull(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType1.Key));
        Assert.NotNull(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType2.Key));
        Assert.NotNull(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType3.Key));
    }

    [Test]
    public async Task Can_Get_By_Id()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.NotNull(dataType);

        // Act
        IDataType? actual = await DataTypeService.GetAsync(dataType.Key);

        // Assert
        Assert.NotNull(actual);
        Assert.AreEqual(dataType.Key, actual.Key);
    }

    [Test]
    public async Task Can_Get_By_Name()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.NotNull(dataType);

        // Act
        IDataType? actual = await DataTypeService.GetAsync(dataType.Name);

        // Assert
        Assert.NotNull(actual);
        Assert.AreEqual(dataType.Key, actual.Key);
    }

    [Test]
    public async Task DataTypeService_Can_Delete_Textfield_DataType_And_Clear_Usages()
    {
        // Arrange
        var dataTypeDefinitions = await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox);
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var doctype =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentTypeService.Save(doctype);

        // Act
        var definition = dataTypeDefinitions.First();
        var definitionKey = definition.Key;
        var result = await DataTypeService.DeleteAsync(definitionKey);
        Assert.True(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.Success, result.Status);
        Assert.NotNull(result.Result);
        Assert.AreEqual(definitionKey, result.Result.Key);

        var deletedDefinition = await DataTypeService.GetAsync(definitionKey);

        // Assert
        Assert.That(deletedDefinition, Is.Null);

        // Further assertions against the ContentType that contains PropertyTypes based on the TextField
        var contentType = ContentTypeService.Get(doctype.Id);
        Assert.That(contentType.Alias, Is.EqualTo("umbTextpage"));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Cannot_Create_DataType_With_Empty_Name()
    {
        // Act
        var dataTypeDefinition =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = string.Empty,
                DatabaseType = ValueStorageType.Ntext
            };

        // Act
        var result = await DataTypeService.CreateAsync(dataTypeDefinition);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.InvalidName, result.Status);
    }

    [Test]
    public async Task Cannot_Create_DataType_With_Too_Long_Name()
    {
        // Act
        var dataTypeDefinition =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = new string('a', 256),
                DatabaseType = ValueStorageType.Ntext
            };

        // Act
        var result = await DataTypeService.CreateAsync(dataTypeDefinition);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeOperationStatus.InvalidName, result.Status);
    }

    [Test]
    public void Cannot_Save_DataType_With_Empty_Name()
    {
        // Act
        var dataTypeDefinition =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = string.Empty,
                DatabaseType = ValueStorageType.Ntext
            };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DataTypeService.Save(dataTypeDefinition));
    }
}
