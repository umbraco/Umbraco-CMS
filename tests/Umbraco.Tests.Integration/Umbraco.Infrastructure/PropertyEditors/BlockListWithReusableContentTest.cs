using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal class BlockListWithReusableContentTest : BlockEditorElementVariationTestBase
{
    public static void ConfigureAllowEditInvariantFromNonDefaultTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.AllowEditInvariantFromNonDefault = true);

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    [Test]
    public async Task Can_Handle_Reusable_Element()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData = [],
            SettingsData = [],
            Expose = [],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        SetVariationContext("en-US", null);

        var publishedContent = GetPublishedContent(content.Key);
        var value = publishedContent.Value<BlockListModel>("blocks");
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("The reusable variant text", blockListItem.Content.Value<string>("variantText"));
        });
    }

    [Test]
    public async Task Can_Contain_The_Same_Reusable_Element_Multiple_Times()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true },
                    ]
                },
            },
            ContentData = [],
            SettingsData = [],
            Expose = [],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        SetVariationContext("en-US", null);

        var publishedContent = GetPublishedContent(content.Key);
        var value = publishedContent.Value<BlockListModel>("blocks");
        Assert.IsNotNull(value);
        Assert.AreEqual(3, value.Count);

        foreach (var blockListItem in value)
        {
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());
            Assert.Multiple(() =>
            {
                Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
                Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual("The reusable variant text", blockListItem.Content.Value<string>("variantText"));
            });
        }
    }

    [Test]
    public async Task Can_Handle_Reusable_Elements_With_Local_Settings()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var localElementKey = Guid.NewGuid();
        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, SettingsKey = localElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData = [],
            SettingsData =
            [
                new BlockItemData
                {
                    Key = localElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local variant text" }
                    ],
                },
            ],
            Expose = [],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        SetVariationContext("en-US", null);

        var publishedContent = GetPublishedContent(content.Key);
        var value = publishedContent.Value<BlockListModel>("blocks");
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());

        Assert.Multiple(() =>
        {
            Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("The reusable variant text", blockListItem.Content.Value<string>("variantText"));
        });

        Assert.IsNotNull(blockListItem.Settings);
        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());

        Assert.Multiple(() =>
        {
            Assert.AreEqual(localElementKey, blockListItem.SettingsKey);
            Assert.AreEqual("The local invariant text", blockListItem.Settings.Value<string>("invariantText"));
            Assert.AreEqual("The local variant text", blockListItem.Settings.Value<string>("variantText"));
        });
    }

    [Test]
    public async Task Can_Handle_Invariant_Mixed_Local_And_Reusabe_Elements()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var localElementKey = Guid.NewGuid();
        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = localElementKey },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = localElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local variant text" }
                    ],
                },
            ],
            SettingsData = [],
            Expose =
            [
                new BlockItemVariation { ContentKey = localElementKey }
            ],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        SetVariationContext("en-US", null);

        var publishedContent = GetPublishedContent(content.Key);
        var value = publishedContent.Value<BlockListModel>("blocks");
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());

        Assert.Multiple(() =>
        {
            Assert.AreEqual(localElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The local invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("The local variant text", blockListItem.Content.Value<string>("variantText"));
        });

        blockListItem = value.Last();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("The reusable variant text", blockListItem.Content.Value<string>("variantText"));
        });
    }

    [Test]
    public async Task Can_Handle_Invariant_Mixed_Local_And_Reusable_Elements_With_Local_Settings()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var localElementKey = Guid.NewGuid();
        var settingsForLocalElementKey = Guid.NewGuid();
        var settingsForReusableElementKey = Guid.NewGuid();
        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = localElementKey, SettingsKey = settingsForLocalElementKey },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, SettingsKey = settingsForReusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = localElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local variant text" }
                    ],
                },
            ],
            SettingsData =
            [
                new BlockItemData
                {
                    Key = settingsForLocalElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local settings invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local settings variant text" }
                    ],
                },
                new BlockItemData
                {
                    Key = settingsForReusableElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The reusable settings invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The reusable settings variant text" }
                    ],
                },
            ],
            Expose =
            [
                new BlockItemVariation { ContentKey = localElementKey }
            ],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        SetVariationContext("en-US", null);

        var publishedContent = GetPublishedContent(content.Key);
        var value = publishedContent.Value<BlockListModel>("blocks");
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());

        Assert.Multiple(() =>
        {
            Assert.AreEqual(localElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The local invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("The local variant text", blockListItem.Content.Value<string>("variantText"));
        });

        Assert.IsNotNull(blockListItem.Settings);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsForLocalElementKey, blockListItem.SettingsKey);
            Assert.AreEqual("The local settings invariant text", blockListItem.Settings.Value<string>("invariantText"));
            Assert.AreEqual("The local settings variant text", blockListItem.Settings.Value<string>("variantText"));
        });

        blockListItem = value.Last();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("The reusable variant text", blockListItem.Content.Value<string>("variantText"));
        });

        Assert.IsNotNull(blockListItem.Settings);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsForReusableElementKey, blockListItem.SettingsKey);
            Assert.AreEqual("The reusable settings invariant text", blockListItem.Settings.Value<string>("invariantText"));
            Assert.AreEqual("The reusable settings variant text", blockListItem.Settings.Value<string>("variantText"));
        });
    }

    [Test]
    public async Task Can_Handle_Variant_Mixed_Local_And_Reusable_Elements()
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);

        var localElementKey = Guid.NewGuid();
        var reusableElementKey = await CreateAndPublishVariantReusableElement(elementType.Key, ["en-US", "da-DK"]);

        var englishBlockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = localElementKey },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = localElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local English text", Culture = "en-US" },
                    ],
                },
            ],
            SettingsData = [],
            Expose =
            [
                new BlockItemVariation { ContentKey = localElementKey, Culture = "en-US" },
            ],
        };

        var danishBlockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = localElementKey },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = localElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local Danish text", Culture = "da-DK" },
                    ],
                },
            ],
            SettingsData = [],
            Expose =
            [
                new BlockItemVariation { ContentKey = localElementKey, Culture = "da-DK" },
            ],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Page (EN)")
            .WithCultureName("da-DK", "Page (DA)");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(englishBlockListValue), culture: "en-US");
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(danishBlockListValue), culture: "da-DK");
        ContentService.Save(content);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", "English");
        AssertPropertyValues("da-DK", "Danish");

        return;

        void AssertPropertyValues(string culture, string expectedVariantText)
        {
            SetVariationContext(culture, null);

            var publishedContent = GetPublishedContent(content.Key);
            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Count);

            var blockListItem = value.First();
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(localElementKey, blockListItem.ContentKey);
                Assert.AreEqual("The local invariant text", blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual($"The local {expectedVariantText} text", blockListItem.Content.Value<string>("variantText"));
            });

            blockListItem = value.Last();
            Assert.Multiple(() =>
            {
                Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
                Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual($"The reusable {expectedVariantText} text", blockListItem.Content.Value<string>("variantText"));
            });
        }
    }

    [Test]
    public async Task Can_Handle_Variant_Mixed_Local_And_Reusable_Elements_With_Block_Level_Variance()
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var localElementKey = Guid.NewGuid();
        var reusableElementKey = await CreateAndPublishVariantReusableElement(elementType.Key, ["en-US", "da-DK"]);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = localElementKey },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = localElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local English text", Culture = "en-US" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local Danish text", Culture = "da-DK" },
                    ],
                },
            ],
            SettingsData = [],
            Expose =
            [
                new BlockItemVariation { ContentKey = localElementKey, Culture = "en-US" },
                new BlockItemVariation { ContentKey = localElementKey, Culture = "da-DK" },
            ],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Page (EN)")
            .WithCultureName("da-DK", "Page (DA)");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", "English");
        AssertPropertyValues("da-DK", "Danish");

        return;

        void AssertPropertyValues(string culture, string expectedVariantText)
        {
            SetVariationContext(culture, null);

            var publishedContent = GetPublishedContent(content.Key);
            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Count);

            var blockListItem = value.First();
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(localElementKey, blockListItem.ContentKey);
                Assert.AreEqual("The local invariant text", blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual($"The local {expectedVariantText} text", blockListItem.Content.Value<string>("variantText"));
            });

            blockListItem = value.Last();
            Assert.Multiple(() =>
            {
                Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
                Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual($"The reusable {expectedVariantText} text", blockListItem.Content.Value<string>("variantText"));
            });
        }
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public async Task Can_Handle_Variant_Reusable_Elements_For_Invariant_Content(string culture)
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var localElementKey = Guid.NewGuid();
        var reusableElementKey = await CreateAndPublishVariantReusableElement(elementType.Key, ["en-US", "da-DK"]);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = localElementKey },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData =
            [
                new BlockItemData
                {
                    Key = localElementKey,
                    ContentTypeKey = elementType.Key,
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "The local invariant text" },
                        new BlockPropertyValue { Alias = "variantText", Value = "The local variant text", Culture = "en-US" },
                    ],
                },
            ],
            SettingsData = [],
            Expose =
            [
                new BlockItemVariation { ContentKey = localElementKey, Culture = "en-US" },
            ],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        SetVariationContext(culture, null);

        var expectedReusableElementVariantText = culture == "en-US" ? "English" : "Danish";
        var publishedContent = GetPublishedContent(content.Key);
        var value = publishedContent.Value<BlockListModel>("blocks");
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());

        Assert.Multiple(() =>
        {
            Assert.AreEqual(localElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The local invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual($"The local variant text", blockListItem.Content.Value<string>("variantText"));
        });

        blockListItem = value.Last();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
            Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
            Assert.AreEqual($"The reusable {expectedReusableElementVariantText} text", blockListItem.Content.Value<string>("variantText"));
        });
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    // allow edit invariant from non-default to ensure invariant element properties are published
    // even if the default language variant for the element is not published
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Only_Outputs_Published_Variants_Of_Reusable_Elements(bool publishElementInEnglish, bool publishElementInDanish)
    {
        var elementCulturesToPublish = new List<string>();

        if (publishElementInEnglish)
        {
            elementCulturesToPublish.Add("en-US");
        }

        if (publishElementInDanish)
        {
            elementCulturesToPublish.Add("da-DK");
        }

        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var reusableElementKey = await CreateAndPublishVariantReusableElement(elementType.Key, elementCulturesToPublish.ToArray());

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            SettingsData = [],
            Expose = [],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Page (EN)")
            .WithCultureName("da-DK", "Page (DA)");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", "English", publishElementInEnglish);
        AssertPropertyValues("da-DK", "Danish", publishElementInDanish);

        return;

        void AssertPropertyValues(string culture, string expectedVariantText, bool shouldExist)
        {
            SetVariationContext(culture, null);

            var publishedContent = GetPublishedContent(content.Key);
            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);

            if (shouldExist)
            {
                Assert.AreEqual(1, value.Count);

                var blockListItem = value.Single();
                Assert.AreEqual(2, blockListItem.Content.Properties.Count());

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(reusableElementKey, blockListItem.ContentKey);
                    Assert.AreEqual("The reusable invariant text", blockListItem.Content.Value<string>("invariantText"));
                    Assert.AreEqual($"The reusable {expectedVariantText} text", blockListItem.Content.Value<string>("variantText"));
                });
            }
            else
            {
                Assert.IsEmpty(value);
            }
        }
    }


    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Include_Invariant_Reusable_Elements_In_Search_Indexing(bool published)
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData = [],
            SettingsData = [],
            Expose = [],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        var editor = blockListDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: published,
            availableCultures: ["en-US"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType },
            });

        Assert.AreEqual(1, indexValues.Count());

        var indexValue = indexValues.FirstOrDefault(v => v.Culture is null);
        Assert.IsNotNull(indexValue);
        Assert.AreEqual(1, indexValue.Values.Count());

        var indexedValue = indexValue.Values.First() as string;
        Assert.IsNotNull(indexedValue);

        var values = indexedValue.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(2, values.Length);
        Assert.Contains("The reusable invariant text", values);
        Assert.Contains("The reusable variant text", values);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Include_Variant_Reusable_Elements_In_Search_Indexing(bool published)
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishVariantReusableElement(elementType.Key, ["en-US", "da-DK"]);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsSharedContent = true }
                    ]
                },
            },
            ContentData = [],
            SettingsData = [],
            Expose = [],
        };

        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Page");

        var content = contentBuilder.Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, ["*"]);

        var editor = blockListDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: published,
            availableCultures: ["en-US", "da-DK"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType },
            });

        Assert.AreEqual(2, indexValues.Count());

        AssertIndexValues("en-US", "The reusable English text");
        AssertIndexValues("da-DK", "The reusable Danish text");

        void AssertIndexValues(string culture, string variantText)
        {
            var indexValue = indexValues.FirstOrDefault(v => v.Culture == culture);
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            var values = indexedValue.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, values.Length);
            Assert.Contains(variantText, values);
            Assert.Contains("The reusable invariant text", values);
        }
    }

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockList,
            new BlockListConfiguration.BlockConfiguration[]
            {
                new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
            });

    private async Task<Guid> CreateAndPublishInvariantReusableElement(Guid elementTypeKey)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementTypeKey,
                ParentKey = null,
                Properties =
                [
                    new PropertyValueModel { Alias = "invariantText", Value = "The reusable invariant text" },
                    new PropertyValueModel { Alias = "variantText", Value = "The reusable variant text" },
                ],
                Variants =
                [
                    new VariantModel { Name = "Reusable element" }
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var elementKey = createResult.Result.Content!.Key;

        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        return elementKey;
    }

    private async Task<Guid> CreateAndPublishVariantReusableElement(Guid elementTypeKey, string[] culturesToPublish)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementTypeKey,
                ParentKey = null,
                Properties =
                [
                    new PropertyValueModel { Alias = "invariantText", Value = "The reusable invariant text" },
                    new PropertyValueModel { Alias = "variantText", Value = "The reusable English text", Culture = "en-US" },
                    new PropertyValueModel { Alias = "variantText", Value = "The reusable Danish text", Culture = "da-DK" },
                ],
                Variants =
                [
                    new VariantModel { Name = "Reusable element (EN)", Culture = "en-US" },
                    new VariantModel { Name = "Reusable element (DA)", Culture = "da-DK" }
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var elementKey = createResult.Result.Content!.Key;

        if (culturesToPublish.Any())
        {
            var publishResult = await ElementPublishingService.PublishAsync(
                elementKey,
                culturesToPublish.Select(culture => new CulturePublishScheduleModel { Culture = culture }).ToArray(),
                Constants.Security.SuperUserKey);
            Assert.IsTrue(publishResult.Success);
        }

        return elementKey;
    }
}
