using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine.ContentIndexRebuild;

internal abstract partial class ContentIndexRebuildTestBase
{
    /// <summary>
    ///     The document and element types used by the seeded content.
    /// </summary>
    /// <remarks>
    ///     Blocks nest three levels deep: a section holds containers, which hold leaves. All element types vary by culture,
    ///     and each has both culture variant and invariant properties.
    /// </remarks>
    protected sealed record IndexingSchema(
        IContentType Leaf,
        IContentType Container,
        IContentType Section,
        IContentType VariantPage,
        IContentType InvariantPage);

    /// <summary>
    ///     Describes the block values to generate for one block editor property value.
    /// </summary>
    /// <param name="Depth">1 for leaves only, 2 for containers of leaves, 3 for sections of containers of leaves.</param>
    /// <param name="BlocksPerLevel">The number of blocks at each level.</param>
    /// <param name="ValueCultures">
    ///     The cultures to store culture variant element values for (block level variance), or <c>null</c> to store them
    ///     without a culture, as for a culture variant block editor property.
    /// </param>
    /// <param name="TextLength">The minimum length of each generated text value.</param>
    /// <param name="UnexposedCulture">A culture for which the first block at each level is not exposed.</param>
    /// <param name="InvariantValueWithCulture">
    ///     A culture to also store for an invariant element property, as happens when the element property variance changes
    ///     after the value was saved.
    /// </param>
    /// <param name="NestedValuesAsStrings">
    ///     Store nested block editor values as serialized JSON strings rather than as JSON objects, as older data may.
    /// </param>
    protected sealed record BlockSpec(
        int Depth,
        int BlocksPerLevel,
        string[]? ValueCultures,
        int TextLength = 0,
        string? UnexposedCulture = null,
        string? InvariantValueWithCulture = null,
        bool NestedValuesAsStrings = false);

    protected async Task<IndexingSchema> CreateSchemaAsync()
    {
        IContentType leaf = CreateElementType("leafElement", builder => builder);
        IDataType leafBlocks = await CreateBlockListDataTypeAsync("Leaf Blocks", leaf);

        IContentType container = CreateElementType("containerElement", builder => builder
            .AddPropertyType()
                .WithAlias("items")
                .WithName("Items")
                .WithDataTypeId(leafBlocks.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Nothing)
                .Done());
        IDataType containerBlocks = await CreateBlockListDataTypeAsync("Container Blocks", container, leaf);

        IContentType section = CreateElementType("sectionElement", builder => builder
            .AddPropertyType()
                .WithAlias("items")
                .WithName("Items")
                .WithDataTypeId(containerBlocks.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Nothing)
                .Done());
        IDataType sectionBlocks = await CreateBlockListDataTypeAsync("Section Blocks", section, container, leaf);
        IDataType grid = await CreateBlockGridDataTypeAsync(leaf);

        IContentType variantPage = new ContentTypeBuilder()
            .WithAlias("variantPage")
            .WithName("Variant Page")
            .WithKey(DeterministicKey("variantPage"))
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .WithVariations(ContentVariation.Culture)
                .Done()
            .AddPropertyType()
                .WithAlias("variantBlocks")
                .WithName("Variant Blocks")
                .WithDataTypeId(sectionBlocks.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Culture)
                .Done()
            .AddPropertyType()
                .WithAlias("invariantBlocks")
                .WithName("Invariant Blocks")
                .WithDataTypeId(sectionBlocks.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .AddPropertyType()
                .WithAlias("body")
                .WithName("Body")
                .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Culture)
                .Done()
            .AddPropertyType()
                .WithAlias("invariantBody")
                .WithName("Invariant Body")
                .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .Build();
        variantPage.AllowedAsRoot = true;
        ContentTypeService.Save(variantPage);

        IContentType invariantPage = new ContentTypeBuilder()
            .WithAlias("invariantPage")
            .WithName("Invariant Page")
            .WithKey(DeterministicKey("invariantPage"))
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .AddPropertyType()
                .WithAlias("invariantBlocks")
                .WithName("Invariant Blocks")
                .WithDataTypeId(sectionBlocks.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .AddPropertyType()
                .WithAlias("grid")
                .WithName("Grid")
                .WithDataTypeId(grid.Id)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockGrid)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .Build();
        invariantPage.AllowedAsRoot = true;
        ContentTypeService.Save(invariantPage);

        return new IndexingSchema(leaf, container, section, variantPage, invariantPage);
    }

