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

        Assert.IsTrue(createResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var titleValue = content.Value<string>("documentTitle");
        Assert.AreEqual("Document Title", titleValue);

        // the title property tracks at content level
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);
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

        Assert.IsTrue(createResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.IsNotNull(blockListValue);
        Assert.AreEqual(1, blockListValue.Count);

        // the block list property itself tracks at content level
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var block = blockListValue.First();

        var contentTitleValue = block.Content.Value<string>("elementTitle");
        Assert.AreEqual("Local Element Content Title", contentTitleValue);

        // the block content property tracks at content level because it's a locally sourced element
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        Assert.IsNotNull(block.Settings);
        var settingsTitleValue = block.Settings.Value<string>("elementTitle");
        Assert.AreEqual("Local Element Settings Title", settingsTitleValue);

        // the block settings property tracks at content level because it's a locally sourced element
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        // both of the block elements should have the content as their identity root
        Assert.AreEqual(content.Id, block.Content.OwningContentId);
        Assert.AreEqual(content.Id, block.Settings.OwningContentId);
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

        Assert.IsTrue(createResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.IsNotNull(blockListValue);
        Assert.AreEqual(1, blockListValue.Count);

        // the block list property itself tracks at content level
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var block = blockListValue.First();

        var nestedBlockListValue = block.Content.Value<BlockListModel>("elementBlockList");
        Assert.IsNotNull(nestedBlockListValue);
        Assert.AreEqual(1, nestedBlockListValue.Count);

        // the block content property tracks at content level because it's a locally sourced element
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var nestedBlock = nestedBlockListValue.First();

        var contentTitleValue = nestedBlock.Content.Value<string>("elementTitle");
        Assert.AreEqual("Nested Local Element Content Title", contentTitleValue);

        // the nested block content property also tracks at content level because it's a locally sourced element
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        // the elements of both the block and the nested block should have the content as their identity root
        Assert.AreEqual(content.Id, block.Content.OwningContentId);
        Assert.AreEqual(content.Id, nestedBlock.Content.OwningContentId);
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

        Assert.IsTrue(contentCreateResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(contentCreateResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.IsNotNull(blockListValue);
        Assert.AreEqual(1, blockListValue.Count);

        // the block list property itself tracks at content level
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var block = blockListValue.First();

        var contentTitleValue = block.Content.Value<string>("elementTitle");
        Assert.AreEqual("Reusable Element Title", contentTitleValue);

        // the block content property tracks at element level because it's a reusable element
        Assert.AreEqual(reusableElement.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        Assert.IsNotNull(block.Settings);
        var settingsTitleValue = block.Settings.Value<string>("elementTitle");
        Assert.AreEqual("Local Element Settings Title", settingsTitleValue);

        // the block settings property tracks at content level because it's a locally sourced element
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        // the content element (reusable element) should not track an identity root,
        // but the settings element (locally sourced) should still have the content as its identity root
        Assert.IsNull(block.Content.OwningContentId);
        Assert.AreEqual(content.Id, block.Settings.OwningContentId);
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

        Assert.IsTrue(contentCreateResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(contentCreateResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        // the assertions below distinguish the reusable element from the document, so the two must differ
        Assert.AreNotEqual(content.Id, reusableElement.Id);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.IsNotNull(blockListValue);
        Assert.AreEqual(1, blockListValue.Count);

        var block = blockListValue.First();

        // the reusable element bears its own identity, so it has no owning content
        Assert.IsNull(block.Content.OwningContentId);

        var innerBlockListValue = block.Content.Value<BlockListModel>("elementBlockList");
        Assert.IsNotNull(innerBlockListValue);
        Assert.AreEqual(1, innerBlockListValue.Count);

        var innerBlock = innerBlockListValue.First();

        // read a document level property first, so the tracking assertion below cannot pass on a value
        // left over from reading the reusable element
        content.Value<string>("documentTitle");
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var innerTitleValue = innerBlock.Content.Value<string>("elementTitle");
        Assert.AreEqual("Local Element Content Title In Reusable Element", innerTitleValue);

        // the locally sourced block inside the reusable element is owned by the reusable element,
        // not by the document that renders it
        Assert.AreEqual(reusableElement.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);
        Assert.AreEqual(reusableElement.Id, innerBlock.Content.OwningContentId);
        Assert.AreNotEqual(content.Id, innerBlock.Content.OwningContentId);
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

        Assert.IsTrue(contentCreateResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(contentCreateResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var elementPickerValue = content.Value<IEnumerable<IPublishedElement>>("elementPicker");
        Assert.IsNotNull(elementPickerValue);

        var publishedElements = elementPickerValue as IPublishedElement[] ?? elementPickerValue.ToArray();
        Assert.AreEqual(1, publishedElements.Count());

        var publishedElement = publishedElements.First();
        Assert.AreEqual(reusableElement.Key, publishedElement.Key);

        // the picker property itself tracks at content level
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var elementTitleValue = publishedElement.Value<string>("elementTitle");
        Assert.AreEqual("Reusable Element Title", elementTitleValue);

        // the element title property tracks at element level
        Assert.AreEqual(reusableElement.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        // the element (reusable element) should not track an identity root,
        // and the settings element should have the content as its identity root
        Assert.IsNull(publishedElement.OwningContentId);
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

        Assert.IsTrue(createResult.Success);

        var content = await DocumentCacheService.GetByKeyAsync(createResult.Result.Content!.Key);
        Assert.IsNotNull(content);

        var blockListValue = content.Value<BlockListModel>("blockList");
        Assert.IsNotNull(blockListValue);
        Assert.AreEqual(1, blockListValue.Count);

        // the block list property itself tracks at content level
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var block = blockListValue.First();

        var nestedElementPickerValue = block.Content.Value<IEnumerable<IPublishedElement>>("elementElementPicker");
        Assert.IsNotNull(nestedElementPickerValue);

        var publishedElements = nestedElementPickerValue as IPublishedElement[] ?? nestedElementPickerValue.ToArray();
        Assert.AreEqual(1, publishedElements.Count());

        var publishedElement = publishedElements.First();
        Assert.AreEqual(reusableElement.Key, publishedElement.Key);

        // the nested picker property itself tracks at content level, because it's inside a local block
        Assert.AreEqual(content.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        var nestedElementTitleValue = publishedElement.Value<string>("elementTitle");
        Assert.AreEqual("Reusable Element Title", nestedElementTitleValue);

        // the element title property tracks at element level
        Assert.AreEqual(reusableElement.Id, _contextTrackingVariationContextAccessor.LastTrackedContentId);

        // the block content element (locally sourced) should have the content as its identity root,
        // and the nested picked element (reusable) should not track any identity root
        Assert.AreEqual(content.Id, block.Content.OwningContentId);
        Assert.IsNull(publishedElement.OwningContentId);
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
        Assert.IsTrue(elementCreateResult.Success);

        var reusableElement = elementCreateResult.Result.Content!;
        Assert.AreNotEqual(0, reusableElement.Id);

        var elementPublishResult = await ElementPublishingService.PublishAsync(
            reusableElement.Key,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(elementPublishResult.Success);

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
