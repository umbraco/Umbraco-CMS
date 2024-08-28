using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class BlockListPropertyEditorTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    [Test]
    public async Task Can_Track_References()
    {
        var textPageContentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        textPageContentType.AllowedTemplates = Enumerable.Empty<ITemplate>();
        ContentTypeService.Save(textPageContentType);

        var textPage = ContentBuilder.CreateTextpageContent(textPageContentType, "My Picked Content", -1);
        ContentService.Save(textPage);

        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var blockListContentType = await CreateBlockListContentType(elementType);

        var contentElementKey = Guid.NewGuid();
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[]
                    {
                        new BlockListLayoutItem { ContentKey = contentElementKey }
                    }
                }
            },
            ContentData =
            [
                new()
                {
                    Key = contentElementKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new ()
                        {
                            Alias = "contentPicker",
                            Value = textPage.GetUdi()
                        }
                    ]
                }
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        var content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(blockListContentType);

        var references = valueEditor.GetReferences(content.GetValue("blocks")).ToArray();
        Assert.AreEqual(1, references.Length);
        var reference = references.First();
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedDocumentAlias, reference.RelationTypeAlias);
        Assert.AreEqual(textPage.GetUdi(), reference.Udi);
    }

    [Test]
    public async Task Can_Track_Tags()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var blockListContentType = await CreateBlockListContentType(elementType);

        var contentElementKey = Guid.NewGuid();
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[]
                    {
                        new BlockListLayoutItem { ContentKey = contentElementKey }
                    }
                }
            },
            ContentData =
            [
                new()
                {
                    Key = contentElementKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new ()
                        {
                            Alias = "tags",
                            // this is a little skewed, but the tags editor expects a serialized array of strings
                            Value = JsonSerializer.Serialize(new[] { "Tag One", "Tag Two", "Tag Three" })
                        }
                    ]
                }
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        var content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(blockListContentType);

        var tags = valueEditor.GetTags(content.GetValue("blocks"), null, null).ToArray();
        Assert.AreEqual(3, tags.Length);
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag One"));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Two"));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Three"));
    }

    [Test]
    public async Task Can_Handle_Variance_Change()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var blockListContentType = await CreateBlockListContentType(elementType);

        var contentElementKey = Guid.NewGuid();
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[]
                    {
                        new BlockListLayoutItem { ContentKey = contentElementKey }
                    }
                }
            },
            ContentData =
            [
                new()
                {
                    Key = contentElementKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new ()
                        {
                            Alias = "singleLineText",
                            Value = "The single line text"
                        }
                    ]
                }
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        var content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        elementType.Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(pt => pt.Alias == "singleLineText").Variations = ContentVariation.Culture;
        ContentTypeService.Save(elementType);

        var valueEditor = await GetValueEditor(blockListContentType);
        var toEditorValue = valueEditor.ToEditor(content.Properties["blocks"]!) as BlockListValue;
        Assert.IsNotNull(toEditorValue);
        Assert.AreEqual(1, toEditorValue.ContentData.Count);

        var properties = toEditorValue.ContentData.First().Values;
        Assert.AreEqual(1, properties.Count);
        Assert.Multiple(() =>
        {
            var property = properties.First();
            Assert.AreEqual("singleLineText", property.Alias);
            Assert.AreEqual("The single line text", property.Value);
            Assert.AreEqual("en-US", property.Culture);
        });
    }

    private async Task<IContentType> CreateBlockListContentType(IContentType elementType)
    {
        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .Build();
        ContentTypeService.Save(contentType);
        // re-fetch to wire up all key bindings (particularly to the datatype)
        return await ContentTypeService.GetAsync(contentType.Key);
    }

    private async Task<BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor> GetValueEditor(IContentType contentType)
    {
        var dataType = await DataTypeService.GetAsync(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "blocks").DataTypeKey);
        Assert.IsNotNull(dataType?.Editor);
        var valueEditor = dataType.Editor.GetValueEditor() as BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor;
        Assert.IsNotNull(valueEditor);

        return valueEditor;
    }
}