    /// <summary>
    ///     Creates a culture variant document, saving and optionally publishing it.
    /// </summary>
    /// <param name="schema">The schema to create the document from.</param>
    /// <param name="name">The name of the document in the first culture; other cultures get the culture appended.</param>
    /// <param name="cultures">The cultures to create the document in.</param>
    /// <param name="setValues">Sets the property values; called once before publishing.</param>
    /// <param name="culturesToPublish">The cultures to publish, or an empty array to leave the document unpublished.</param>
    /// <param name="setDraftValues">If set, changes property values after publishing so the draft differs from the published version.</param>
    /// <returns>The created document.</returns>
    protected IContent CreateVariantDocument(
        IndexingSchema schema,
        string name,
        string[] cultures,
        Action<IContent> setValues,
        string[] culturesToPublish,
        Action<IContent>? setDraftValues = null)
    {
        var content = new ContentBuilder()
            .WithContentType(schema.VariantPage)
            .WithKey(DeterministicKey(name))
            .Build();
        foreach (var culture in cultures)
        {
            content.SetCultureName(culture == cultures[0] ? name : $"{name} ({culture})", culture);
        }

        setValues(content);
        SaveAndPublish(content, culturesToPublish, setDraftValues);
        return content;
    }

    /// <summary>
    ///     Creates an invariant document, saving and optionally publishing it.
    /// </summary>
    /// <returns>The created document.</returns>
    protected IContent CreateInvariantDocument(
        IndexingSchema schema,
        string name,
        Action<IContent> setValues,
        bool publish,
        Action<IContent>? setDraftValues = null)
    {
        var content = new ContentBuilder()
            .WithContentType(schema.InvariantPage)
            .WithKey(DeterministicKey(name))
            .WithName(name)
            .Build();

        setValues(content);
        SaveAndPublish(content, publish ? ["*"] : [], setDraftValues);
        return content;
    }

    private void SaveAndPublish(Content content, string[] culturesToPublish, Action<IContent>? setDraftValues)
    {
        ContentService.Save(content);
        if (culturesToPublish.Length > 0)
        {
            PublishResult result = ContentService.Publish(content, culturesToPublish);
            if (result.Success is false)
            {
                throw new InvalidOperationException($"Could not publish {content.Name}: {result.Result}");
            }
        }

        if (setDraftValues is not null)
        {
            setDraftValues(content);
            ContentService.Save(content);
        }
    }

    protected string BlockListJson(IndexingSchema schema, string seed, BlockSpec spec)
        => JsonSerializer.Serialize(BuildBlockList(schema, seed, spec, spec.Depth));

    protected string BlockGridJson(IndexingSchema schema, string seed, BlockSpec spec)
    {
        var rootKey = DeterministicKey($"{seed}/grid/root");
        var areaItemKeys = Enumerable.Range(0, spec.BlocksPerLevel)
            .Select(i => DeterministicKey($"{seed}/grid/area/{i}"))
            .ToArray();

        var value = new BlockGridValue(
        [
            new BlockGridLayoutItem(rootKey)
            {
                ColumnSpan = 12,
                RowSpan = 1,
                Areas =
                [
                    new BlockGridLayoutAreaItem(DeterministicKey("gridArea"))
                    {
                        Items = areaItemKeys.Select(key => new BlockGridLayoutItem(key) { ColumnSpan = 12, RowSpan = 1 }).ToArray(),
                    },
                ],
            },
        ])
        {
            ContentData = new[] { rootKey }.Concat(areaItemKeys)
                .Select((key, i) => LeafData(schema, key, $"{seed}/grid/{i}", spec))
                .ToList(),
            Expose = new[] { rootKey }.Concat(areaItemKeys)
                .SelectMany(key => ExposeFor(key, spec, isFirst: false))
                .ToList(),
        };

        return JsonSerializer.Serialize(value);
    }

    protected string RichTextJson(IndexingSchema schema, string seed, BlockSpec spec)
    {
        var keys = Enumerable.Range(0, spec.BlocksPerLevel)
            .Select(i => DeterministicKey($"{seed}/rte/{i}"))
            .ToArray();

        var markup = $"<p>{Text($"{seed} markup", spec.TextLength)}</p>"
                     + string.Concat(keys.Select(key => $"<umb-rte-block data-content-key=\"{key:D}\"><!--Umbraco-Block--></umb-rte-block>"));

        var blocks = new RichTextBlockValue(keys.Select(key => new RichTextBlockLayoutItem(key)))
        {
            ContentData = keys.Select((key, i) => LeafData(schema, key, $"{seed}/rte/{i}", spec)).ToList(),
            Expose = keys.SelectMany((key, i) => ExposeFor(key, spec, isFirst: i == 0)).ToList(),
        };

        return RichTextPropertyEditorHelper.SerializeRichTextEditorValue(
            new RichTextEditorValue { Markup = markup, Blocks = blocks },
            JsonSerializer);
    }

    private BlockListValue BuildBlockList(IndexingSchema schema, string seed, BlockSpec spec, int depth)
    {
        var keys = Enumerable.Range(0, spec.BlocksPerLevel)
            .Select(i => DeterministicKey($"{seed}/{depth}/{i}"))
            .ToArray();

        List<BlockItemData> contentData = keys
            .Select((key, i) => depth switch
            {
                1 => LeafData(schema, key, $"{seed}/{i}", spec),
                2 => NestingData(schema.Container, "title", key, $"{seed}/{i}", spec, BuildBlockList(schema, $"{seed}/{i}", spec, 1)),
                _ => NestingData(schema.Section, "heading", key, $"{seed}/{i}", spec, BuildBlockList(schema, $"{seed}/{i}", spec, depth - 1)),
            })
            .ToList();

        return new BlockListValue(keys.Select(key => new BlockListLayoutItem(key)))
        {
            ContentData = contentData,
            Expose = keys.SelectMany((key, i) => ExposeFor(key, spec, isFirst: i == 0)).ToList(),
        };
    }

