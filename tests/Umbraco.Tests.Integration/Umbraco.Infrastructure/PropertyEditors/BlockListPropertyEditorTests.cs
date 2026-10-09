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
internal sealed class BlockListPropertyEditorTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

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
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag One" && tag.LanguageId == null));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Two" && tag.LanguageId == null));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Three" && tag.LanguageId == null));
    }

    [Test]
    public async Task Can_Track_Tags_For_Block_Level_Variance()
    {
        var result = await LanguageService.CreateAsync(
            new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var daDkId = result.Result.Id;

        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        elementType.Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(p => p.Alias == "tags").Variations = ContentVariation.Culture;
        ContentTypeService.Save(elementType);

        var blockListContentType = await CreateBlockListContentType(elementType);
        blockListContentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(blockListContentType);

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
                            Value = JsonSerializer.Serialize(new[] { "Tag One EN", "Tag Two EN", "Tag Three EN" }),
                            Culture = "en-US"
                        },
                        new ()
                        {
                            Alias = "tags",
                            // this is a little skewed, but the tags editor expects a serialized array of strings
                            Value = JsonSerializer.Serialize(new[] { "Tag One DA", "Tag Two DA", "Tag Three DA" }),
                            Culture = "da-DK"
                        }
                    ]
                }
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        var content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithCultureName("en-US", "My Blocks EN")
            .WithCultureName("da-DK", "My Blocks DA")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(blockListContentType);

        var tags = valueEditor.GetTags(content.GetValue("blocks"), null, null).ToArray();
        Assert.AreEqual(6, tags.Length);
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag One EN" && tag.LanguageId == 1));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Two EN" && tag.LanguageId == 1));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Three EN" && tag.LanguageId == 1));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag One DA" && tag.LanguageId == daDkId));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Two DA" && tag.LanguageId == daDkId));
        Assert.IsNotNull(tags.Single(tag => tag.Text == "Tag Three DA" && tag.LanguageId == daDkId));
    }

    [Test]
    public async Task Can_Handle_Culture_Variance_Addition()
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
            ],
            Expose =
            [
                new (contentElementKey, null, null)
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

        Assert.AreEqual(1, toEditorValue.Expose.Count);
        Assert.Multiple(() =>
        {
            var itemVariation = toEditorValue.Expose[0];
            Assert.AreEqual(contentElementKey, itemVariation.ContentKey);
            Assert.AreEqual("en-US", itemVariation.Culture);
        });
    }

    [Test]
    public async Task Can_Handle_Culture_Variance_Removal()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        elementType.Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(pt => pt.Alias == "singleLineText").Variations = ContentVariation.Culture;
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
                            Value = "The single line text",
                            Culture = "en-US"
                        }
                    ]
                }
            ],
            Expose =
            [
                new (contentElementKey, "en-US", null)
            ]
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockListValue);

        var content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        elementType.PropertyTypes.First(pt => pt.Alias == "singleLineText").Variations = ContentVariation.Nothing;
        elementType.Variations = ContentVariation.Nothing;
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
            Assert.AreEqual(null, property.Culture);
        });

        Assert.AreEqual(1, toEditorValue.Expose.Count);
        Assert.Multiple(() =>
        {
            var itemVariation = toEditorValue.Expose[0];
            Assert.AreEqual(contentElementKey, itemVariation.ContentKey);
            Assert.AreEqual(null, itemVariation.Culture);
        });
    }

    [Test]
    public async Task ToEditor_Orders_Block_Values_And_Expose_Deterministically()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("myElementType", "My Element Type");
        elementType.IsElement = true;
        ContentTypeService.Save(elementType);

        var blockListContentType = await CreateBlockListContentType(elementType);

        // keys and aliases are stored in reverse of their ordinal order
        var firstContentKey = new Guid("00000000-0000-0000-0000-000000000002");
        var secondContentKey = new Guid("00000000-0000-0000-0000-000000000001");
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[]
                    {
                        new BlockListLayoutItem { ContentKey = firstContentKey },
                        new BlockListLayoutItem { ContentKey = secondContentKey },
                    }
                }
            },
            ContentData =
            [
                new()
                {
                    Key = firstContentKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new() { Alias = "singleLineText", Value = "The single line text" },
                        new() { Alias = "multilineText", Value = "The multiline text" },
                    ]
                },
                new()
                {
                    Key = secondContentKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values = [new() { Alias = "singleLineText", Value = "The other single line text" }]
                }
            ],
            Expose =
            [
                new(firstContentKey, null, null),
                new(secondContentKey, null, null),
            ]
        };

        var content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { blocks = JsonSerializer.Serialize(blockListValue) })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(blockListContentType);
        var toEditorValue = valueEditor.ToEditor(content.Properties["blocks"]!) as BlockListValue;
        Assert.IsNotNull(toEditorValue);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(
                new[] { "multilineText", "singleLineText" },
                toEditorValue.ContentData.First(c => c.Key == firstContentKey).Values.Select(v => v.Alias));
            CollectionAssert.AreEqual(
                new[] { secondContentKey, firstContentKey },
                toEditorValue.Expose.Select(e => e.ContentKey));
            CollectionAssert.AreEqual(
                new[] { firstContentKey, secondContentKey },
                toEditorValue.ContentData.Select(c => c.Key),
                "Block order is defined by the layout and must not be changed.");
        });
    }

    [Test]
    public async Task ToEditor_Orders_Nested_Block_Values_And_Expose_Deterministically()
    {
        var nestedElementType = ContentTypeBuilder.CreateAllTypesContentType("myNestedElementType", "My Nested Element Type");
        nestedElementType.IsElement = true;
        ContentTypeService.Save(nestedElementType);
        var nestedBlockListDataType = await CreateBlockListDataType(nestedElementType);

        var rootElementType = new ContentTypeBuilder()
            .WithAlias("myRootElementType")
            .WithName("My Root Element Type")
            .WithIsElement(true)
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .Done()
            .AddPropertyType()
                .WithAlias("nestedBlocks")
                .WithName("Nested blocks")
                .WithDataTypeId(nestedBlockListDataType.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                .WithValueStorageType(ValueStorageType.Ntext)
                .Done()
            .Build();
        ContentTypeService.Save(rootElementType);

        var blockListContentType = await CreateBlockListContentType(rootElementType);

        // keys and aliases are stored in reverse of their ordinal order, at both levels
        var firstNestedContentKey = new Guid("00000000-0000-0000-0000-000000000002");
        var secondNestedContentKey = new Guid("00000000-0000-0000-0000-000000000001");
        var nestedBlockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[]
                    {
                        new BlockListLayoutItem { ContentKey = firstNestedContentKey },
                        new BlockListLayoutItem { ContentKey = secondNestedContentKey },
                    }
                }
            },
            ContentData =
            [
                new()
                {
                    Key = firstNestedContentKey,
                    ContentTypeAlias = nestedElementType.Alias,
                    ContentTypeKey = nestedElementType.Key,
                    Values =
                    [
                        new() { Alias = "singleLineText", Value = "The nested single line text" },
                        new() { Alias = "multilineText", Value = "The nested multiline text" },
                    ]
                },
                new()
                {
                    Key = secondNestedContentKey,
                    ContentTypeAlias = nestedElementType.Alias,
                    ContentTypeKey = nestedElementType.Key,
                    Values = [new() { Alias = "singleLineText", Value = "The other nested single line text" }]
                }
            ],
            Expose =
            [
                new(firstNestedContentKey, null, null),
                new(secondNestedContentKey, null, null),
            ]
        };

        var rootContentKey = Guid.NewGuid();
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[] { new BlockListLayoutItem { ContentKey = rootContentKey } }
                }
            },
            ContentData =
            [
                new()
                {
                    Key = rootContentKey,
                    ContentTypeAlias = rootElementType.Alias,
                    ContentTypeKey = rootElementType.Key,
                    Values =
                    [
                        new() { Alias = "title", Value = "The root title" },
                        new() { Alias = "nestedBlocks", Value = nestedBlockListValue },
                    ]
                }
            ],
            Expose = [new(rootContentKey, null, null)]
        };

        var content = new ContentBuilder()
            .WithContentType(blockListContentType)
            .WithName("My Nested Blocks")
            .WithPropertyValues(new { blocks = JsonSerializer.Serialize(blockListValue) })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(blockListContentType);
        var toEditorValue = valueEditor.ToEditor(content.Properties["blocks"]!) as BlockListValue;
        Assert.IsNotNull(toEditorValue);

        IList<BlockPropertyValue> rootValues = toEditorValue.ContentData.Single().Values;
        var nestedToEditorValue = rootValues.Single(v => v.Alias == "nestedBlocks").Value as BlockListValue;
        Assert.IsNotNull(nestedToEditorValue);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { "nestedBlocks", "title" }, rootValues.Select(v => v.Alias));
            CollectionAssert.AreEqual(
                new[] { "multilineText", "singleLineText" },
                nestedToEditorValue.ContentData.First(c => c.Key == firstNestedContentKey).Values.Select(v => v.Alias));
            CollectionAssert.AreEqual(
                new[] { secondNestedContentKey, firstNestedContentKey },
                nestedToEditorValue.Expose.Select(e => e.ContentKey));
            CollectionAssert.AreEqual(
                new[] { firstNestedContentKey, secondNestedContentKey },
                nestedToEditorValue.ContentData.Select(c => c.Key),
                "Block order is defined by the layout and must not be changed.");
        });
    }

    private async Task<IContentType> CreateBlockListContentType(IContentType elementType)
    {
        var blockListDataType = await CreateBlockListDataType(elementType);

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

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
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
            Name = $"My Block List ({elementType.Alias})",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);
        return blockListDataType;
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
