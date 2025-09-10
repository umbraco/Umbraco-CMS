using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class SingleBlockPropertyEditorTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentValidationService ContentValidationService => GetRequiredService<IContentValidationService>();

    private IContentEditingModelFactory ContentEditingModelFactory => GetRequiredService<IContentEditingModelFactory>();

    private ILocalizedTextService LocalizedTextService => GetRequiredService<ILocalizedTextService>();

    private const string AllTypes = "allTypes";
    private const string MetaType = "metaType";
    private const string TextType = "textType";

    [Theory]
    [TestCase(AllTypes)]
    [TestCase(MetaType)]
    public async Task Can_Select_Different_Configured_Block(string elementTypeName)
    {
        if (elementTypeName != AllTypes && elementTypeName != MetaType)
        {
            throw new ArgumentOutOfRangeException(nameof(elementTypeName));
        }

        var textPageContentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        textPageContentType.AllowedTemplates = [];
        await ContentTypeService.CreateAsync(textPageContentType, Constants.Security.SuperUserKey);

        var textPage = ContentBuilder.CreateTextpageContent(textPageContentType, "My Picked Content", -1);
        ContentService.Save(textPage);

        var allTypesType = ContentTypeBuilder.CreateAllTypesContentType("allTypesType", "All Types type");
        allTypesType.IsElement = true;
        await ContentTypeService.CreateAsync(allTypesType, Constants.Security.SuperUserKey);

        var metaTypesType = ContentTypeBuilder.CreateMetaContentType();
        metaTypesType.IsElement = true;
        await ContentTypeService.CreateAsync(metaTypesType, Constants.Security.SuperUserKey);

        var singleBlockContentType = await CreateSingleBlockContentTypePage([allTypesType, metaTypesType]);

        var contentElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = contentElementKey }
                    ]
                },
            },
            ContentData =
            [
                elementTypeName == AllTypes
                ? new BlockItemData
                {
                    Key = contentElementKey,
                    ContentTypeAlias = allTypesType.Alias,
                    ContentTypeKey = allTypesType.Key,
                    Values =
                    [
                        new BlockPropertyValue
                        {
                            Alias = "contentPicker",
                            Value = textPage.GetUdi(),
                        }
                    ],
                }
                : new BlockItemData
                {
                    Key = contentElementKey,
                    ContentTypeAlias = metaTypesType.Alias,
                    ContentTypeKey = metaTypesType.Key,
                    Values =
                    [
                        new BlockPropertyValue
                        {
                            Alias = "metadescription",
                            Value = "something very meta",
                        }
                    ],
                }
            ],
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { block = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(singleBlockContentType);
        var toEditorValue = valueEditor.ToEditor(content.Properties["block"]!) as SingleBlockValue;
        Assert.IsNotNull(toEditorValue);
        Assert.AreEqual(1, toEditorValue.ContentData.Count);

        var properties = toEditorValue.ContentData.First().Values;
        Assert.AreEqual(1, properties.Count);
        Assert.Multiple(() =>
        {
            var property = properties.First();
            Assert.AreEqual(elementTypeName == AllTypes ? "contentPicker" : "metadescription", property.Alias);
            Assert.AreEqual(elementTypeName == AllTypes ? textPage.Key : "something very meta", property.Value);
        });

        // convert to updateModel and run validation
        var updateModel = await ContentEditingModelFactory.CreateFromAsync(content);
        var validationResult = await ContentValidationService.ValidatePropertiesAsync(updateModel, singleBlockContentType);

        Assert.AreEqual(0, validationResult.ValidationErrors.Count());
    }

    [Theory]
    [TestCase(AllTypes, true)]
    [TestCase(MetaType, true)]
    [TestCase(TextType, false)]
    [Ignore("Reenable when configured block validation is introduced")]
    public async Task Validates_Configured_Blocks(string elementTypeName, bool shouldPass)
    {
        if (elementTypeName != AllTypes && elementTypeName != MetaType && elementTypeName != TextType)
        {
            throw new ArgumentOutOfRangeException(nameof(elementTypeName));
        }

        var textPageContentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        textPageContentType.AllowedTemplates = [];
        await ContentTypeService.CreateAsync(textPageContentType, Constants.Security.SuperUserKey);

        var textPage = ContentBuilder.CreateTextpageContent(textPageContentType, "My Picked Content", -1);
        ContentService.Save(textPage);

        var allTypesType = ContentTypeBuilder.CreateAllTypesContentType("allTypesType", "All Types type");
        allTypesType.IsElement = true;
        await ContentTypeService.CreateAsync(allTypesType, Constants.Security.SuperUserKey);

        var metaTypesType = ContentTypeBuilder.CreateMetaContentType();
        metaTypesType.IsElement = true;
        await ContentTypeService.CreateAsync(metaTypesType, Constants.Security.SuperUserKey);

        var textType = new ContentTypeBuilder()
            .WithAlias("TextType")
            .WithName("Text type")
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSortOrder(1)
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithSortOrder(1)
            .Done()
            .Done()
            .Build();
        textType.IsElement = true;
        await ContentTypeService.CreateAsync(textType, Constants.Security.SuperUserKey);

        // do not allow textType to be a valid block
        var singleBlockContentType = await CreateSingleBlockContentTypePage([allTypesType, metaTypesType]);

        var contentElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = contentElementKey }
                    ]
                },
            },
            ContentData =
            [
                elementTypeName == AllTypes
                ? new BlockItemData
                {
                    Key = contentElementKey,
                    ContentTypeAlias = allTypesType.Alias,
                    ContentTypeKey = allTypesType.Key,
                    Values =
                    [
                        new BlockPropertyValue
                        {
                            Alias = "contentPicker",
                            Value = textPage.GetUdi(),
                        }
                    ],
                }
                : elementTypeName == MetaType ?
                    new BlockItemData
                    {
                        Key = contentElementKey,
                        ContentTypeAlias = metaTypesType.Alias,
                        ContentTypeKey = metaTypesType.Key,
                        Values =
                        [
                            new BlockPropertyValue
                            {
                                Alias = "metadescription",
                                Value = "something very meta",
                            }
                        ],
                    }
                    : new BlockItemData
                    {
                        Key = contentElementKey,
                        ContentTypeAlias = textType.Alias,
                        ContentTypeKey = textType.Key,
                        Values =
                        [
                            new BlockPropertyValue
                            {
                                Alias = "title",
                                Value = "a random title",
                            }
                        ],
                    },
            ],
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { block = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        // convert to updateModel and run validation
        var updateModel = await ContentEditingModelFactory.CreateFromAsync(content);
        var validationResult = await ContentValidationService.ValidatePropertiesAsync(updateModel, singleBlockContentType);

        Assert.AreEqual(shouldPass ? 0 : 1, validationResult.ValidationErrors.Count());
    }

    /// <summary>
    /// There should be some validation when publishing through the contentEditingService
    /// </summary>
    [Test]
    public async Task Cannot_Select_Multiple_Configured_Blocks()
    {
        var textPageContentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        textPageContentType.AllowedTemplates = [];
        await ContentTypeService.CreateAsync(textPageContentType, Constants.Security.SuperUserKey);

        var textPage = ContentBuilder.CreateTextpageContent(textPageContentType, "My Picked Content", -1);
        ContentService.Save(textPage);

        var allTypesType = ContentTypeBuilder.CreateAllTypesContentType("allTypesType", "All Types type");
        allTypesType.IsElement = true;
        await ContentTypeService.CreateAsync(allTypesType, Constants.Security.SuperUserKey);

        var metaTypesType = ContentTypeBuilder.CreateMetaContentType();
        metaTypesType.IsElement = true;
        await ContentTypeService.CreateAsync(metaTypesType, Constants.Security.SuperUserKey);

        var singleBlockContentType = await CreateSingleBlockContentTypePage([allTypesType, metaTypesType]);

        var firstElementKey = Guid.NewGuid();
        var secondElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = firstElementKey },
                        new SingleBlockLayoutItem { ContentKey = secondElementKey }
                    ]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = firstElementKey,
                    ContentTypeAlias = allTypesType.Alias,
                    ContentTypeKey = allTypesType.Key,
                    Values =
                    [
                        new BlockPropertyValue
                        {
                            Alias = "contentPicker",
                            Value = textPage.GetUdi(),
                        }
                    ],
                },
                new BlockItemData
                {
                    Key = secondElementKey,
                    ContentTypeAlias = metaTypesType.Alias,
                    ContentTypeKey = metaTypesType.Key,
                    Values =
                    [
                        new BlockPropertyValue
                        {
                            Alias = "metadescription",
                            Value = "something very meta",
                        }
                    ],
                }
            ],
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { block = blocksPropertyValue })
            .Build();

        // No validation, should just save
        ContentService.Save(content);

        // convert to updateModel and run validation
        var updateModel = await ContentEditingModelFactory.CreateFromAsync(content);
        var validationResult = await ContentValidationService.ValidatePropertiesAsync(updateModel, singleBlockContentType);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, validationResult.ValidationErrors.Count());
            var validationError = validationResult.ValidationErrors.Single();
            var expectedErrorMessage = SingleBlockPropertyEditor.SingleBlockEditorPropertyValueEditor
                .SingleBlockValidator
                .BuildErrorMessage(LocalizedTextService, 1, 2);
            Assert.AreEqual("block", validationError.Alias);
            Assert.AreEqual(expectedErrorMessage, validationError.ErrorMessages.Single());
        });
    }

    [Test]
    public async Task Can_Track_References()
    {
        var textPageContentType = ContentTypeBuilder.CreateTextPageContentType("myContentType");
        textPageContentType.AllowedTemplates = [];
        await ContentTypeService.CreateAsync(textPageContentType, Constants.Security.SuperUserKey);

        var textPage = ContentBuilder.CreateTextpageContent(textPageContentType, "My Picked Content", -1);
        ContentService.Save(textPage);

        var elementType = ContentTypeBuilder.CreateAllTypesContentType("allTypesType", "All Types type");
        elementType.IsElement = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var singleBlockContentType = await CreateSingleBlockContentTypePage([elementType]);

        var contentElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = contentElementKey }
                    ]
                },
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
                        new BlockPropertyValue
                        {
                            Alias = "contentPicker",
                            Value = textPage.GetUdi(),
                        }
                    ],
                }
            ],
        };
        var blocksPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { block = blocksPropertyValue })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(singleBlockContentType);

        var references = valueEditor.GetReferences(content.GetValue("block")).ToArray();
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
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var singleBlockContentType = await CreateSingleBlockContentTypePage([elementType]);

        var contentElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = contentElementKey }
                    ]
                },
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
                            Value = JsonSerializer.Serialize(new[] { "Tag One", "Tag Two", "Tag Three" }),
                        }
                    ],
                }
            ],
        };
        var blockPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { block = blockPropertyValue })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(singleBlockContentType);

        var tags = valueEditor.GetTags(content.GetValue("block"), null, null).ToArray();
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
        await ContentTypeService.CreateAsync(elementType,  Constants.Security.SuperUserKey);

        var singleBlockContentType = await CreateSingleBlockContentTypePage([elementType]);
        singleBlockContentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(singleBlockContentType,  Constants.Security.SuperUserKey);

        var contentElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = contentElementKey }
                    ]
                },
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
                        new()
                        {
                            Alias = "tags",
                            // this is a little skewed, but the tags editor expects a serialized array of strings
                            Value = JsonSerializer.Serialize(new[] { "Tag One EN", "Tag Two EN", "Tag Three EN" }),
                            Culture = "en-US",
                        },
                        new()
                        {
                            Alias = "tags",
                            // this is a little skewed, but the tags editor expects a serialized array of strings
                            Value = JsonSerializer.Serialize(new[] { "Tag One DA", "Tag Two DA", "Tag Three DA" }),
                            Culture = "da-DK",
                        }
                    ],
                }
            ],
        };
        var blockPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithCultureName("en-US", "My Blocks EN")
            .WithCultureName("da-DK", "My Blocks DA")
            .WithPropertyValues(new { block = blockPropertyValue })
            .Build();
        ContentService.Save(content);

        var valueEditor = await GetValueEditor(singleBlockContentType);

        var tags = valueEditor.GetTags(content.GetValue("block"), null, null).ToArray();
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
        await ContentTypeService.CreateAsync(elementType,   Constants.Security.SuperUserKey);

        var singleBlockContentType = await CreateSingleBlockContentTypePage([elementType]);

        var contentElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = contentElementKey }
                    ]
                },
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
                        }
                    ],
                }
            ],
            Expose =
            [
                new BlockItemVariation(contentElementKey, null, null)
            ],
        };
        var blockPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { block = blockPropertyValue })
            .Build();
        ContentService.Save(content);

        elementType.Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(pt => pt.Alias == "singleLineText").Variations = ContentVariation.Culture;
        ContentTypeService.Save(elementType);

        var valueEditor = await GetValueEditor(singleBlockContentType);
        var toEditorValue = valueEditor.ToEditor(content.Properties["block"]!) as SingleBlockValue;
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
        await ContentTypeService.CreateAsync(elementType,  Constants.Security.SuperUserKey);

        var singleBlockContentType = await CreateSingleBlockContentTypePage([elementType]);

        var contentElementKey = Guid.NewGuid();
        var blockValue = new SingleBlockValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.SingleBlock,
                    [
                        new SingleBlockLayoutItem { ContentKey = contentElementKey },
                    ]
                },
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
                            Culture = "en-US",
                        }
                    ],
                }
            ],
            Expose =
            [
                new BlockItemVariation(contentElementKey, "en-US", null)
            ],
        };
        var blockPropertyValue = JsonSerializer.Serialize(blockValue);

        var content = new ContentBuilder()
            .WithContentType(singleBlockContentType)
            .WithName("My Blocks")
            .WithPropertyValues(new { block = blockPropertyValue })
            .Build();
        ContentService.Save(content);

        elementType.PropertyTypes.First(pt => pt.Alias == "singleLineText").Variations = ContentVariation.Nothing;
        elementType.Variations = ContentVariation.Nothing;
        await ContentTypeService.SaveAsync(elementType,   Constants.Security.SuperUserKey);

        var valueEditor = await GetValueEditor(singleBlockContentType);
        var toEditorValue = valueEditor.ToEditor(content.Properties["block"]!) as SingleBlockValue;
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

    private async Task<IContentType> CreateSingleBlockContentTypePage(IContentType[] allowedElementTypes)
    {
        var singleBlockDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.SingleBlock], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    allowedElementTypes.Select(allowedElementType =>
                        new BlockListConfiguration.BlockConfiguration
                        {
                            ContentElementTypeKey = allowedElementType.Key,
                        }).ToArray()
                },
            },
            Name = "My Single Block",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(singleBlockDataType, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
            .AddPropertyType()
            .WithAlias("block")
            .WithName("Block")
            .WithDataTypeId(singleBlockDataType.Id)
            .Done()
            .Build();
        ContentTypeService.Save(contentType);
        // re-fetch to wire up all key bindings (particularly to the datatype)
        return await ContentTypeService.GetAsync(contentType.Key);
    }

    private async Task<SingleBlockPropertyEditor.SingleBlockEditorPropertyValueEditor> GetValueEditor(IContentType contentType)
    {
        var dataType = await DataTypeService.GetAsync(contentType.PropertyTypes.First(propertyType => propertyType.Alias == "block").DataTypeKey);
        Assert.IsNotNull(dataType?.Editor);
        var valueEditor = dataType.Editor.GetValueEditor() as SingleBlockPropertyEditor.SingleBlockEditorPropertyValueEditor;
        Assert.IsNotNull(valueEditor);

        return valueEditor;
    }
}