    private static BlockItemData LeafData(IndexingSchema schema, Guid key, string seed, BlockSpec spec)
        => new(key, schema.Leaf.Key, schema.Leaf.Alias)
        {
            Values = InvariantValues("invariantText", seed, spec)
                .Concat(VariantValues("variantText", seed, spec))
                .ToList(),
        };

    private BlockItemData NestingData(IContentType elementType, string variantAlias, Guid key, string seed, BlockSpec spec, BlockListValue nested)
        => new(key, elementType.Key, elementType.Alias)
        {
            Values = InvariantValues("invariantText", seed, spec)
                .Concat(VariantValues(variantAlias, seed, spec))
                .Append(new BlockPropertyValue { Alias = "items", Value = spec.NestedValuesAsStrings ? JsonSerializer.Serialize(nested) : nested })
                .ToList(),
        };

    private static IEnumerable<BlockPropertyValue> InvariantValues(string alias, string seed, BlockSpec spec)
    {
        yield return new BlockPropertyValue { Alias = alias, Value = Text($"{seed} {alias}", spec.TextLength) };
        if (spec.InvariantValueWithCulture is not null)
        {
            yield return new BlockPropertyValue
            {
                Alias = alias,
                Culture = spec.InvariantValueWithCulture,
                Value = Text($"{seed} {alias} stale {spec.InvariantValueWithCulture}", spec.TextLength),
            };
        }
    }

    private static IEnumerable<BlockPropertyValue> VariantValues(string alias, string seed, BlockSpec spec)
        => spec.ValueCultures is null
            ? [new BlockPropertyValue { Alias = alias, Value = Text($"{seed} {alias}", spec.TextLength) }]
            : spec.ValueCultures.Select(culture => new BlockPropertyValue
            {
                Alias = alias,
                Culture = culture,
                Value = Text($"{seed} {alias} {culture}", spec.TextLength),
            });

    private static IEnumerable<BlockItemVariation> ExposeFor(Guid key, BlockSpec spec, bool isFirst)
        => spec.ValueCultures is null
            ? [new BlockItemVariation(key, null, null)]
            : spec.ValueCultures
                .Where(culture => isFirst is false || culture != spec.UnexposedCulture)
                .Select(culture => new BlockItemVariation(key, culture, null));

    /// <summary>
    ///     Creates a readable text value, padded with words to at least the given length.
    /// </summary>
    /// <returns>The seed, padded to at least the minimum length.</returns>
    protected static string Text(string seed, int minimumLength)
    {
        if (seed.Length >= minimumLength)
        {
            return seed;
        }

        const string filler = " lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor";
        var builder = new System.Text.StringBuilder(seed, minimumLength + filler.Length);
        while (builder.Length < minimumLength)
        {
            builder.Append(filler);
        }

        return builder.ToString();
    }

    private IContentType CreateElementType(string alias, Func<ContentTypeBuilder, ContentTypeBuilder> addProperties)
    {
        ContentTypeBuilder builder = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithKey(DeterministicKey(alias))
            .WithIsElement(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
                .WithAlias("invariantText")
                .WithName("Invariant text")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .AddPropertyType()
                .WithAlias(alias == "leafElement" ? "variantText" : alias == "containerElement" ? "title" : "heading")
                .WithName("Variant text")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .WithVariations(ContentVariation.Culture)
                .Done();

        IContentType elementType = addProperties(builder).Build();
        ContentTypeService.Save(elementType);
        return elementType;
    }

    private async Task<IDataType> CreateBlockListDataTypeAsync(string name, params IContentType[] elementTypes)
        => await CreateDataTypeAsync(
            name,
            Constants.PropertyEditors.Aliases.BlockList,
            elementTypes
                .Select(elementType => new BlockListConfiguration.BlockConfiguration { ContentElementTypeKey = elementType.Key })
                .ToArray());

    private async Task<IDataType> CreateBlockGridDataTypeAsync(IContentType elementType)
        => await CreateDataTypeAsync(
            "Grid",
            Constants.PropertyEditors.Aliases.BlockGrid,
            new BlockGridConfiguration.BlockGridBlockConfiguration[]
            {
                new()
                {
                    ContentElementTypeKey = elementType.Key,
                    Areas =
                    [
                        new BlockGridConfiguration.BlockGridAreaConfiguration
                        {
                            Key = DeterministicKey("gridArea"), Alias = "area", ColumnSpan = 12, RowSpan = 1,
                        },
                    ],
                },
            });

    private async Task<IDataType> CreateDataTypeAsync(string name, string editorAlias, object blocksConfiguration)
    {
        var dataType = new DataType(PropertyEditorCollection[editorAlias], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object> { { "blocks", blocksConfiguration } },
            Name = name,
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }
}
