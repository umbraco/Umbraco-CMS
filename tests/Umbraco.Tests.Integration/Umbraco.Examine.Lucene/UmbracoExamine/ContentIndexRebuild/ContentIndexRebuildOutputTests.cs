using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
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

    private static readonly string[] _allSeededProperties = ["title", "variantBlocks", "invariantBlocks", "body", "invariantBody", "grid"];

    private static readonly string[] _propertiesForIncludedFields = ["title", "invariantBlocks", "body"];

    // Every document in its own batch, documents split across batches, and all documents in one batch.
    private static readonly int[] _batchSizes = [1, 2, 1000];

    public static void ConfigureExplicitlyIndexEachNestedProperty(IUmbracoBuilder builder, object explicitlyIndexEachNestedProperty)
        => builder.Services.Configure<IndexingSettings>(settings =>
            settings.ExplicitlyIndexEachNestedProperty = (bool)explicitlyIndexEachNestedProperty);

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    [ConfigureBuilderTestCase(ActionName = nameof(ConfigureExplicitlyIndexEachNestedProperty), IndexOfParameter = 1)]
    public async Task Rebuild_Produces_Expected_Value_Sets_For_Any_Batch_Size(bool publishedValuesOnly, bool explicitlyIndexEachNestedProperty)
    {
        await SeedAsync();

        var snapshotName = $"{(publishedValuesOnly ? "published" : "internal")}-{(explicitlyIndexEachNestedProperty ? "explicit" : "summary")}";
        foreach (var batchSize in _batchSizes)
        {
            TestContext.Out.WriteLine($"Rebuilding with batch size {batchSize}.");
            CapturedIndex captured = Rebuild(publishedValuesOnly, batchSize);
            AssertMatchesSnapshot(snapshotName, RenderSnapshot(captured.ValueSets));
        }
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
            page = ContentService.GetPagedDescendants(Constants.System.Root, pageIndex++, pageSize, out _, propertyAliases: null, filter: null, ordering: null).ToArray();
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
            page = ContentService.GetPagedDescendants(Constants.System.Root, pageIndex++, pageSize, out _, propertyAliases: null, filter: publishedQuery, ordering: Ordering.By("Path")).ToArray();
            content.AddRange(page);
        }
        while (page.Length == pageSize);

        AssertMatchesSnapshot("load-published", RenderSnapshot(content));
    }

    [Test]
    public async Task Paged_Descendants_With_More_Property_Aliases_Than_Query_Parameters_Load_The_Requested_Properties()
    {
        await SeedAsync();
        string[] propertyAliases = ["title", .. Enumerable.Range(0, Constants.Sql.MaxParameterCount).Select(i => $"unknown{i}")];

        IContent[] content = ContentService.GetPagedDescendants(Constants.System.Root, 0, 1000, out _, propertyAliases, filter: null, ordering: null).ToArray();

        Assert.That(content, Is.Not.Empty);
        Assert.That(PropertiesWithValues(content), Does.Contain("title"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Rebuild_Of_Index_Including_Specific_Fields_Loads_Only_Properties_For_Those_Fields_When_Enabled(bool publishedValuesOnly)
    {
        await SeedAsync();
        var validator = new ValueSetValidator(null, null, ["nodeName", "title", "title_en-us", "title_da-dk", "invariantBlocks", "invariantBlocks_de-de", "__Raw_body_en-us", "body_en-us"], null);

        CapturedIndex unfiltered = Rebuild(publishedValuesOnly, batchSize: 2);
        CapturedIndex filtered = Rebuild(publishedValuesOnly, batchSize: 2, validator: validator, loadOnlyPropertiesForIncludedFields: true);
        CapturedIndex notEnabled = Rebuild(publishedValuesOnly, batchSize: 2, validator: validator);

        var expected = RenderSnapshot(unfiltered.ValueSets.Select(valueSet => validator.Validate(valueSet).ValueSet));
        Assert.Multiple(() =>
        {
            Assert.That(expected, Does.Contain("invariantBlocks_de-de = ").And.Contain("body_en-us = ").And.Contain("title = "));
            Assert.That(RenderSnapshot(filtered.ValueSets), Is.EqualTo(expected));
            Assert.That(PropertiesWithValues(unfiltered.LoadedContent), Is.EquivalentTo(_allSeededProperties));
            Assert.That(PropertiesWithValues(filtered.LoadedContent), Is.EquivalentTo(_propertiesForIncludedFields));
            Assert.That(RenderSnapshot(notEnabled.ValueSets), Is.EqualTo(expected));
            Assert.That(PropertiesWithValues(notEnabled.LoadedContent), Is.EquivalentTo(PropertiesWithValues(unfiltered.LoadedContent)));
        });
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

    private static string[] PropertiesWithValues(IEnumerable<IContent> content)
        => content
            .SelectMany(item => item.Properties)
            .Where(property => property.Values.Any(value => value.EditedValue is not null || value.PublishedValue is not null))
            .Select(property => property.Alias)
            .Distinct()
            .ToArray();

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
