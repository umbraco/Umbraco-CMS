using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class ContentTypeReferenceServiceTests : ContentTypeEditingServiceTestsBase
{
    private IContentTypeReferenceService ContentTypeReferenceService =>
        GetRequiredService<IContentTypeReferenceService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Can_Get_Referenced_DocumentKeys()
    {
        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType();
        var contentType = (await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var contentCreateModel = ContentEditingBuilder.CreateSimpleContent(contentType.Key);
        var content = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        var keys = await ContentTypeReferenceService.GetReferencedDocumentKeysAsync(contentType.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(content.Result.Content.Key));
        });
    }

    [Test]
    public async Task Can_Get_Multiple_Referenced_DocumentKeys()
    {
        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType();
        var contentTypeCreateModel2 = ContentTypeEditingBuilder.CreateSimpleContentType("otherTextPage", "Other text page");
        var contentType = (await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var contentType2 = (await ContentTypeEditingService.CreateAsync(contentTypeCreateModel2, Constants.Security.SuperUserKey)).Result!;
        var contentCreateModel = ContentEditingBuilder.CreateSimpleContent(contentType.Key);
        var contentCreateModel1 = ContentEditingBuilder.CreateSimpleContent(contentType.Key);
        var contentCreateModel2 = ContentEditingBuilder.CreateSimpleContent(contentType2.Key);
        var contentCreateModel3 = ContentEditingBuilder.CreateSimpleContent(contentType2.Key);
        var content = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        await ContentEditingService.CreateAsync(contentCreateModel1, Constants.Security.SuperUserKey);
        await ContentEditingService.CreateAsync(contentCreateModel2, Constants.Security.SuperUserKey);
        await ContentEditingService.CreateAsync(contentCreateModel3, Constants.Security.SuperUserKey);
        var keys = await ContentTypeReferenceService.GetReferencedDocumentKeysAsync(contentType.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(2));
            Assert.That(keys.Items.First(), Is.EqualTo(content.Result.Content.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypeKeys()
    {
        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1");
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2");

        var compositionCreateModel = ContentTypeCreateModel("Composition", "composition");
        compositionCreateModel.Properties = new[] { propertyType1 };
        var compositionContentType =
            (await ContentTypeEditingService.CreateAsync(compositionCreateModel, Constants.Security.SuperUserKey))
            .Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Properties = new[] { propertyType2 };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey))
            .Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };

        await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);

        var keys = await ContentTypeReferenceService.GetReferencedDocumentTypeKeysAsync(compositionContentType.Key,
            CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(contentType.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypes_From_BlockList_ContentElementTypeKey()
    {
        var elementTypeCreateModel = ContentTypeEditingBuilder.CreateElementType();
        var elementType = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel, Constants.Security.SuperUserKey)).Result!;

        var dataType = await CreateBlockList(elementType.Key);
        var keys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(elementType.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(dataType.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypes_From_BlockList_SettingsElementTypeKey()
    {
        var elementTypeCreateModel = ContentTypeEditingBuilder.CreateElementType();
        var elementTypeCreateModel2 = ContentTypeEditingBuilder.CreateElementType("otherElement", "Other Element");
        var elementType1 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var elementType2 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel2, Constants.Security.SuperUserKey)).Result!;

        var dataType = await CreateBlockList(elementType1.Key, elementType2.Key);
        var keys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(elementType2.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(dataType.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypes_From_BlockGrid_ContentElementTypeKey()
    {
        var elementTypeCreateModel = ContentTypeEditingBuilder.CreateElementType();
        var elementType = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var dataType = await CreateBlockGrid(elementType.Key);
        var keys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(elementType.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(dataType.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypes_From_BlockGrid_SettingsElementTypeKey()
    {
        var elementTypeCreateModel = ContentTypeEditingBuilder.CreateElementType();
        var elementTypeCreateModel2 = ContentTypeEditingBuilder.CreateElementType("otherElement", "Other Element");
        var elementType1 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var elementType2 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel2, Constants.Security.SuperUserKey)).Result!;
        var dataType = await CreateBlockGrid(elementType1.Key, elementType2.Key);
        var keys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(elementType2.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(dataType.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypes_From_RichText_ContentElementTypeKey()
    {
        var elementTypeCreateModel = ContentTypeEditingBuilder.CreateElementType();
        var elementTypeCreateModel2 = ContentTypeEditingBuilder.CreateElementType("otherElement", "Other Element");
        var elementType1 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var elementType2 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel2, Constants.Security.SuperUserKey)).Result!;

        var dataType = await CreateRichText(elementType1.Key, elementType2.Key);

        var keys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(elementType1.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(dataType.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypes_From_RichText_SettingsElementTypeKey()
    {
        var elementTypeCreateModel = ContentTypeEditingBuilder.CreateElementType();
        var elementTypeCreateModel2 = ContentTypeEditingBuilder.CreateElementType("otherElement", "Other Element");
        var elementType1 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var elementType2 = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel2, Constants.Security.SuperUserKey)).Result!;

        var dataType = await CreateRichText(elementType1.Key, elementType2.Key);

        var keys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(elementType2.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(dataType.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypes_From_Each_Editor_With_Multiple_Elements()
    {
        var elementTypeCreateModel = ContentTypeEditingBuilder.CreateElementType("blocklistElement", "Blocklist Element");
        var elementTypeCreateModel2 = ContentTypeEditingBuilder.CreateElementType("blockgridElement", "Blockgrid Element");
        var elementTypeCreateModel3 = ContentTypeEditingBuilder.CreateElementType("richtextElement", "RichTexts Element");
        var blockListElement = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var blockGridElement = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel2, Constants.Security.SuperUserKey)).Result!;
        var richTextElement = (await ContentTypeEditingService.CreateAsync(elementTypeCreateModel3, Constants.Security.SuperUserKey)).Result!;

        var blockList = await CreateBlockList(blockListElement.Key);
        var blockGrid = await CreateBlockGrid(blockGridElement.Key);
        var richText = await CreateRichText(richTextElement.Key);

        var blockListkeys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(blockListElement.Key, CancellationToken.None, 0, 100);
        var blockGridKeys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(blockGridElement.Key, CancellationToken.None, 0, 100);
        var richTextKeys = await ContentTypeReferenceService.GetReferencedElementsFromDataTypesAsync(richTextElement.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(blockListkeys.Total, Is.EqualTo(1));
            Assert.That(blockListkeys.Items.First(), Is.EqualTo(blockList.Key));
            Assert.That(blockGridKeys.Total, Is.EqualTo(1));
            Assert.That(blockGridKeys.Items.First(), Is.EqualTo(blockGrid.Key));
            Assert.That(richTextKeys.Total, Is.EqualTo(1));
            Assert.That(richTextKeys.Items.First(), Is.EqualTo(richText.Key));
        });
    }

    private async Task<IDataType> CreateRichText(Guid contentElementTypeKey, Guid? settingsElementTypeKey = null)
    {
        var blockListConfig = new RichTextConfiguration.RichTextBlockConfiguration[] {
            new()
            {
                ContentElementTypeKey = contentElementTypeKey, SettingsElementTypeKey = settingsElementTypeKey
            }
        };

        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.RichText], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object> { { "blocks", blockListConfig } },
            Name = "My Block Editor",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        return dataType;
    }

    private async Task<IDataType> CreateBlockList(Guid contentElementTypeKey, Guid? settingsElementTypeKey = null)
    {
        var blockListConfig = new BlockListConfiguration.BlockConfiguration[] {
            new()
            {
                ContentElementTypeKey = contentElementTypeKey, SettingsElementTypeKey = settingsElementTypeKey

            }
        };

        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object> { { "blocks", blockListConfig } },
            Name = "My Block Editor",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        return dataType;
    }

    private async Task<IDataType> CreateBlockGrid(Guid contentElementTypeKey, Guid? settingsElementTypeKey = null)
    {
        var blockListConfig = new BlockGridConfiguration.BlockGridBlockConfiguration[] {
            new()
            {
                ContentElementTypeKey = contentElementTypeKey, SettingsElementTypeKey = settingsElementTypeKey
            }
        };

        var dataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockGrid], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object> { { "blocks", blockListConfig } },
            Name = "My Block Editor",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        return dataType;
    }
}
