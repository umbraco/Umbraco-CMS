// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Tests for the conversion of single block mode Block Lists performed by <see cref="MigrateSingleBlockList" />,
/// covering the nested case reported in https://github.com/umbraco/Umbraco-CMS/issues/23596.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MigrateSingleBlockListTests : UmbracoIntegrationTest
{
    private const string OuterPropertyAlias = "blocks";
    private const string NestedPropertyAlias = "items";
    private const string TextPropertyAlias = "text";
    private const string InnerTextValue = "The inner text";
    private const string OuterTextValue = "The outer text";

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        new MigrateSingleBlockListComposer().Compose(builder);
#pragma warning restore CS0618 // Type or member is obsolete

        // The migration works with, and explicitly clears, the repository caches. With the harness default of
        // AppCaches.NoCache nothing is cached, so a stale read could not happen at all.
        builder.Services.AddUnique(_ => new AppCaches(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            NoAppCache.Instance,
            new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()))));
    }

    [Test]
    public async Task Can_Migrate_Single_Block_List_Nested_In_Block_List()
        => await AssertCanMigrateNestedSingleBlock(ContainerEditor.BlockList);

    [Test]
    public async Task Can_Migrate_Single_Block_List_Nested_In_Block_Grid()
        => await AssertCanMigrateNestedSingleBlock(ContainerEditor.BlockGrid);

    [Test]
    public async Task Can_Migrate_Single_Block_List_Nested_In_Rich_Text()
        => await AssertCanMigrateNestedSingleBlock(ContainerEditor.RichText);

    private async Task AssertCanMigrateNestedSingleBlock(ContainerEditor containerEditor)
    {
        TestSchema schema = await CreateSchemaAsync(containerEditor);
        var innerBlockKey = Guid.NewGuid();
        var outerBlockKey = Guid.NewGuid();

        Content content = SaveContent(
            schema,
            BuildOuterValueJson(schema, outerBlockKey, BuildNestedSingleBlockListJson(schema, innerBlockKey)));

        await ExecuteMigrationAsync();

        await AssertDataTypeIsSingleBlockAsync(schema.NestedDataType.Id);

        BlockItemData outerBlock = await GetStoredOuterBlockAsync(schema, content.Id, outerBlockKey);
        AssertNestedValueIsConvertedSingleBlock(schema, outerBlock, innerBlockKey);
    }

    [Test]
    public async Task Can_Migrate_Single_Block_List_Nested_In_Block_List_Stored_With_Pascal_Cased_Property_Names()
    {
        TestSchema schema = await CreateSchemaAsync();
        var innerBlockKey = Guid.NewGuid();
        var outerBlockKey = Guid.NewGuid();

        // Values that have not been re-saved since they were written by a pre-v14 (Newtonsoft) serializer carry
        // Pascal cased "Layout"/"ContentData" property names. JsonBlockValueConverter reads both spellings, so the
        // migration has to cope with them too.
        Content content = SaveContent(
            schema,
            ToPascalCasedPropertyNames(BuildOuterValueJson(
                schema,
                outerBlockKey,
                ToPascalCasedPropertyNames(BuildNestedSingleBlockListJson(schema, innerBlockKey)))));

        await ExecuteMigrationAsync();

        BlockItemData outerBlock = await GetStoredOuterBlockAsync(schema, content.Id, outerBlockKey);
        AssertNestedValueIsConvertedSingleBlock(schema, outerBlock, innerBlockKey);
    }

    [Test]
    public async Task Can_Migrate_Single_Block_List_Nested_Two_Levels_Deep()
    {
        TestSchema schema = await CreateSchemaAsync(addIntermediateLevel: true);
        var innerBlockKey = Guid.NewGuid();
        var intermediateBlockKey = Guid.NewGuid();
        var outerBlockKey = Guid.NewGuid();

        // blocks -> outer -> items (multiple) -> intermediate -> items (single) -> inner
        var intermediateJson = JsonSerializer.Serialize(BuildBlockListValue(
            intermediateBlockKey,
            schema.IntermediateElementType!,
            NestedPropertyAlias,
            BuildNestedSingleBlockListJson(schema, innerBlockKey)));

        Content content = SaveContent(schema, BuildOuterValueJson(schema, outerBlockKey, intermediateJson));

        await ExecuteMigrationAsync();

        BlockItemData outerBlock = await GetStoredOuterBlockAsync(schema, content.Id, outerBlockKey);
        var intermediateValue = JsonSerializer.Deserialize<BlockListValue>(GetNestedValueJson(outerBlock))!;
        BlockItemData intermediateBlock = intermediateValue.ContentData.Single(x => x.Key == intermediateBlockKey);

        AssertNestedValueIsConvertedSingleBlock(schema, intermediateBlock, innerBlockKey);
    }

    [Test]
    public async Task Can_Migrate_Single_Block_List_Used_Directly_On_A_Document_Type()
    {
        TestSchema schema = await CreateSchemaAsync();
        var innerBlockKey = Guid.NewGuid();

        // The single block mode Block List is the document type's own property, rather than nested in a block.
        IContentType pageContentType = await CreateContentTypeAsync(
            "topLevelPage",
            OuterPropertyAlias,
            schema.NestedDataType.Id,
            Constants.PropertyEditors.Aliases.BlockList);

        Content content = SaveContent(
            pageContentType,
            "Top level page",
            BuildNestedSingleBlockListJson(schema, innerBlockKey));

        await ExecuteMigrationAsync();

        var storedValue = await GetStoredValueAsync(content.Id, OuterPropertyAlias);
        Assert.That(storedValue, Is.Not.Null.And.Not.Empty);

        AssertIsInnerSingleBlock(schema, JsonSerializer.Deserialize<SingleBlockValue>(storedValue!), innerBlockKey);
    }

    [Test]
    public async Task Can_Migrate_Single_Block_List_Without_Losing_Sibling_Values()
    {
        TestSchema schema = await CreateSchemaAsync();
        var innerBlockKey = Guid.NewGuid();
        var outerBlockKey = Guid.NewGuid();

        Content content = SaveContent(
            schema,
            BuildOuterValueJson(
                schema,
                outerBlockKey,
                BuildNestedSingleBlockListJson(schema, innerBlockKey),
                addTextValue: true));

        await ExecuteMigrationAsync();

        BlockItemData outerBlock = await GetStoredOuterBlockAsync(schema, content.Id, outerBlockKey);

        Assert.That(
            outerBlock.Values.Single(x => x.Alias == TextPropertyAlias).Value,
            Is.EqualTo(OuterTextValue));
    }

    [Test]
    public async Task Can_Migrate_Empty_Single_Block_List_Without_Failing()
    {
        TestSchema schema = await CreateSchemaAsync();
        var outerBlockKey = Guid.NewGuid();

        // A single block mode Block List holding no block at all: neither the layout lookup nor the access of the
        // first layout item in the conversion may throw.
        Content content = SaveContent(
            schema,
            BuildOuterValueJson(schema, outerBlockKey, JsonSerializer.Serialize(new BlockListValue())));

        await ExecuteMigrationAsync();

        // There is nothing to convert, so the value is left as it was - but the upgrade completes and the containing
        // block is still there.
        BlockItemData outerBlock = await GetStoredOuterBlockAsync(schema, content.Id, outerBlockKey);
        Assert.That(outerBlock.Values.Select(x => x.Alias), Does.Contain(NestedPropertyAlias));
    }

    private async Task<TestSchema> CreateSchemaAsync(
        ContainerEditor containerEditor = ContainerEditor.BlockList,
        bool addIntermediateLevel = false)
    {
        IContentType innerElementType = await CreateElementTypeAsync("inner", TextPropertyAlias, null, null);

        IDataType nestedDataType = await CreateSingleBlockModeBlockListDataTypeAsync(innerElementType.Key);

        IContentType? intermediateElementType = null;
        var outerNestedDataTypeId = nestedDataType.Id;
        if (addIntermediateLevel)
        {
            intermediateElementType = await CreateElementTypeAsync(
                "intermediate",
                NestedPropertyAlias,
                nestedDataType.Id,
                Constants.PropertyEditors.Aliases.BlockList);

            IDataType intermediateDataType = await CreateBlockListDataTypeAsync(
                "Intermediate List",
                intermediateElementType.Key,
                singleBlockMode: false);

            outerNestedDataTypeId = intermediateDataType.Id;
        }

        IContentType outerElementType = await CreateElementTypeAsync(
            "outer",
            NestedPropertyAlias,
            outerNestedDataTypeId,
            Constants.PropertyEditors.Aliases.BlockList,
            addTextProperty: true);

        IDataType containerDataType = await CreateContainerDataTypeAsync(containerEditor, outerElementType.Key);

        IContentType pageContentType = await CreateContentTypeAsync(
            "page",
            OuterPropertyAlias,
            containerDataType.Id,
            containerDataType.EditorAlias);

        return new TestSchema(
            containerEditor,
            innerElementType,
            intermediateElementType,
            outerElementType,
            nestedDataType,
            pageContentType);
    }

    private async Task<IContentType> CreateElementTypeAsync(
        string alias,
        string propertyAlias,
        int? dataTypeId,
        string? propertyEditorAlias,
        bool addTextProperty = false)
    {
        IContentType elementType = BuildContentType(
            alias,
            propertyAlias,
            dataTypeId,
            propertyEditorAlias,
            addTextProperty);
        elementType.IsElement = true;

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Re-fetch so the property type's data type key is wired up.
        return (await ContentTypeService.GetAsync(elementType.Key))!;
    }

    private Task<IDataType> CreateSingleBlockModeBlockListDataTypeAsync(Guid elementTypeKey)
        => CreateBlockListDataTypeAsync("Inner Single", elementTypeKey, singleBlockMode: true);

    private async Task<IDataType> CreateBlockListDataTypeAsync(
        string name,
        Guid elementTypeKey,
        bool singleBlockMode)
    {
        var configurationData = new Dictionary<string, object>
        {
            {
                "blocks",
                new[] { new BlockListConfiguration.BlockConfiguration { ContentElementTypeKey = elementTypeKey } }
            },
        };

        if (singleBlockMode)
        {
            configurationData["useSingleBlockMode"] = true;
            configurationData["validationLimit"] = new BlockListConfiguration.NumberRange { Min = 1, Max = 1 };
        }

        return await CreateDataTypeAsync(name, Constants.PropertyEditors.Aliases.BlockList, configurationData);
    }

    private async Task<IDataType> CreateContainerDataTypeAsync(ContainerEditor containerEditor, Guid elementTypeKey)
        => containerEditor switch
        {
            ContainerEditor.BlockList => await CreateBlockListDataTypeAsync(
                "Outer List",
                elementTypeKey,
                singleBlockMode: false),
            ContainerEditor.BlockGrid => await CreateDataTypeAsync(
                "Outer Grid",
                Constants.PropertyEditors.Aliases.BlockGrid,
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
                }),
            ContainerEditor.RichText => await CreateDataTypeAsync(
                "Outer Rich Text",
                Constants.PropertyEditors.Aliases.RichText,
                new Dictionary<string, object>
                {
                    {
                        "blocks",
                        new[]
                        {
                            new RichTextConfiguration.RichTextBlockConfiguration
                            {
                                ContentElementTypeKey = elementTypeKey,
                            },
                        }
                    },
                }),
            _ => throw new ArgumentOutOfRangeException(nameof(containerEditor)),
        };

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

    private async Task<IContentType> CreateContentTypeAsync(
        string alias,
        string propertyAlias,
        int dataTypeId,
        string propertyEditorAlias)
    {
        IContentType contentType = BuildContentType(alias, propertyAlias, dataTypeId, propertyEditorAlias);
        contentType.AllowedAsRoot = true;

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return (await ContentTypeService.GetAsync(contentType.Key))!;
    }

    private static IContentType BuildContentType(
        string alias,
        string propertyAlias,
        int? dataTypeId,
        string? propertyEditorAlias,
        bool addTextProperty = false)
    {
        PropertyTypeBuilder<ContentTypeBuilder> propertyTypeBuilder = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName(alias.ToFirstUpperInvariant())
            .AddPropertyType()
            .WithAlias(propertyAlias)
            .WithName(propertyAlias.ToFirstUpperInvariant());

        if (dataTypeId.HasValue && propertyEditorAlias is not null)
        {
            propertyTypeBuilder = propertyTypeBuilder
                .WithDataTypeId(dataTypeId.Value)
                .WithPropertyEditorAlias(propertyEditorAlias)
                .WithValueStorageType(ValueStorageType.Ntext);
        }

        ContentTypeBuilder contentTypeBuilder = propertyTypeBuilder.Done();

        if (addTextProperty)
        {
            contentTypeBuilder = contentTypeBuilder
                .AddPropertyType()
                .WithAlias(TextPropertyAlias)
                .WithName(TextPropertyAlias.ToFirstUpperInvariant())
                .Done();
        }

        IContentType contentType = contentTypeBuilder.Build();
        contentType.AllowedTemplates = [];

        return contentType;
    }

    private string BuildNestedSingleBlockListJson(TestSchema schema, Guid innerBlockKey)
        => JsonSerializer.Serialize(BuildBlockListValue(
            innerBlockKey,
            schema.InnerElementType,
            TextPropertyAlias,
            InnerTextValue));

    private string BuildOuterValueJson(
        TestSchema schema,
        Guid outerBlockKey,
        string nestedValueJson,
        bool addTextValue = false)
    {
        BlockItemData outerBlock = BuildBlockItemData(
            outerBlockKey,
            schema.OuterElementType,
            NestedPropertyAlias,
            nestedValueJson);

        if (addTextValue)
        {
            outerBlock.Values.Add(new BlockPropertyValue { Alias = TextPropertyAlias, Value = OuterTextValue });
        }

        switch (schema.ContainerEditor)
        {
            case ContainerEditor.BlockList:
                return JsonSerializer.Serialize(new BlockListValue
                {
                    Layout = BuildLayout(
                        Constants.PropertyEditors.Aliases.BlockList,
                        new BlockListLayoutItem { ContentKey = outerBlockKey }),
                    ContentData = [outerBlock],
                    Expose = [new BlockItemVariation(outerBlockKey, null, null)],
                });
            case ContainerEditor.BlockGrid:
                return JsonSerializer.Serialize(new BlockGridValue
                {
                    Layout = BuildLayout(
                        Constants.PropertyEditors.Aliases.BlockGrid,
                        new BlockGridLayoutItem { ContentKey = outerBlockKey, ColumnSpan = 12, RowSpan = 1 }),
                    ContentData = [outerBlock],
                    Expose = [new BlockItemVariation(outerBlockKey, null, null)],
                });
            case ContainerEditor.RichText:
                return JsonSerializer.Serialize(new RichTextEditorValue
                {
                    Markup = $"<p>Some markup</p><umb-rte-block data-content-key=\"{outerBlockKey:D}\"></umb-rte-block>",
                    Blocks = new RichTextBlockValue
                    {
                        Layout = BuildLayout(
                            Constants.PropertyEditors.Aliases.RichText,
                            new RichTextBlockLayoutItem { ContentKey = outerBlockKey }),
                        ContentData = [outerBlock],
                        Expose = [new BlockItemVariation(outerBlockKey, null, null)],
                    },
                });
            default:
                throw new ArgumentOutOfRangeException(nameof(schema));
        }
    }

    private static BlockListValue BuildBlockListValue(
        Guid blockKey,
        IContentType elementType,
        string propertyAlias,
        object? propertyValue)
        => new()
        {
            Layout = BuildLayout(
                Constants.PropertyEditors.Aliases.BlockList,
                new BlockListLayoutItem { ContentKey = blockKey }),
            ContentData = [BuildBlockItemData(blockKey, elementType, propertyAlias, propertyValue)],
            Expose = [new BlockItemVariation(blockKey, null, null)],
        };

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

    private static string ToPascalCasedPropertyNames(string json)
        => json
            .Replace("\"layout\":", "\"Layout\":")
            .Replace("\"contentData\":", "\"ContentData\":")
            .Replace("\"settingsData\":", "\"SettingsData\":")
            .Replace("\"expose\":", "\"Expose\":");

    private Content SaveContent(TestSchema schema, string propertyValue)
        => SaveContent(schema.PageContentType, "Page", propertyValue);

    private Content SaveContent(IContentType contentType, string name, string propertyValue)
    {
        // The value is set directly rather than through a value editor, so the database holds exactly the
        // pre-migration JSON the test built.
        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName(name)
            .Build();
        content.SetValue(OuterPropertyAlias, propertyValue);

        ContentService.Save(content);

        return content;
    }

    private async Task ExecuteMigrationAsync()
    {
        MigrationPlan plan = new MigrationPlan(nameof(MigrateSingleBlockListTests))
            .From(string.Empty)
            .To<MigrateSingleBlockList>("done");

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

    private async Task AssertDataTypeIsSingleBlockAsync(int dataTypeId)
    {
        using Cms.Infrastructure.Scoping.IScope scope = ScopeProvider.CreateScope();

        Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(dataType => dataType.NodeId == dataTypeId);

        DataTypeDto dto = await scope.Database.FirstAsync<DataTypeDto>(sql);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(dto.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleBlock));

            // The alias the backoffice registers the single block editor UI under - a data type left pointing at
            // anything else has no editor in the backoffice.
            Assert.That(dto.EditorUiAlias, Is.EqualTo("Umb.PropertyEditorUi.BlockSingle"));
        });
    }

    private async Task<BlockItemData> GetStoredOuterBlockAsync(TestSchema schema, int contentId, Guid outerBlockKey)
    {
        var storedValue = await GetStoredValueAsync(contentId, OuterPropertyAlias);
        Assert.That(storedValue, Is.Not.Null.And.Not.Empty, "The outer property value was overwritten with null.");

        BlockValue? outerValue = schema.ContainerEditor switch
        {
            ContainerEditor.BlockList => JsonSerializer.Deserialize<BlockListValue>(storedValue!),
            ContainerEditor.BlockGrid => JsonSerializer.Deserialize<BlockGridValue>(storedValue!),
            ContainerEditor.RichText => JsonSerializer.Deserialize<RichTextEditorValue>(storedValue!)?.Blocks,
            _ => throw new ArgumentOutOfRangeException(nameof(schema)),
        };

        Assert.That(outerValue, Is.Not.Null);

        return outerValue!.ContentData.SingleOrDefault(x => x.Key == outerBlockKey)
               ?? throw new AssertionException(
                   $"The block {outerBlockKey} is no longer present in the migrated value: {storedValue}");
    }

    private async Task<string?> GetStoredValueAsync(int contentId, string propertyAlias)
    {
        using Cms.Infrastructure.Scoping.IScope scope = ScopeProvider.CreateScope();

        Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
            .Select<PropertyDataDto>()
            .From<PropertyDataDto>()
            .InnerJoin<PropertyTypeDto>()
            .On<PropertyDataDto, PropertyTypeDto>(pd => pd.PropertyTypeId, pt => pt.Id)
            .InnerJoin<ContentVersionDto>()
            .On<PropertyDataDto, ContentVersionDto>(pd => pd.VersionId, cv => cv.Id)
            .Where<PropertyTypeDto>(pt => pt.Alias == propertyAlias)
            .Where<ContentVersionDto>(cv => cv.NodeId == contentId && cv.Current);

        List<PropertyDataDto> dtos = await scope.Database.FetchAsync<PropertyDataDto>(sql);
        scope.Complete();

        return dtos.Single().TextValue;
    }

    private void AssertNestedValueIsConvertedSingleBlock(
        TestSchema schema,
        BlockItemData containingBlock,
        Guid innerBlockKey)
    {
        var nestedJson = GetNestedValueJson(containingBlock);

        AssertIsInnerSingleBlock(schema, JsonSerializer.Deserialize<SingleBlockValue>(nestedJson), innerBlockKey);
    }

    private static string GetNestedValueJson(BlockItemData containingBlock)
    {
        var nestedValue = containingBlock.Values.Single(x => x.Alias == NestedPropertyAlias).Value;
        Assert.That(nestedValue, Is.Not.Null, "The nested value was overwritten with null by the migration.");

        var nestedJson = nestedValue as string;
        Assert.That(nestedJson, Is.Not.Null.And.Not.Empty);

        return nestedJson!;
    }

    private static void AssertIsInnerSingleBlock(TestSchema schema, SingleBlockValue? singleBlockValue, Guid innerBlockKey)
    {
        Assert.That(singleBlockValue, Is.Not.Null);

        SingleBlockLayoutItem[]? layoutItems = singleBlockValue!.GetLayouts()?.ToArray();
        Assert.That(
            layoutItems,
            Is.Not.Null,
            $"The value holds no \"{Constants.PropertyEditors.Aliases.SingleBlock}\" layout - it was not converted.");
        Assert.That(layoutItems!.Length, Is.EqualTo(1));
        Assert.That(layoutItems[0].ContentKey, Is.EqualTo(innerBlockKey));

        BlockItemData innerBlock = singleBlockValue.ContentData.Single();
        Assert.Multiple(() =>
        {
            Assert.That(innerBlock.Key, Is.EqualTo(innerBlockKey));
            Assert.That(innerBlock.ContentTypeKey, Is.EqualTo(schema.InnerElementType.Key));
            Assert.That(
                innerBlock.Values.Single(x => x.Alias == TextPropertyAlias).Value,
                Is.EqualTo(InnerTextValue));
            Assert.That(singleBlockValue.Expose.Select(x => x.ContentKey), Does.Contain(innerBlockKey));
        });
    }

    private enum ContainerEditor
    {
        BlockList,
        BlockGrid,
        RichText,
    }

    private sealed record TestSchema(
        ContainerEditor ContainerEditor,
        IContentType InnerElementType,
        IContentType? IntermediateElementType,
        IContentType OuterElementType,
        IDataType NestedDataType,
        IContentType PageContentType);

    private sealed class NoopDatabaseCacheRebuilder : IDatabaseCacheRebuilder
    {
        public Task<Attempt<DatabaseCacheRebuildResult>> RebuildAsync(bool useBackgroundThread)
            => Task.FromResult(Attempt.Succeed(DatabaseCacheRebuildResult.Success));

        public Task RebuildDatabaseCacheIfSerializerChangedAsync() => throw new NotSupportedException();
    }
}
