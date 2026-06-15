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
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the DataTypeService.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DataTypeServiceTests : UmbracoIntegrationTest
{
    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

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
        var result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));

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
        Assert.That(dataType, Is.Not.Null);

        // Act
        dataType.Name += " UPDATED";
        var result = await DataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));

        // Assert
        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.Name, Does.EndWith(" UPDATED"));
    }

    [Test]
    public async Task Can_Get_All_By_Editor_Alias()
    {
        // Arrange
        async Task<IDataType> CreateTextBoxDataType()
        {
            IDataType dataType =
                new DataType(new TextboxPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
                {
                    Name = Guid.NewGuid().ToString(),
                    DatabaseType = ValueStorageType.Nvarchar
                };
            var result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
            Assert.That(result.Success, Is.True);
            return result.Result;
        }

        IDataType dataType1 = await CreateTextBoxDataType();
        IDataType dataType2 = await CreateTextBoxDataType();
        IDataType dataType3 = await CreateTextBoxDataType();

        // Act
        IEnumerable<IDataType> dataTypes = await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox);

        // Assert
        Assert.That(dataTypes.Count(), Is.GreaterThanOrEqualTo(3));
        Assert.That(dataTypes.All(dataType => dataType.EditorAlias == Constants.PropertyEditors.Aliases.TextBox), Is.True);
        Assert.That(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType1.Key), Is.Not.Null);
        Assert.That(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType2.Key), Is.Not.Null);
        Assert.That(dataTypes.FirstOrDefault(dataType => dataType.Key == dataType3.Key), Is.Not.Null);
    }

    [Test]
    public async Task Can_Get_By_Id()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.That(dataType, Is.Not.Null);

        // Act
        IDataType? actual = await DataTypeService.GetAsync(dataType.Key);

        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Key, Is.EqualTo(dataType.Key));
    }

    [Test]
    public async Task Can_Get_By_Name()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox)).FirstOrDefault();
        Assert.That(dataType, Is.Not.Null);

        // Act
        IDataType? actual = await DataTypeService.GetAsync(dataType.Name);

        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Key, Is.EqualTo(dataType.Key));
    }

    [Test]
    public async Task DataTypeService_Can_Delete_DataType_And_Clear_Usages()
    {
        // Arrange
        var dataTypeDefinitions = await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText);
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var doctype =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(doctype, Constants.Security.SuperUserKey);

        // validate the assumptions used for assertions later in this test
        var contentType = ContentTypeService.Get(doctype.Id);
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
        Assert.That(contentType.PropertyTypes.SingleOrDefault(pt => pt.PropertyEditorAlias is Constants.PropertyEditors.Aliases.RichText), Is.Not.Null);

        // Act
        var definition = dataTypeDefinitions.First();
        var definitionKey = definition.Key;
        var result = await DataTypeService.DeleteAsync(definitionKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Key, Is.EqualTo(definitionKey));

        var deletedDefinition = await DataTypeService.GetAsync(definitionKey);

        // Assert
        Assert.That(deletedDefinition, Is.Null);

        // Further assertions against the ContentType that contains PropertyTypes based on the TextField
        contentType = ContentTypeService.Get(doctype.Id);
        Assert.That(contentType.Alias, Is.EqualTo("umbTextpage"));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Can_Create_DataType_In_Container()
    {
        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container.Id
            },
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));

        var dataType = await DataTypeService.GetAsync(result.Result.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.ParentId, Is.EqualTo(container.Id));
    }

    [Test]
    public async Task Can_Move_DataType_To_Container()
    {
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.ParentId, Is.EqualTo(Constants.System.Root));

        var result = await DataTypeService.MoveAsync(dataType, container.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.ParentId, Is.EqualTo(container.Id));
    }

    [Test]
    public async Task Can_Move_DataType_To_Root()
    {
        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container.Id
            },
            Constants.Security.SuperUserKey)).Result;

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.ParentId, Is.EqualTo(container.Id));

        var result = await DataTypeService.MoveAsync(dataType, null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Copy_DataType_To_Root()
    {
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Key, Is.Not.EqualTo(dataType.Key));
        Assert.That(result.Result.Name, Is.Not.EqualTo(dataType.Name));

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Copy_DataType_To_Container()
    {
        var container = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, container.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Key, Is.Not.EqualTo(dataType.Key));
        Assert.That(result.Result.Name, Is.Not.EqualTo(dataType.Name));

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.ParentId, Is.EqualTo(container.Id));
    }

    [Test]
    public async Task Can_Copy_DataType_Between_Containers()
    {
        var container1 = (await DataTypeContainerService.CreateAsync(null, "Root Container 1", null, Constants.Security.SuperUserKey)).Result;
        var container2 = (await DataTypeContainerService.CreateAsync(null, "Root Container 2", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container1.Id
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, container2.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Key, Is.Not.EqualTo(dataType.Key));
        Assert.That(result.Result.Name, Is.Not.EqualTo(dataType.Name));

        IDataType original = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(original, Is.Not.Null);
        Assert.That(original.ParentId, Is.EqualTo(container1.Id));

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.ParentId, Is.EqualTo(container2.Id));
    }

    [Test]
    public async Task Can_Copy_DataType_From_Container_To_Root()
    {
        var container1 = (await DataTypeContainerService.CreateAsync(null, "Root Container 1", null, Constants.Security.SuperUserKey)).Result;
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext,
                ParentId = container1.Id
            },
            Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeService.CopyAsync(dataType, null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Key, Is.Not.EqualTo(dataType.Key));
        Assert.That(result.Result.Name, Is.Not.EqualTo(dataType.Name));

        IDataType original = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(original, Is.Not.Null);
        Assert.That(original.ParentId, Is.EqualTo(container1.Id));

        IDataType copy = await DataTypeService.GetAsync(result.Result.Key);
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy.ParentId, Is.EqualTo(Constants.System.Root));
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
        var result = await DataTypeService.CreateAsync(dataTypeDefinition, Constants.Security.SuperUserKey);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.InvalidName));
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
        var result = await DataTypeService.CreateAsync(dataTypeDefinition, Constants.Security.SuperUserKey);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.InvalidName));
    }

    [Test]
    public async Task Cannot_Save_DataType_With_Empty_Name()
    {
        // Act
        var dataTypeDefinition =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = string.Empty,
                DatabaseType = ValueStorageType.Ntext
            };

        // Act & Assert
        var result = await DataTypeService.CreateAsync(dataTypeDefinition, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.InvalidName));
    }

    [Test]
    public async Task Cannot_Move_DataType_To_Non_Existing_Container()
    {
        var dataType = (await DataTypeService.CreateAsync(
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            },
            Constants.Security.SuperUserKey)).Result;

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.ParentId, Is.EqualTo(Constants.System.Root));

        var result = await DataTypeService.MoveAsync(dataType, Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.ParentNotFound));

        dataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(dataType, Is.Not.Null);
        Assert.That(dataType.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [TestCase(Constants.DataTypes.Guids.LabelDateTime)]
    [TestCase(Constants.DataTypes.Guids.Textstring)]
    [TestCase(Constants.DataTypes.Guids.Checkbox)]
    [TestCase(Constants.DataTypes.Guids.ListViewContent)]
    [TestCase(Constants.DataTypes.Guids.ListViewMedia)]
    public async Task Cannot_Delete_NonDeletable_DataType(string dataTypeKey)
    {
        var dataType = await DataTypeService.GetAsync(Guid.Parse(dataTypeKey));
        Assert.That(dataType, Is.Not.Null);

        var result = await DataTypeService.DeleteAsync(dataType.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DataTypeOperationStatus.NonDeletable));
    }

    [Test]
    public async Task DataTypeService_Can_Get_References()
    {
        IEnumerable<IDataType> dataTypeDefinitions = await DataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText);

        IContentType documentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Text Page");
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);

        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("umbMediaItem", "Media Item");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        documentType = ContentTypeService.Get(documentType.Id);
        Assert.That(documentType.PropertyTypes.SingleOrDefault(pt => pt.PropertyEditorAlias is Constants.PropertyEditors.Aliases.RichText), Is.Not.Null);

        mediaType = MediaTypeService.Get(mediaType.Id);
        Assert.That(mediaType.PropertyTypes.SingleOrDefault(pt => pt.PropertyEditorAlias is Constants.PropertyEditors.Aliases.RichText), Is.Not.Null);

        var definition = dataTypeDefinitions.First();
        var definitionKey = definition.Key;
        PagedModel<RelationItemModel> result = await DataTypeService.GetPagedRelationsAsync(definitionKey, 0, 10);
        Assert.That(result.Total, Is.EqualTo(2));

        RelationItemModel firstResult = result.Items.First();
        Assert.That(firstResult.ContentTypeAlias, Is.EqualTo("umbTextpage"));
        Assert.That(firstResult.ContentTypeName, Is.EqualTo("Text Page"));
        Assert.That(firstResult.ContentTypeIcon, Is.EqualTo("icon-document"));
        Assert.That(firstResult.ContentTypeKey, Is.EqualTo(documentType.Key));
        Assert.That(firstResult.NodeAlias, Is.EqualTo("bodyText"));
        Assert.That(firstResult.NodeName, Is.EqualTo("Body text"));

        RelationItemModel secondResult = result.Items.Skip(1).First();
        Assert.That(secondResult.ContentTypeAlias, Is.EqualTo("umbMediaItem"));
        Assert.That(secondResult.ContentTypeName, Is.EqualTo("Media Item"));
        Assert.That(secondResult.ContentTypeIcon, Is.EqualTo("icon-picture"));
        Assert.That(secondResult.ContentTypeKey, Is.EqualTo(mediaType.Key));
        Assert.That(secondResult.NodeAlias, Is.EqualTo("bodyText"));
        Assert.That(secondResult.NodeName, Is.EqualTo("Body text"));
    }

    [Test]
    public async Task DataTypeService_Can_Get_ListView_References()
    {
        // Arrange - Create a custom list view data type
        var customListViewKey = Guid.NewGuid();
        IDataType customListView = new DataType(
            GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.ListView],
            ConfigurationEditorJsonSerializer)
        {
            Key = customListViewKey,
            Name = "Custom List View For Test",
            DatabaseType = ValueStorageType.Nvarchar
        };
        var createResult = await DataTypeService.CreateAsync(customListView, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        // Create a document type that uses this list view as its collection
        IContentType documentTypeWithListView = new ContentTypeBuilder()
            .WithAlias("listViewContainer")
            .WithName("List View Container")
            .WithIsContainer(customListViewKey)
            .Build();
        await ContentTypeService.CreateAsync(documentTypeWithListView, Constants.Security.SuperUserKey);

        // Act - Get references for the custom list view
        PagedModel<RelationItemModel> result = await DataTypeService.GetPagedRelationsAsync(customListViewKey, 0, 10);

        // Assert - The document type should be listed as a reference
        Assert.That(result.Total, Is.EqualTo(1));

        RelationItemModel listViewReference = result.Items.First();
        Assert.That(listViewReference.ContentTypeAlias, Is.EqualTo("listViewContainer"));
        Assert.That(listViewReference.ContentTypeName, Is.EqualTo("List View Container"));
        Assert.That(listViewReference.ContentTypeKey, Is.EqualTo(documentTypeWithListView.Key));
        Assert.That(listViewReference.NodeName, Is.EqualTo("Custom List View For Test"));
    }

    [Test]
    public async Task Gets_MissingPropertyEditor_When_Editor_NotFound()
    {
        // Arrange
        IDataType? dataType = (await DataTypeService.CreateAsync(
            new DataType(new TestEditor(DataValueEditorFactory), ConfigurationEditorJsonSerializer)
            {
                Name = "Test Missing Editor",
                DatabaseType = ValueStorageType.Ntext,
            },
            Constants.Security.SuperUserKey)).Result;

        Assert.That(dataType, Is.Not.Null);

        // Act
        IDataType? actual = await DataTypeService.GetAsync(dataType.Key);

        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Key, Is.EqualTo(dataType.Key));
        Assert.That(actual.Editor, Is.AssignableFrom(typeof(MissingPropertyEditor)));
        Assert.That(actual.EditorAlias, Is.EqualTo("Test Editor"), "The alias should be the same as the original editor");
        Assert.That(actual.EditorUiAlias, Is.EqualTo("Umb.PropertyEditorUi.Missing"), "The editor UI alias should be the Missing Editor UI");
    }

    private class TestEditor : DataEditor
    {
        public TestEditor(IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory) =>
            Alias = "Test Editor";
    }
}
