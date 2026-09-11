// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class VariationContextSegmentAccessorTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private readonly ContextTrackingVariationContextAccessor _contextTrackingVariationContextAccessor = new();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.Services.AddUnique<IVariationContextAccessor>(_ => _contextTrackingVariationContextAccessor);
    }

    [Test]
    public async Task Can_Track_Content_Id_For_Simple_Property()
    {
        var contentType = (await SetupContentTypes()).ContentType;
        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [new() { Alias = "documentTitle", Value = "Document Title" }],
                Variants = [new() { Name = "Page" }],
            },
            [],
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.That(content, Is.Not.Null);

        var titleValue = content.Value<string>("documentTitle");
        Assert.That(titleValue, Is.EqualTo("Document Title"));

        // the title property tracks at content level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));
    }

    [Test]
    public async Task Can_Track_Content_Id_For_Local_Block()
    {
        var contentElementKey = Guid.NewGuid();
        var settingsElementKey = Guid.NewGuid();
        var (contentType, elementType) = await SetupContentTypes();
        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new()
                    {
                        Alias = "blockList",
                        Value = JsonSerializer.Serialize(
                            new BlockListValue
                            {
                                Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                                {
                                    {
                                        Constants.PropertyEditors.Aliases.BlockList,
                                        [
                                            new BlockListLayoutItem
                                            {
                                                ContentKey = contentElementKey,
                                                SettingsKey = settingsElementKey,
                                            }
                                        ]
                                    },
                                },
                                ContentData = [
                                    new BlockItemData
                                    {
                                        Key = contentElementKey,
                                        ContentTypeAlias = elementType.Alias,
                                        ContentTypeKey = elementType.Key,
                                        Values =
                                        [
                                            new BlockPropertyValue
                                            {
                                                Alias = "elementTitle",
                                                Value = "Local Element Content Title",
                                            }
                                        ],
                                    },
                                ],
                                SettingsData = [
                                    new BlockItemData
                                    {
                                        Key = settingsElementKey,
                                        ContentTypeAlias = elementType.Alias,
                                        ContentTypeKey = elementType.Key,
                                        Values =
                                        [
                                            new BlockPropertyValue
                                            {
                                                Alias = "elementTitle",
                                                Value = "Local Element Settings Title",
                                            }
                                        ],
                                    },
                                ],
                                Expose = [new BlockItemVariation(contentElementKey, null, null)],
                            }),
                    }
                ],
                Variants = [new() { Name = "Page" }],
            },
            [],
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.That(content, Is.Not.Null);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.That(blockListValue, Is.Not.Null);
        Assert.That(blockListValue, Has.Count.EqualTo(1));

        // the block list property itself tracks at content level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var block = blockListValue.First();

        var contentTitleValue = block.Content.Value<string>("elementTitle");
        Assert.That(contentTitleValue, Is.EqualTo("Local Element Content Title"));

        // the block content property tracks at content level because it's a locally sourced element
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        Assert.That(block.Settings, Is.Not.Null);
        var settingsTitleValue = block.Settings.Value<string>("elementTitle");
        Assert.That(settingsTitleValue, Is.EqualTo("Local Element Settings Title"));

        // the block settings property tracks at content level because it's a locally sourced element
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        // both of the block elements should have the content as their identity root
        Assert.That(block.Content.OwningContentId, Is.EqualTo(content.Id));
        Assert.That(block.Settings.OwningContentId, Is.EqualTo(content.Id));
    }

    [Test]
    public async Task Can_Track_Content_Id_For_Nested_Local_Block()
    {
        var contentElementKey = Guid.NewGuid();
        var nestedContentElementKey = Guid.NewGuid();
        var (contentType, elementType) = await SetupContentTypes();
        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new()
                    {
                        Alias = "blockList",
                        Value = JsonSerializer.Serialize(
                            new BlockListValue
                            {
                                Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                                {
                                    {
                                        Constants.PropertyEditors.Aliases.BlockList,
                                        [new BlockListLayoutItem { ContentKey = contentElementKey }]
                                    },
                                },
                                ContentData = [
                                    new BlockItemData
                                    {
                                        Key = contentElementKey,
                                        ContentTypeAlias = elementType.Alias,
                                        ContentTypeKey = elementType.Key,
                                        Values =
                                        [
                                            new BlockPropertyValue
                                            {
                                                Alias = "elementBlockList",
                                                Value = JsonSerializer.Serialize(
                                                    new BlockListValue
                                                    {
                                                        Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                                                        {
                                                            {
                                                                Constants.PropertyEditors.Aliases.BlockList,
                                                                [new BlockListLayoutItem { ContentKey = nestedContentElementKey }]
                                                            },
                                                        },
                                                        ContentData = [
                                                            new BlockItemData
                                                            {
                                                                Key = nestedContentElementKey,
                                                                ContentTypeAlias = elementType.Alias,
                                                                ContentTypeKey = elementType.Key,
                                                                Values =
                                                                [
                                                                    new BlockPropertyValue
                                                                    {
                                                                        Alias = "elementTitle",
                                                                        Value = "Nested Local Element Content Title",
                                                                    }
                                                                ],
                                                            },
                                                        ],
                                                        SettingsData = [],
                                                        Expose = [new BlockItemVariation(nestedContentElementKey, null, null)],
                                                    }),
                                            }
                                        ],
                                    },
                                ],
                                SettingsData = [],
                                Expose = [new BlockItemVariation(contentElementKey, null, null)],
                            }),
                    }
                ],
                Variants = [new() { Name = "Page" }],
            },
            [],
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.That(content, Is.Not.Null);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.That(blockListValue, Is.Not.Null);
        Assert.That(blockListValue, Has.Count.EqualTo(1));

        // the block list property itself tracks at content level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var block = blockListValue.First();

        var nestedBlockListValue = block.Content.Value<BlockListModel>("elementBlockList");
        Assert.That(nestedBlockListValue, Is.Not.Null);
        Assert.That(nestedBlockListValue, Has.Count.EqualTo(1));

        // the block content property tracks at content level because it's a locally sourced element
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var nestedBlock = nestedBlockListValue.First();

        var contentTitleValue = nestedBlock.Content.Value<string>("elementTitle");
        Assert.That(contentTitleValue, Is.EqualTo("Nested Local Element Content Title"));

        // the nested block content property also tracks at content level because it's a locally sourced element
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        // the elements of both the block and the nested block should have the content as their identity root
        Assert.That(block.Content.OwningContentId, Is.EqualTo(content.Id));
        Assert.That(nestedBlock.Content.OwningContentId, Is.EqualTo(content.Id));
    }

    [Test]
    public async Task Can_Track_Content_Id_For_Reusable_Block()
    {
        var (contentType, elementType) = await SetupContentTypes();
        var reusableElement = await CreateAndPublishReusableElement(elementType);
        var settingsElementKey = Guid.NewGuid();

        var contentCreateResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new()
                    {
                        Alias = "blockList",
                        Value = JsonSerializer.Serialize(
                            new BlockListValue
                            {
                                Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                                {
                                    {
                                        Constants.PropertyEditors.Aliases.BlockList,
                                        [
                                            new BlockListLayoutItem
                                            {
                                                ContentKey = reusableElement.Key,
                                                SettingsKey = settingsElementKey,
                                                IsExternalContent = true,
                                            },
                                        ]
                                    },
                                },
                                ContentData = [],
                                SettingsData = [
                                    new BlockItemData
                                    {
                                        Key = settingsElementKey,
                                        ContentTypeAlias = elementType.Alias,
                                        ContentTypeKey = elementType.Key,
                                        Values =
                                        [
                                            new BlockPropertyValue
                                            {
                                                Alias = "elementTitle",
                                                Value = "Local Element Settings Title",
                                            }
                                        ],
                                    },
                                ],
                                Expose = [new BlockItemVariation(reusableElement.Key, null, null)],
                            }),
                    }
                ],
                Variants = [new() { Name = "Page" }],
            },
            [],
            Constants.Security.SuperUserKey);

        Assert.That(contentCreateResult.Success, Is.True);

        var content = await DocumentCacheService.GetByKeyAsync(contentCreateResult.Result.Content!.Key);
        Assert.That(content, Is.Not.Null);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.That(blockListValue, Is.Not.Null);
        Assert.That(blockListValue, Has.Count.EqualTo(1));

        // the block list property itself tracks at content level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var block = blockListValue.First();

        var contentTitleValue = block.Content.Value<string>("elementTitle");
        Assert.That(contentTitleValue, Is.EqualTo("Reusable Element Title"));

        // the block content property tracks at element level because it's a reusable element
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(reusableElement.Id));

        Assert.That(block.Settings, Is.Not.Null);
        var settingsTitleValue = block.Settings.Value<string>("elementTitle");
        Assert.That(settingsTitleValue, Is.EqualTo("Local Element Settings Title"));

        // the block settings property tracks at content level because it's a locally sourced element
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        // the content element (reusable element) should not track an identity root,
        // but the settings element (locally sourced) should still have the content as its identity root
        Assert.That(block.Content.OwningContentId, Is.Null);
        Assert.That(block.Settings.OwningContentId, Is.EqualTo(content.Id));
    }

    [Test]
    public async Task Can_Track_Content_Id_For_Local_Block_In_Reusable_Element()
    {
        var innerContentElementKey = Guid.NewGuid();
        var (contentType, elementType) = await SetupContentTypes();
        var reusableElement = await CreateAndPublishReusableElement(elementType, innerContentElementKey);

        var contentCreateResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new() { Alias = "documentTitle", Value = "Document Title" },
                    new()
                    {
                        Alias = "blockList",
                        Value = JsonSerializer.Serialize(
                            new BlockListValue
                            {
                                Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                                {
                                    {
                                        Constants.PropertyEditors.Aliases.BlockList,
                                        [
                                            new BlockListLayoutItem
                                            {
                                                ContentKey = reusableElement.Key,
                                                IsExternalContent = true,
                                            },
                                        ]
                                    },
                                },
                                ContentData = [],
                                SettingsData = [],
                                Expose = [new BlockItemVariation(reusableElement.Key, null, null)],
                            }),
                    }
                ],
                Variants = [new() { Name = "Page" }],
            },
            [],
            Constants.Security.SuperUserKey);

        Assert.That(contentCreateResult.Success, Is.True);

        var content = await DocumentCacheService.GetByKeyAsync(contentCreateResult.Result.Content!.Key);
        Assert.That(content, Is.Not.Null);

        // the assertions below distinguish the reusable element from the document, so the two must differ
        Assert.That(reusableElement.Id, Is.Not.EqualTo(content.Id));

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.That(blockListValue, Is.Not.Null);
        Assert.That(blockListValue, Has.Count.EqualTo(1));

        var block = blockListValue.First();

        // the reusable element bears its own identity, so it has no owning content
        Assert.That(block.Content.OwningContentId, Is.Null);

        var innerBlockListValue = block.Content.Value<BlockListModel>("elementBlockList");
        Assert.That(innerBlockListValue, Is.Not.Null);
        Assert.That(innerBlockListValue, Has.Count.EqualTo(1));

        var innerBlock = innerBlockListValue.First();

        // read a document level property first, so the tracking assertion below cannot pass on a value
        // left over from reading the reusable element
        content.Value<string>("documentTitle");
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var innerTitleValue = innerBlock.Content.Value<string>("elementTitle");
        Assert.That(innerTitleValue, Is.EqualTo("Local Element Content Title In Reusable Element"));

        // the locally sourced block inside the reusable element is owned by the reusable element,
        // not by the document that renders it
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(reusableElement.Id));
        Assert.That(innerBlock.Content.OwningContentId, Is.EqualTo(reusableElement.Id));
        Assert.That(innerBlock.Content.OwningContentId, Is.Not.EqualTo(content.Id));
    }

    [Test]
    public async Task Can_Track_Content_Id_For_Picked_Reusable_Element()
    {
        var (contentType, elementType) = await SetupContentTypes();
        var reusableElement = await CreateAndPublishReusableElement(elementType);

        var contentCreateResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new()
                    {
                        Alias = "elementPicker",
                        Value = JsonSerializer.Serialize(new[] { reusableElement.Key }),
                    }
                ],
                Variants = [new() { Name = "Page" }],
            },
            [],
            Constants.Security.SuperUserKey);

        Assert.That(contentCreateResult.Success, Is.True);

        var content = await DocumentCacheService.GetByKeyAsync(contentCreateResult.Result.Content!.Key);
        Assert.That(content, Is.Not.Null);

        var elementPickerValue = content.Value<IEnumerable<IPublishedElement>>("elementPicker");
        Assert.That(elementPickerValue, Is.Not.Null);

        var publishedElements = elementPickerValue as IPublishedElement[] ?? elementPickerValue.ToArray();
        Assert.That(publishedElements.Count(), Is.EqualTo(1));

        var publishedElement = publishedElements.First();
        Assert.That(publishedElement.Key, Is.EqualTo(reusableElement.Key));

        // the picker property itself tracks at content level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var elementTitleValue = publishedElement.Value<string>("elementTitle");
        Assert.That(elementTitleValue, Is.EqualTo("Reusable Element Title"));

        // the element title property tracks at element level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(reusableElement.Id));

        // the element (reusable element) should not track an identity root,
        // and the settings element should have the content as its identity root
        Assert.That(publishedElement.OwningContentId, Is.Null);
    }

    [Test]
    public async Task Can_Track_Content_Id_For_Nested_Picked_Reusable_Element()
    {
        var (contentType, elementType) = await SetupContentTypes();
        var reusableElement = await CreateAndPublishReusableElement(elementType);

        var contentElementKey = Guid.NewGuid();

        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                Properties = [
                    new()
                    {
                        Alias = "blockList",
                        Value = JsonSerializer.Serialize(
                            new BlockListValue
                            {
                                Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                                {
                                    {
                                        Constants.PropertyEditors.Aliases.BlockList,
                                        [new BlockListLayoutItem { ContentKey = contentElementKey }]
                                    },
                                },
                                ContentData = [
                                    new BlockItemData
                                    {
                                        Key = contentElementKey,
                                        ContentTypeAlias = elementType.Alias,
                                        ContentTypeKey = elementType.Key,
                                        Values =
                                        [
                                            new BlockPropertyValue
                                            {
                                                Alias = "elementElementPicker",
                                                Value = JsonSerializer.Serialize(new[] { reusableElement.Key }),
                                            }
                                        ],
                                    },
                                ],
                                SettingsData = [],
                                Expose = [new BlockItemVariation(contentElementKey, null, null)],
                            }),
                    }
                ],
                Variants = [new() { Name = "Page" }],
            },
            [],
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.That(content, Is.Not.Null);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.That(blockListValue, Is.Not.Null);
        Assert.That(blockListValue, Has.Count.EqualTo(1));

        // the block list property itself tracks at content level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var block = blockListValue.First();

        var nestedElementPickerValue = block.Content.Value<IEnumerable<IPublishedElement>>("elementElementPicker");
        Assert.That(nestedElementPickerValue, Is.Not.Null);

        var publishedElements = nestedElementPickerValue as IPublishedElement[] ?? nestedElementPickerValue.ToArray();
        Assert.That(publishedElements.Count(), Is.EqualTo(1));

        var publishedElement = publishedElements.First();
        Assert.That(publishedElement.Key, Is.EqualTo(reusableElement.Key));

        // the nested picker property itself tracks at content level, because it's inside a local block
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(content.Id));

        var nestedElementTitleValue = publishedElement.Value<string>("elementTitle");
        Assert.That(nestedElementTitleValue, Is.EqualTo("Reusable Element Title"));

        // the element title property tracks at element level
        Assert.That(_contextTrackingVariationContextAccessor.LastTrackedContentId, Is.EqualTo(reusableElement.Id));

        // the block content element (locally sourced) should have the content as its identity root,
        // and the nested picked element (reusable) should not track any identity root
        Assert.That(block.Content.OwningContentId, Is.EqualTo(content.Id));
        Assert.That(publishedElement.OwningContentId, Is.Null);
    }

    private async Task<(IContentType ContentType, IContentType ElementType)> SetupContentTypes()
    {
        var blockListDataType = new DataType(
            PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList],
            ConfigurationEditorJsonSerializer)
        {
            Name = "Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        var elementPickerDataType = new DataType(
            PropertyEditorCollection[Constants.PropertyEditors.Aliases.ElementPicker],
            ConfigurationEditorJsonSerializer)
        {
            Name = "Element Picker",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        await DataTypeService.CreateAsync(elementPickerDataType, Constants.Security.SuperUserKey);

        var elementType = new ContentTypeBuilder()
            .WithAlias("elementType")
            .WithName("Element Type")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Segment)
            .AddPropertyType()
            .WithAlias("elementTitle")
            .WithName("Element Title")
            .WithVariations(ContentVariation.Segment)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .Done()
            .AddPropertyType()
            .WithAlias("elementBlockList")
            .WithName("Element Block List")
            .WithDataTypeId(blockListDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
            .Done()
            .AddPropertyType()
            .WithAlias("elementElementPicker")
            .WithName("Element Element Picker")
            .WithDataTypeId(elementPickerDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ElementPicker)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("documentType")
            .WithName("Document Type")
            .WithAllowAsRoot(true)
            .WithContentVariation(ContentVariation.Segment)
            .AddPropertyType()
            .WithAlias("documentTitle")
            .WithName("Document Title")
            .WithVariations(ContentVariation.Segment)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .Done()
            .AddPropertyType()
            .WithAlias("blockList")
            .WithName("Block List")
            .WithDataTypeId(blockListDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
            .Done()
            .AddPropertyType()
            .WithAlias("elementPicker")
            .WithName("Element Picker")
            .WithDataTypeId(elementPickerDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ElementPicker)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        blockListDataType.ConfigurationData = new Dictionary<string, object>
        {
            {
                "blocks",
                new BlockListConfiguration.BlockConfiguration[]
                {
                    new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key, },
                }
            },
        };

        await DataTypeService.UpdateAsync(blockListDataType, Constants.Security.SuperUserKey);

        return (contentType, elementType);
    }

    private async Task<IElement> CreateAndPublishReusableElement(IContentType elementType, Guid? innerBlockKey = null)
    {
        var properties = new List<PropertyValueModel>
        {
            new() { Alias = "elementTitle", Value = "Reusable Element Title" },
        };

        if (innerBlockKey.HasValue)
        {
            properties.Add(new PropertyValueModel
            {
                Alias = "elementBlockList",
                Value = JsonSerializer.Serialize(
                    new BlockListValue
                    {
                        Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                        {
                            {
                                Constants.PropertyEditors.Aliases.BlockList,
                                [new BlockListLayoutItem { ContentKey = innerBlockKey.Value }]
                            },
                        },
                        ContentData = [
                            new BlockItemData
                            {
                                Key = innerBlockKey.Value,
                                ContentTypeAlias = elementType.Alias,
                                ContentTypeKey = elementType.Key,
                                Values =
                                [
                                    new BlockPropertyValue
                                    {
                                        Alias = "elementTitle",
                                        Value = "Local Element Content Title In Reusable Element",
                                    }
                                ],
                            },
                        ],
                        SettingsData = [],
                        Expose = [new BlockItemVariation(innerBlockKey.Value, null, null)],
                    }),
            });
        }

        var elementCreateResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementType.Key,
                ParentKey = null,
                Properties = properties,
                Variants = [new VariantModel { Name = "Reusable element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(elementCreateResult.Success, Is.True);

        var reusableElement = elementCreateResult.Result.Content!;
        Assert.That(reusableElement.Id, Is.Not.EqualTo(0));

        var elementPublishResult = await ElementPublishingService.PublishAsync(
            reusableElement.Key,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.That(elementPublishResult.Success, Is.True);

        return reusableElement;
    }

    private class ContextTrackingVariationContextAccessor : IVariationContextAccessor
    {
        public VariationContext VariationContext { get; set; } = new ContextTrackingVariationContext();

        public int LastTrackedContentId => ((ContextTrackingVariationContext)VariationContext).LastTrackedContentId;
    }

    private class ContextTrackingVariationContext : VariationContext
    {
        public override string GetSegment(int contentId, string propertyAlias)
        {
            LastTrackedContentId = contentId;
            return base.GetSegment(contentId, propertyAlias);
        }

        public int LastTrackedContentId { get; private set; }
    }
}
