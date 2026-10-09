// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_15_0_0;

/// <summary>
/// Tests for the conversion of legacy local links performed by <see cref="ConvertLocalLinks" />.
/// </summary>
/// <remarks>
/// Every test runs with the production page size and with a page size of two, so that the conversion
/// is verified both within a single page and across page boundaries.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ConvertLocalLinksTests : UmbracoIntegrationTest
{
    private const string PropertyAlias = "bodyText";
    private const string NestedPropertyAlias = "nestedText";
    private const string MediaPickerPropertyAlias = "media";

    private static readonly Guid LinkedDocumentKey = new("8729b1c6-e11a-48ff-909f-a30b9b2ad74f");
    private static readonly Guid LinkedMediaKey = new("7e21a725-b905-4c5f-86dc-8c41ec116e39");

    private static readonly string LegacyDocumentLink =
        $"<a href=\"/{{localLink:umb://document/{LinkedDocumentKey:N}}}\" title=\"Doc\">Doc</a>";

    private static readonly string ConvertedDocumentLink =
        $"<a href=\"/{{localLink:{LinkedDocumentKey:D}}}\" type=\"document\" title=\"Doc\">Doc</a>";

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        new ConvertLocalLinkComposer().Compose(builder);
#pragma warning restore CS0618 // Type or member is obsolete

        // The value editors resolve referenced media through the repository caches, so exercise them with real
        // caches rather than the harness default of AppCaches.NoCache.
        builder.Services.AddUnique(_ => new AppCaches(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            NoAppCache.Instance,
            new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()))));
    }

    [Test]
    public async Task Converts_Udi_Based_Local_Links_In_Rich_Text([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        var legacyMarkup =
            $"<p>{LegacyDocumentLink} and <a href=\"/{{localLink:umb://media/{LinkedMediaKey:N}}}\" title=\"Media\">Media</a></p>";
        Content content = SaveContent(contentType, "Page", RichText(legacyMarkup));

        await ExecuteMigrationAsync(smallPages);

        Assert.That(
            await GetStoredValueAsync(content.Id),
            Is.EqualTo(ConvertedRichText(
                $"<p>{ConvertedDocumentLink} and <a href=\"/{{localLink:{LinkedMediaKey:D}}}\" type=\"media\" title=\"Media\">Media</a></p>")));
    }

    [Test]
    public async Task Converts_Integer_Based_Local_Links_That_Resolve_To_An_Entity([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        Content target = SaveContent(contentType, "Target", RichText("<p>Target</p>"));
        Content content = SaveContent(
            contentType,
            "Page",
            RichText($"<p><a href=\"/{{localLink:{target.Id}}}\">Found</a><a href=\"/{{localLink:987654}}\">Missing</a></p>"));

        await ExecuteMigrationAsync(smallPages);

        Assert.That(
            await GetStoredValueAsync(content.Id),
            Is.EqualTo(ConvertedRichText(
                $"<p><a href=\"/{{localLink:{target.Key:D}}}\" type=\"document\">Found</a><a href=\"/{{localLink:987654}}\">Missing</a></p>")));
    }

    [Test]
    public async Task Converts_Encoded_Local_Links_And_Preserves_Fragments_And_Query_Strings([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        var legacyMarkup =
            $"<p><a href=\"/%7BlocalLink:umb://document/{LinkedDocumentKey:N}%7D\">Encoded</a>"
            + $"<a href=\"/{{localLink:umb://document/{LinkedDocumentKey:N}}}#section\">Fragment</a>"
            + $"<a href=\"/{{localLink:umb://document/{LinkedDocumentKey:N}}}?page=2\">Query</a></p>";
        Content content = SaveContent(contentType, "Page", RichText(legacyMarkup));

        await ExecuteMigrationAsync(smallPages);

        Assert.That(
            await GetStoredValueAsync(content.Id),
            Is.EqualTo(ConvertedRichText(
                $"<p><a href=\"/%7BlocalLink:{LinkedDocumentKey:D}%7D\" type=\"document\">Encoded</a>"
                + $"<a href=\"/{{localLink:{LinkedDocumentKey:D}}}#section\" type=\"document\">Fragment</a>"
                + $"<a href=\"/{{localLink:{LinkedDocumentKey:D}}}?page=2\" type=\"document\">Query</a></p>")));
    }

    [Test]
    public async Task Converts_Local_Links_Regardless_Of_Casing([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        Content content = SaveContent(
            contentType,
            "Page",
            RichText($"<p><a href=\"/{{LOCALLINK:umb://document/{LinkedDocumentKey:N}}}\">Upper</a></p>"));

        await ExecuteMigrationAsync(smallPages);

        Assert.That(
            await GetStoredValueAsync(content.Id),
            Is.EqualTo(ConvertedRichText(
                $"<p><a href=\"/{{LOCALLINK:{LinkedDocumentKey:D}}}\" type=\"document\">Upper</a></p>")));
    }

    [Test]
    public async Task Appends_Data_Anchor_To_Converted_Local_Links([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        Content content = SaveContent(
            contentType,
            "Page",
            RichText($"<p><a data-anchor=\"#top\" href=\"/{{localLink:umb://document/{LinkedDocumentKey:N}}}\">Top</a></p>"));

        await ExecuteMigrationAsync(smallPages);

        Assert.That(
            await GetStoredValueAsync(content.Id),
            Is.EqualTo(ConvertedRichText(
                $"<p><a data-anchor=\"#top\" href=\"/{{localLink:{LinkedDocumentKey:D}}}#top\" type=\"document\">Top</a></p>")));
    }

    [Test]
    public async Task Converts_Block_Udis_In_Rich_Text_Without_Local_Links([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        var blockKey = new Guid("2f6c5d1e-4b3a-4f5e-9a8b-7c6d5e4f3a2b");
        Content content = SaveContent(
            contentType,
            "Page",
            RichText($"<p>Text</p><umb-rte-block data-content-udi=\"umb://element/{blockKey:N}\"></umb-rte-block>"));

        await ExecuteMigrationAsync(smallPages);

        Assert.That(
            await GetStoredValueAsync(content.Id),
            Is.EqualTo(ConvertedRichText($"<p>Text</p><umb-rte-block data-content-key=\"{blockKey:D}\"></umb-rte-block>")));
    }

    [Test]
    public async Task Does_Not_Rewrite_Values_Without_Convertible_Content([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();

        // Whitespace and property order the serializer would never produce, so any rewrite of the row is visible.
        const string untouchedValue = "{ \"blocks\": null, \"markup\": \"<p>No links here, just umb://document text.</p>\" }";
        Content content = SaveContent(contentType, "Page", untouchedValue);

        await ExecuteMigrationAsync(smallPages);

        Assert.That(await GetStoredValueAsync(content.Id), Is.EqualTo(untouchedValue));
    }

    [Test]
    public async Task Converts_Rich_Text_Stored_As_Raw_Markup([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        Content content = SaveContent(contentType, "Page", $"<p>{LegacyDocumentLink}</p>");

        await ExecuteMigrationAsync(smallPages);

        Assert.That(await GetStoredValueAsync(content.Id), Is.EqualTo(ConvertedRichText($"<p>{ConvertedDocumentLink}</p>")));
    }

    [Test]
    public async Task Converts_Local_Links_In_Rich_Text_Nested_In_Blocks(
        [Values] bool smallPages,
        [Values(Constants.PropertyEditors.Aliases.BlockList, Constants.PropertyEditors.Aliases.BlockGrid)] string blockEditorAlias)
    {
        IContentType elementType = await CreateElementTypeAsync(
            "nestedElement",
            NestedPropertyAlias,
            Constants.DataTypes.RichtextEditor,
            Constants.PropertyEditors.Aliases.RichText);
        IDataType blockDataType = await CreateBlockDataTypeAsync(blockEditorAlias, elementType.Key);
        IContentType contentType = await CreateContentTypeAsync(
            "blockPage",
            blockDataType.Id,
            blockDataType.EditorAlias);

        var blockKey = new Guid("5b0f6a3c-2d1e-4c7b-8a9f-0e1d2c3b4a59");
        BlockItemData block = BuildBlockItemData(blockKey, elementType, NestedPropertyAlias, BuildRichTextValue($"<p>{LegacyDocumentLink}</p>"));
        var value = blockEditorAlias == Constants.PropertyEditors.Aliases.BlockList
            ? JsonSerializer.Serialize(new BlockListValue
            {
                Layout = BuildLayout(blockEditorAlias, new BlockListLayoutItem { ContentKey = blockKey }),
                ContentData = [block],
                Expose = [new BlockItemVariation(blockKey, null, null)],
            })
            : JsonSerializer.Serialize(new BlockGridValue
            {
                Layout = BuildLayout(
                    blockEditorAlias,
                    new BlockGridLayoutItem { ContentKey = blockKey, ColumnSpan = 12, RowSpan = 1 }),
                ContentData = [block],
                Expose = [new BlockItemVariation(blockKey, null, null)],
            });
        Content content = SaveContent(contentType, "Page", value);

        await ExecuteMigrationAsync(smallPages);

        var storedValue = await GetStoredValueAsync(content.Id);
        BlockValue? storedBlockValue = blockEditorAlias == Constants.PropertyEditors.Aliases.BlockList
            ? JsonSerializer.Deserialize<BlockListValue>(storedValue!)
            : JsonSerializer.Deserialize<BlockGridValue>(storedValue!);
        Assert.That(storedBlockValue, Is.Not.Null);
        BlockItemData storedBlock = storedBlockValue!.ContentData.Single();
        Assert.Multiple(() =>
        {
            Assert.That(storedBlock.Key, Is.EqualTo(blockKey));
            Assert.That(
                GetRichTextMarkup(storedBlock.Values.Single(x => x.Alias == NestedPropertyAlias).Value),
                Is.EqualTo($"<p>{ConvertedDocumentLink}</p>"));
        });
    }

    [Test]
    public async Task Removes_Missing_Media_From_Media_Pickers_In_Rich_Text_Blocks_When_Converting([Values] bool smallPages)
    {
        IMedia existingMedia = MediaService.CreateMedia("Existing", Constants.System.Root, Constants.Conventions.MediaTypes.Image);
        MediaService.Save(existingMedia);
        var missingMediaKey = new Guid("0d4c3b2a-1f0e-4d9c-8b7a-6f5e4d3c2b1a");

        IContentType elementType = await CreateElementTypeAsync(
            "mediaElement",
            MediaPickerPropertyAlias,
            (await DataTypeService.GetAsync(Constants.DataTypes.Guids.MediaPicker3Guid))!.Id,
            Constants.PropertyEditors.Aliases.MediaPicker3);
        IDataType richTextDataType = await CreateDataTypeAsync(
            "Rich Text With Blocks",
            Constants.PropertyEditors.Aliases.RichText,
            new Dictionary<string, object>
            {
                {
                    "blocks",
                    new[] { new RichTextConfiguration.RichTextBlockConfiguration { ContentElementTypeKey = elementType.Key } }
                },
            });
        IContentType contentType = await CreateContentTypeAsync("mediaPage", richTextDataType.Id, richTextDataType.EditorAlias);

        var blockKey = new Guid("9e8d7c6b-5a4f-4e3d-2c1b-0a9f8e7d6c5b");
        var mediaPickerValue = JsonSerializer.Serialize(new[]
        {
            new { key = new Guid("11111111-2222-4333-8444-555555555555"), mediaKey = existingMedia.Key },
            new { key = new Guid("66666666-7777-4888-9999-000000000000"), mediaKey = missingMediaKey },
        });
        var value = JsonSerializer.Serialize(new RichTextEditorValue
        {
            Markup = $"<p>{LegacyDocumentLink}</p><umb-rte-block data-content-key=\"{blockKey:D}\"></umb-rte-block>",
            Blocks = new RichTextBlockValue
            {
                Layout = BuildLayout(
                    Constants.PropertyEditors.Aliases.RichText,
                    new RichTextBlockLayoutItem { ContentKey = blockKey }),
                ContentData = [BuildBlockItemData(blockKey, elementType, MediaPickerPropertyAlias, mediaPickerValue)],
                Expose = [new BlockItemVariation(blockKey, null, null)],
            },
        });
        Content content = SaveContent(contentType, "Page", value);

        await ExecuteMigrationAsync(smallPages);

        RichTextEditorValue? storedValue = JsonSerializer.Deserialize<RichTextEditorValue>((await GetStoredValueAsync(content.Id))!);
        Assert.That(storedValue?.Blocks, Is.Not.Null);
        var storedMediaPickerValue = JsonSerializer.Serialize(
            storedValue!.Blocks!.ContentData.Single().Values.Single(x => x.Alias == MediaPickerPropertyAlias).Value);
        Assert.Multiple(() =>
        {
            Assert.That(
                storedValue.Markup,
                Is.EqualTo($"<p>{ConvertedDocumentLink}</p><umb-rte-block data-content-key=\"{blockKey:D}\"></umb-rte-block>"));
            Assert.That(storedMediaPickerValue, Does.Contain(existingMedia.Key.ToString("D")));
            Assert.That(storedMediaPickerValue, Does.Not.Contain(missingMediaKey.ToString("D")));
        });
    }

    [Test]
    public async Task Converts_Each_Culture_Of_A_Variant_Property([Values] bool smallPages)
    {
        await LanguageService.CreateAsync(
            new LanguageBuilder().WithCultureInfo("da-DK").Build(),
            Constants.Security.SuperUserKey);
        IContentType contentType = await CreateRichTextContentTypeAsync(variesByCulture: true);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Page")
            .WithCultureName("da-DK", "Side")
            .Build();
        content.SetValue(PropertyAlias, RichText($"<p>English {LegacyDocumentLink}</p>"), "en-US");
        content.SetValue(PropertyAlias, RichText($"<p>Dansk {LegacyDocumentLink}</p>"), "da-DK");
        ContentService.Save(content);

        await ExecuteMigrationAsync(smallPages);

        IDictionary<int, string?> storedValues = await GetStoredValuesByLanguageAsync(content.Id);
        var languageIdsByCulture = (await LanguageService.GetAllAsync()).ToDictionary(x => x.IsoCode, x => x.Id);
        Assert.Multiple(() =>
        {
            Assert.That(storedValues, Has.Count.EqualTo(2));
            Assert.That(
                storedValues[languageIdsByCulture["en-US"]],
                Is.EqualTo(ConvertedRichText($"<p>English {ConvertedDocumentLink}</p>")));
            Assert.That(
                storedValues[languageIdsByCulture["da-DK"]],
                Is.EqualTo(ConvertedRichText($"<p>Dansk {ConvertedDocumentLink}</p>")));
        });
    }

    [Test]
    public async Task Converts_Only_Current_And_Published_Versions([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        Content content = SaveContent(contentType, "Page", RichText("<p>Initial</p>"));
        ContentService.Publish(content, ["*"]);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);
        ContentService.Save(content);

        List<VersionState> versions = await GetVersionsAsync(content.Id);
        VersionState historicVersion = versions.Single(x => x.Current is false && x.Published is false);
        VersionState publishedVersion = versions.Single(x => x.Current is false && x.Published);
        VersionState currentVersion = versions.Single(x => x.Current && x.Published is false);

        var legacyValue = RichText($"<p>{LegacyDocumentLink}</p>");
        await SetStoredValueAsync(historicVersion.Id, textValue: legacyValue);
        await SetStoredValueAsync(publishedVersion.Id, textValue: legacyValue);
        await SetStoredValueAsync(currentVersion.Id, textValue: legacyValue);

        await ExecuteMigrationAsync(smallPages);

        var convertedValue = ConvertedRichText($"<p>{ConvertedDocumentLink}</p>");
        var historicValue = (await GetStoredRowAsync(historicVersion.Id)).TextValue;
        var publishedValue = (await GetStoredRowAsync(publishedVersion.Id)).TextValue;
        var currentValue = (await GetStoredRowAsync(currentVersion.Id)).TextValue;
        Assert.Multiple(() =>
        {
            Assert.That(historicValue, Is.EqualTo(legacyValue));
            Assert.That(publishedValue, Is.EqualTo(convertedValue));
            Assert.That(currentValue, Is.EqualTo(convertedValue));
        });
    }

    [Test]
    public async Task Converts_Local_Links_In_Media_Properties([Values] bool smallPages)
    {
        IMediaType mediaType = new MediaTypeBuilder()
            .WithAlias("richMedia")
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .AddPropertyType()
                    .WithAlias(PropertyAlias)
                    .WithName("Body Text")
                    .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
                    .WithValueStorageType(ValueStorageType.Ntext)
                    .Done()
                .Done()
            .Build();
        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        IMedia media = MediaService.CreateMedia("Rich media", Constants.System.Root, mediaType.Alias);
        media.SetValue(PropertyAlias, RichText($"<p>{LegacyDocumentLink}</p>"));
        MediaService.Save(media);

        await ExecuteMigrationAsync(smallPages);

        Assert.That(await GetStoredValueAsync(media.Id), Is.EqualTo(ConvertedRichText($"<p>{ConvertedDocumentLink}</p>")));
    }

    [Test]
    public async Task Converts_Values_Held_In_The_Varchar_Column([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        Content content = SaveContent(contentType, "Page", RichText("<p>Initial</p>"));
        var legacyValue = RichText($"<p>{LegacyDocumentLink}</p>");
        await SetStoredValueAsync(content.VersionId, textValue: null, varcharValue: legacyValue);

        await ExecuteMigrationAsync(smallPages);

        PropertyDataDto storedRow = await GetStoredRowAsync(content.VersionId);
        Assert.Multiple(() =>
        {
            Assert.That(storedRow.TextValue, Is.EqualTo(ConvertedRichText($"<p>{ConvertedDocumentLink}</p>")));
            Assert.That(storedRow.VarcharValue, Is.EqualTo(legacyValue));
        });
    }

    [Test]
    public async Task Converts_All_Values_When_Interleaved_With_Values_That_Need_No_Conversion([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        var expectedValuesById = new Dictionary<int, string>();
        for (var i = 0; i < 7; i++)
        {
            var needsConversion = i % 2 == 0;
            var legacyValue = needsConversion ? RichText($"<p>{i} {LegacyDocumentLink}</p>") : RichText($"<p>{i}</p>");
            Content content = SaveContent(contentType, $"Page {i}", legacyValue);
            expectedValuesById[content.Id] = needsConversion ? ConvertedRichText($"<p>{i} {ConvertedDocumentLink}</p>") : legacyValue;
        }

        await ExecuteMigrationAsync(smallPages);

        foreach ((int contentId, string expectedValue) in expectedValuesById)
        {
            Assert.That(await GetStoredValueAsync(contentId), Is.EqualTo(expectedValue), $"Content {contentId}");
        }
    }

    [Test]
    public async Task Running_The_Migration_Again_Makes_No_Further_Changes([Values] bool smallPages)
    {
        IContentType contentType = await CreateRichTextContentTypeAsync();
        Content content = SaveContent(contentType, "Page", RichText($"<p>{LegacyDocumentLink}</p>"));

        await ExecuteMigrationAsync(smallPages);
        var afterFirstRun = await GetStoredValueAsync(content.Id);
        await ExecuteMigrationAsync(smallPages);

        Assert.That(await GetStoredValueAsync(content.Id), Is.EqualTo(afterFirstRun));
    }

    private string RichText(string markup) => JsonSerializer.Serialize(BuildRichTextValue(markup));

    // A converted value is re-serialized by the rich text value editor, which writes an empty block value.
    private string ConvertedRichText(string markup)
        => JsonSerializer.Serialize(new RichTextEditorValue { Markup = markup, Blocks = new RichTextBlockValue() });

    private static RichTextEditorValue BuildRichTextValue(string markup) => new() { Markup = markup, Blocks = null };

    private string GetRichTextMarkup(object? value)
    {
        var json = value as string ?? JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<RichTextEditorValue>(json)!.Markup;
    }

    private async Task<IContentType> CreateRichTextContentTypeAsync(bool variesByCulture = false)
    {
        IContentType contentType = BuildContentType(
            "richPage",
            PropertyAlias,
            Constants.DataTypes.RichtextEditor,
            Constants.PropertyEditors.Aliases.RichText);
        contentType.AllowedAsRoot = true;
        if (variesByCulture)
        {
            contentType.Variations = ContentVariation.Culture;
            contentType.PropertyTypes.Single().Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return (await ContentTypeService.GetAsync(contentType.Key))!;
    }

    private async Task<IContentType> CreateContentTypeAsync(string alias, int dataTypeId, string propertyEditorAlias)
    {
        IContentType contentType = BuildContentType(alias, PropertyAlias, dataTypeId, propertyEditorAlias);
        contentType.AllowedAsRoot = true;

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return (await ContentTypeService.GetAsync(contentType.Key))!;
    }

    private async Task<IContentType> CreateElementTypeAsync(
        string alias,
        string propertyAlias,
        int dataTypeId,
        string propertyEditorAlias)
    {
        IContentType elementType = BuildContentType(alias, propertyAlias, dataTypeId, propertyEditorAlias);
        elementType.IsElement = true;

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return (await ContentTypeService.GetAsync(elementType.Key))!;
    }

    private static IContentType BuildContentType(
        string alias,
        string propertyAlias,
        int dataTypeId,
        string propertyEditorAlias)
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName(alias.ToFirstUpperInvariant())
            .AddPropertyType()
                .WithAlias(propertyAlias)
                .WithName(propertyAlias.ToFirstUpperInvariant())
                .WithDataTypeId(dataTypeId)
                .WithPropertyEditorAlias(propertyEditorAlias)
                .WithValueStorageType(ValueStorageType.Ntext)
                .Done()
            .Build();
        contentType.AllowedTemplates = [];

        return contentType;
    }

    private async Task<IDataType> CreateBlockDataTypeAsync(string blockEditorAlias, Guid elementTypeKey)
        => blockEditorAlias == Constants.PropertyEditors.Aliases.BlockList
            ? await CreateDataTypeAsync(
                "Block List",
                blockEditorAlias,
                new Dictionary<string, object>
                {
                    {
                        "blocks",
                        new[] { new BlockListConfiguration.BlockConfiguration { ContentElementTypeKey = elementTypeKey } }
                    },
                })
            : await CreateDataTypeAsync(
                "Block Grid",
                blockEditorAlias,
                new Dictionary<string, object>
                {
                    {
                        "blocks",
                        new[]
                        {
                            new BlockGridConfiguration.BlockGridBlockConfiguration
                            {
                                ContentElementTypeKey = elementTypeKey,
                                AllowAtRoot = true,
                            },
                        }
                    },
                });

    private async Task<IDataType> CreateDataTypeAsync(
        string name,
        string propertyEditorAlias,
        Dictionary<string, object> configurationData)
    {
        var dataType = new DataType(PropertyEditors[propertyEditorAlias], ConfigurationEditorJsonSerializer)
        {
            Name = name,
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            ConfigurationData = configurationData,
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    private static BlockItemData BuildBlockItemData(
        Guid blockKey,
        IContentType elementType,
        string propertyAlias,
        object? propertyValue)
        => new()
        {
            Key = blockKey,
            ContentTypeAlias = elementType.Alias,
            ContentTypeKey = elementType.Key,
            Values = [new BlockPropertyValue { Alias = propertyAlias, Value = propertyValue }],
        };

    private static Dictionary<string, IEnumerable<IBlockLayoutItem>> BuildLayout(
        string propertyEditorAlias,
        IBlockLayoutItem layoutItem)
        => new() { { propertyEditorAlias, [layoutItem] } };

    private Content SaveContent(IContentType contentType, string name, string propertyValue)
    {
        // The value is set directly rather than through a value editor, so the database holds exactly the
        // pre-migration value the test built.
        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName(name)
            .Build();
        content.SetValue(PropertyAlias, propertyValue);

        ContentService.Save(content);

        return content;
    }

    private async Task ExecuteMigrationAsync(bool smallPages)
    {
        MigrationPlan plan = new MigrationPlan(nameof(ConvertLocalLinksTests)).From(string.Empty);
        plan = smallPages
            ? plan.To<SmallPageConvertLocalLinks>("done")
            : plan.To<ConvertLocalLinks>("done");

        var executor = new MigrationPlanExecutor(
            GetRequiredService<ICoreScopeProvider>(),
            ScopeAccessor,
            LoggerFactory,
            GetRequiredService<IMigrationBuilder>(),
            GetRequiredService<IUmbracoDatabaseFactory>(),
            new NoopDatabaseCacheRebuilder(),
            GetRequiredService<DistributedCache>(),
            Mock.Of<IKeyValueService>(),
            GetRequiredService<IServiceScopeFactory>(),
            GetRequiredService<AppCaches>(),
            GetRequiredService<IPublishedContentTypeFactory>());

        ExecutedMigrationPlan result = await executor.ExecutePlanAsync(plan, string.Empty);

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
    }

    private async Task<List<PropertyDataDto>> GetStoredRowsAsync(Sql<ISqlContext> sql)
    {
        using Cms.Infrastructure.Scoping.IScope scope = ScopeProvider.CreateScope();
        List<PropertyDataDto> rows = await scope.Database.FetchAsync<PropertyDataDto>(sql);
        scope.Complete();
        return rows;
    }

    private Sql<ISqlContext> Sql() => GetRequiredService<IUmbracoDatabaseFactory>().SqlContext.Sql();

    private Sql<ISqlContext> SelectCurrentPropertyData(int nodeId)
        => Sql()
            .Select<PropertyDataDto>()
            .From<PropertyDataDto>()
            .InnerJoin<ContentVersionDto>()
            .On<PropertyDataDto, ContentVersionDto>(pd => pd.VersionId, cv => cv.Id)
            .Where<ContentVersionDto>(cv => cv.NodeId == nodeId && cv.Current);

    private async Task<string?> GetStoredValueAsync(int nodeId)
        => (await GetStoredRowsAsync(SelectCurrentPropertyData(nodeId))).Single().TextValue;

    private async Task<IDictionary<int, string?>> GetStoredValuesByLanguageAsync(int nodeId)
        => (await GetStoredRowsAsync(SelectCurrentPropertyData(nodeId)))
            .ToDictionary(x => x.LanguageId!.Value, x => x.TextValue);

    private async Task<PropertyDataDto> GetStoredRowAsync(int versionId)
        => (await GetStoredRowsAsync(
                Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .Where<PropertyDataDto>(pd => pd.VersionId == versionId)))
            .Single();

    private async Task SetStoredValueAsync(int versionId, string? textValue, string? varcharValue = null)
    {
        PropertyDataDto row = await GetStoredRowAsync(versionId);
        row.TextValue = textValue;
        row.VarcharValue = varcharValue;

        using Cms.Infrastructure.Scoping.IScope scope = ScopeProvider.CreateScope();
        await scope.Database.UpdateAsync(row);
        scope.Complete();
    }

    private async Task<List<VersionState>> GetVersionsAsync(int nodeId)
    {
        using Cms.Infrastructure.Scoping.IScope scope = ScopeProvider.CreateScope();
        Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
            .Select<ContentVersionDto>(cv => cv.Id, cv => cv.Current)
            .AndSelect<DocumentVersionDto>(dv => dv.Published)
            .From<ContentVersionDto>()
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>(cv => cv.Id, dv => dv.Id)
            .Where<ContentVersionDto>(cv => cv.NodeId == nodeId);
        List<VersionState> versions = await scope.Database.FetchAsync<VersionState>(sql);
        scope.Complete();
        return versions;
    }

    private sealed class VersionState
    {
        public int Id { get; set; }

        public bool Current { get; set; }

        public bool Published { get; set; }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    private sealed class SmallPageConvertLocalLinks : ConvertLocalLinks
    {
        public SmallPageConvertLocalLinks(
            IMigrationContext context,
            IUmbracoContextFactory umbracoContextFactory,
            IContentTypeService contentTypeService,
            ILogger<ConvertLocalLinks> logger,
            IDataTypeService dataTypeService,
            ILanguageService languageService,
            IJsonSerializer jsonSerializer,
            LocalLinkProcessor localLinkProcessor,
            IMediaTypeService mediaTypeService,
            ICoreScopeProvider coreScopeProvider,
            LocalLinkMigrationTracker linkMigrationTracker)
            : base(
                context,
                umbracoContextFactory,
                contentTypeService,
                logger,
                dataTypeService,
                languageService,
                jsonSerializer,
                localLinkProcessor,
                mediaTypeService,
                coreScopeProvider,
                linkMigrationTracker)
        {
        }

        internal override int PageSize => 2;
    }
#pragma warning restore CS0618 // Type or member is obsolete

    private sealed class NoopDatabaseCacheRebuilder : IDatabaseCacheRebuilder
    {
        public Task<Attempt<DatabaseCacheRebuildResult>> RebuildAsync(bool useBackgroundThread)
            => Task.FromResult(Attempt.Succeed(DatabaseCacheRebuildResult.Success));

#pragma warning disable CS0618 // Type or member is obsolete
        public void RebuildDatabaseCacheIfSerializerChanged() => throw new NotSupportedException();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
