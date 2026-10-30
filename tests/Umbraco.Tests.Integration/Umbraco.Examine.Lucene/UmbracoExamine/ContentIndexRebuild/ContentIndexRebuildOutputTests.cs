using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine.ContentIndexRebuild;

/// <summary>
///     Pins the exact value sets a content index rebuild produces for multilingual content with nested block editor
///     values, so that changes to the indexing pipeline cannot alter the indexed output unnoticed.
/// </summary>
/// <remarks>
///     The expected output lives in the <c>Snapshots</c> folder. To deliberately change it, run these tests with the
///     <c>UMBRACO_TEST_UPDATE_SNAPSHOTS</c> environment variable set to <c>true</c> and review the resulting diff.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentIndexRebuildOutputTests : ContentIndexRebuildTestBase
{
    private static readonly string[] _cultures = ["en-US", "da-DK", "de-DE"];

    public static void ConfigureExplicitlyIndexEachNestedProperty(IUmbracoBuilder builder, object explicitlyIndexEachNestedProperty)
        => builder.Services.Configure<IndexingSettings>(settings =>
            settings.ExplicitlyIndexEachNestedProperty = (bool)explicitlyIndexEachNestedProperty);

    [TestCase(false, false, 1)]
    [TestCase(false, false, 1000)]
    [TestCase(false, true, 1)]
    [TestCase(false, true, 1000)]
    [TestCase(true, false, 1)]
    [TestCase(true, false, 1000)]
    [TestCase(true, true, 1)]
    [TestCase(true, true, 1000)]
    [ConfigureBuilderTestCase(ActionName = nameof(ConfigureExplicitlyIndexEachNestedProperty), IndexOfParameter = 1)]
    public async Task Rebuild_Produces_Expected_Value_Sets(bool publishedValuesOnly, bool explicitlyIndexEachNestedProperty, int batchSize)
    {
        await SeedAsync();

        CapturedIndex captured = Rebuild(publishedValuesOnly, batchSize);

        var snapshotName = $"{(publishedValuesOnly ? "published" : "internal")}-{(explicitlyIndexEachNestedProperty ? "explicit" : "summary")}-batch{batchSize}";
        AssertMatchesSnapshot(snapshotName, RenderSnapshot(captured.ValueSets));
    }

    [TestCase(1)]
    [TestCase(1000)]
    public async Task Paged_Descendants_Load_Expected_Property_Values(int pageSize)
    {
        await SeedAsync();

        var content = new List<IContent>();
        var pageIndex = 0;
        IContent[] page;
        do
        {
            page = ContentService.GetPagedDescendants(Constants.System.Root, pageIndex++, pageSize, out _).ToArray();
            content.AddRange(page);
        }
        while (page.Length == pageSize);

        AssertMatchesSnapshot("load-all", RenderSnapshot(content));
    }

    [TestCase(1)]
    [TestCase(1000)]
    public async Task Published_Paged_Descendants_Load_Expected_Property_Values(int pageSize)
    {
        await SeedAsync();

        IQuery<IContent> publishedQuery = GetRequiredService<IUmbracoDatabaseFactory>().SqlContext.Query<IContent>().Where(x => x.Published);
        var content = new List<IContent>();
        var pageIndex = 0;
        IContent[] page;
        do
        {
            page = ContentService.GetPagedDescendants(Constants.System.Root, pageIndex++, pageSize, out _, publishedQuery, Ordering.By("Path")).ToArray();
            content.AddRange(page);
        }
        while (page.Length == pageSize);

        AssertMatchesSnapshot("load-published", RenderSnapshot(content));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Rebuild_Indexes_Each_Document_Once_In_Batches_Of_The_Configured_Size(bool publishedValuesOnly)
    {
        var documentCount = await SeedAsync(publishedValuesOnly);

        CapturedIndex captured = Rebuild(publishedValuesOnly, batchSize: 2);

        Assert.Multiple(() =>
        {
            Assert.That(captured.ValueSets.Select(x => x.Id), Is.Unique);
            Assert.That(captured.ValueSets.Count(), Is.EqualTo(documentCount));
            Assert.That(captured.Batches.Select(x => x.ValueSets.Count), Has.All.LessThanOrEqualTo(2));
        });
    }

    /// <summary>
    ///     Seeds documents covering the block editor indexing paths, interleaving variant and invariant documents so that
    ///     both appear within the same batch.
    /// </summary>
    /// <returns>The number of documents a rebuild should index.</returns>
    private async Task<int> SeedAsync(bool publishedValuesOnly = false)
    {
        await CreateLanguagesAsync(_cultures.Length);
        IndexingSchema schema = await CreateSchemaAsync();

        // Culture variant blocks, block level variant blocks with an unexposed culture, and a rich text with blocks.
        // The draft differs from the published version.
        CreateVariantDocument(
            schema,
            "A variant page",
            _cultures,
            content =>
            {
                foreach (var culture in _cultures)
                {
                    content.SetValue("title", $"A title {culture}", culture);
                    content.SetValue("variantBlocks", BlockListJson(schema, $"A variantBlocks {culture}", new BlockSpec(3, 2, null)), culture);
                    content.SetValue("body", RichTextJson(schema, $"A body {culture}", new BlockSpec(1, 2, null)), culture);
                }

                content.SetValue("invariantBlocks", BlockListJson(schema, "A invariantBlocks", new BlockSpec(3, 2, _cultures, UnexposedCulture: "de-DE")));
                content.SetValue("invariantBody", RichTextJson(schema, "A invariantBody", new BlockSpec(1, 2, _cultures, UnexposedCulture: "da-DK")));
            },
            culturesToPublish: _cultures,
            setDraftValues: content =>
            {
                content.SetValue("title", "A title en-US draft", "en-US");
                content.SetValue("variantBlocks", BlockListJson(schema, "A variantBlocks da-DK draft", new BlockSpec(2, 1, null)), "da-DK");
            });

        // Invariant blocks and a block grid with an area, on an invariant document.
        CreateInvariantDocument(
            schema,
            "B invariant page",
            content =>
            {
                content.SetValue("title", "B title");
                content.SetValue("invariantBlocks", BlockListJson(schema, "B invariantBlocks", new BlockSpec(3, 2, null)));
                content.SetValue("grid", BlockGridJson(schema, "B grid", new BlockSpec(1, 2, null)));
            },
            publish: true);

        // Only some cultures created and published. The block values were stored before the element properties changed
        // variance: culture variant element properties hold values without a culture, and an invariant one holds a value
        // with a culture.
        CreateVariantDocument(
            schema,
            "C variant page",
            ["en-US", "da-DK"],
            content =>
            {
                content.SetValue("title", "C title en-US", "en-US");
                content.SetValue("title", "C title da-DK", "da-DK");
                content.SetValue("invariantBlocks", BlockListJson(schema, "C invariantBlocks", new BlockSpec(3, 2, null, InvariantValueWithCulture: "da-DK")));
            },
            culturesToPublish: ["en-US"]);

        // Never published.
        CreateInvariantDocument(
            schema,
            "D invariant page",
            content =>
            {
                content.SetValue("title", "D title");
                content.SetValue("invariantBlocks", BlockListJson(schema, "D invariantBlocks", new BlockSpec(2, 2, null)));
            },
            publish: false);

        // Invariant blocks without cultures on a variant document.
        CreateVariantDocument(
            schema,
            "E variant page",
            _cultures,
            content =>
            {
                foreach (var culture in _cultures)
                {
                    content.SetValue("title", $"E title {culture}", culture);
                }

                content.SetValue("invariantBlocks", BlockListJson(schema, "E invariantBlocks", new BlockSpec(1, 3, null)));
                content.SetValue("invariantBody", RichTextJson(schema, "E invariantBody", new BlockSpec(1, 1, null)));
            },
            culturesToPublish: _cultures);

        // Invariant blocks on an invariant document, indexed after variant documents that use the same element types.
        CreateInvariantDocument(
            schema,
            "F invariant page",
            content =>
            {
                content.SetValue("title", "F title");
                content.SetValue("invariantBlocks", BlockListJson(schema, "F invariantBlocks", new BlockSpec(3, 2, null)));
            },
            publish: true);

        // Nested block values stored as serialized JSON strings rather than JSON objects.
        CreateVariantDocument(
            schema,
            "G variant page",
            _cultures,
            content =>
            {
                foreach (var culture in _cultures)
                {
                    content.SetValue("title", $"G title {culture}", culture);
                    content.SetValue("variantBlocks", BlockListJson(schema, $"G variantBlocks {culture}", new BlockSpec(3, 1, null, NestedValuesAsStrings: true)), culture);
                }

                content.SetValue("invariantBlocks", BlockListJson(schema, "G invariantBlocks", new BlockSpec(3, 1, _cultures, NestedValuesAsStrings: true)));
            },
            culturesToPublish: _cultures);

        return publishedValuesOnly ? 6 : 7;
    }
}
