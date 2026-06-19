using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal class BlockListWithReusableContentTest : BlockEditorWithReusableContentTestBase
{
    public static void ConfigureAllowEditInvariantFromNonDefaultTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.AllowEditInvariantFromNonDefault = true);

    public static void ConfigureIndexExternalElementsTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<IndexingSettings>(config =>
            config.IndexExternalElements = true);

    [Test]
    public async Task Can_Handle_Reusable_Element()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true },
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true },
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
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var localElementKey = Guid.NewGuid();
        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, SettingsKey = localElementKey, IsExternalContent = true }
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
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

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
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

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
                        new BlockListLayoutItem { ContentKey = reusableElementKey, SettingsKey = settingsForReusableElementKey, IsExternalContent = true }
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
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);

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
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

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
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
        AssertBlockValues();

        // Retry to re-visit the cache within the same test, but for another culture, so we don't get false positives
        // because of caching. No matter the requested culture, the variant-for-invariant handling should always
        // resolve the default language for rendering.
        culture = culture == "en-US" ? "da-DK" : "en-US";
        SetVariationContext(culture, null);
        AssertBlockValues();

        void AssertBlockValues()
        {
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

        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        var reusableElementKey = await CreateAndPublishVariantReusableElement(elementType.Key, elementCulturesToPublish.ToArray());

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
    [ConfigureBuilder(ActionName = nameof(ConfigureIndexExternalElementsTrue))]
    public async Task Can_Include_Invariant_Reusable_Elements_In_Search_Indexing(bool published)
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
            }).ToList();

        var indexValue = indexValues.FirstOrDefault(v => v.Culture is null && v.FieldName == "blocks");
        Assert.IsNotNull(indexValue);
        Assert.AreEqual(1, indexValue!.Values.Count());

        var indexedValue = indexValue.Values.First() as string;
        Assert.IsNotNull(indexedValue);

        var values = indexedValue!.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(2, values.Length);
        Assert.Contains("The reusable invariant text", values);
        Assert.Contains("The reusable variant text", values);
    }

    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(ConfigureIndexExternalElementsTrue))]
    public async Task Can_Include_Variant_Reusable_Elements_In_Search_Indexing(bool published)
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishVariantReusableElement(elementType.Key, ["en-US", "da-DK"]);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
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
            }).ToList();

        AssertIndexValues("en-US", "The reusable English text");
        AssertIndexValues("da-DK", "The reusable Danish text");

        void AssertIndexValues(string culture, string variantText)
        {
            var indexValue = indexValues.FirstOrDefault(v => v.Culture == culture && v.FieldName == "blocks");
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue!.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            var values = indexedValue!.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, values.Length);
            Assert.Contains(variantText, values);
            Assert.Contains("The reusable invariant text", values);
        }
    }

    [Test]
    public async Task Does_Not_Index_Shared_Element_Content_When_Opt_In_Disabled()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, blockListDataType);

        var reusableElementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);

        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [
                        new BlockListLayoutItem { ContentKey = reusableElementKey, IsExternalContent = true }
                    ]
                },
            },
            ContentData = [],
            SettingsData = [],
            Expose = [],
        };

        var content = new ContentBuilder().WithContentType(contentType).WithName("Page").Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, ["*"]);

        var editor = blockListDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: true,
            availableCultures: ["en-US"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType },
            }).ToList();

        // Element content must NOT be indexed when opt-in is disabled.
        var allText = string.Join(
            Environment.NewLine,
            indexValues.SelectMany(v => v.Values).OfType<string>());
        Assert.IsFalse(allText.Contains("The reusable invariant text"), "Shared-element content must not be indexed when the opt-in is disabled.");
        Assert.IsFalse(allText.Contains("The reusable variant text"), "Shared-element content must not be indexed when the opt-in is disabled.");
    }

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockList,
            new BlockListConfiguration.BlockConfiguration[]
            {
                new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
            });
}
