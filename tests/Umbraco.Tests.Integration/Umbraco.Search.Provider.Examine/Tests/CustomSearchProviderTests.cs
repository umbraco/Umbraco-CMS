using Examine;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Examine.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Provider.Examine.Configuration;
using Umbraco.Cms.Search.Provider.Examine.DependencyInjection;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;
using ISearcher = Umbraco.Cms.Search.Core.Services.ISearcher;
using SearchResult = Umbraco.Cms.Search.Core.Models.Searching.SearchResult;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

/// <summary>
/// Tests to verify that custom <see cref="IndexValue"/> subclasses can be indexed
/// using a custom <see cref="Indexer"/> and <see cref="Searcher"/> implementation.
/// </summary>
[TestFixture]
public class CustomSearchProviderTests
{
    private ServiceProvider _serviceProvider = null!;
    private const string IndexAlias = global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;
    private const string CustomGuidFieldName = "customGuids";

    private Dictionary<int, Guid> DocumentIds { get; } = [];
    private Dictionary<int, Guid[]> DocumentCustomGuids { get; } = [];

    [OneTimeSetUp]
    protected async Task PerformOneTimeSetUpAsync()
    {
        var serviceCollection = new ServiceCollection();

        // Use custom service registration that includes our custom indexer
        serviceCollection.AddCustomIndexerServicesForTest<TestIndex, TestInMemoryDirectoryFactory>();
        serviceCollection.AddLogging();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        await EnsureIndex();

        IIndexer indexer = GetRequiredService<IIndexer>();

        // Create test documents with custom IndexValue containing Guids
        for (var i = 1; i <= 10; i++)
        {
            var id = Guid.NewGuid();
            DocumentIds[i] = id;

            // Create some custom Guids for each document
            Guid[] customGuids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            DocumentCustomGuids[i] = customGuids;

            await indexer.AddOrUpdateAsync(
                IndexAlias,
                id,
                UmbracoObjectTypes.Document,
                [new Variation(Culture: null, Segment: null)],
                [
                    new IndexField(
                        Constants.FieldNames.PathIds,
                        new IndexValue { Keywords = [id.AsKeyword()] },
                        Culture: null,
                        Segment: null),
                    // Use our custom IndexValue with Guids property
                    new IndexField(
                        CustomGuidFieldName,
                        new CustomIndexValue
                        {
                            Keywords = [$"doc{i}"],
                            Guids = customGuids
                        },
                        Culture: null,
                        Segment: null),
                ],
                null);
        }

        await Task.Delay(3000);
    }

    [OneTimeTearDown]
    protected async Task PerformOneTimeTearDownAsync()
    {
        await DeleteIndex();

        if (_serviceProvider is IDisposable disposableServiceProvider)
        {
            disposableServiceProvider.Dispose();
        }
    }

    [Test]
    public void CustomIndexValueCanBeCreated()
    {
        // Verify that custom IndexValue can be created with additional properties
        var customValue = new CustomIndexValue
        {
            Keywords = ["test"],
            Guids = [Guid.NewGuid(), Guid.NewGuid()]
        };

        Assert.Multiple(() =>
        {
            Assert.That(customValue.Keywords, Has.Exactly(1).Items);
            Assert.That(customValue.Guids, Has.Exactly(2).Items);
        });
    }

    [Test]
    public void CustomIndexerIsResolved()
    {
        // Verify that our custom indexer is being used
        IIndexer indexer = GetRequiredService<IIndexer>();
        Assert.That(indexer, Is.InstanceOf<CustomIndexer>(), $"Expected CustomIndexer but got {indexer.GetType().FullName}");
    }

    [Test]
    public void CustomIndexValueInheritsFromIndexValue()
    {
        var customValue = new CustomIndexValue
        {
            Keywords = ["test"],
            Texts = ["some text"],
            Guids = [Guid.NewGuid()]
        };

        // Verify inheritance - can be assigned to IndexValue
        IndexValue baseValue = customValue;

        Assert.Multiple(() =>
        {
            Assert.That(baseValue.Keywords, Is.EqualTo(customValue.Keywords));
            Assert.That(baseValue.Texts, Is.EqualTo(customValue.Texts));
        });
    }

    [Test]
    public void CustomGuidsAreIndexed()
    {
        // Get the Examine index directly to verify custom fields were indexed
        IIndex index = GetPhysicalIndex(IndexAlias);

        // Search for a document by its custom guid
        Guid targetGuid = DocumentCustomGuids[1][0];
        var fieldName = $"Field_{CustomGuidFieldName}_guids_keywords";

        ISearchResults results = index.Searcher
            .CreateQuery()
            .Field(fieldName, targetGuid.ToString())
            .Execute();

        Assert.That(results.TotalItemCount, Is.EqualTo(1), $"Expected to find 1 document with guid {targetGuid}");
    }

    [Test]
    public void CustomGuidsFieldIsIndexed()
    {
        // Verify that the custom guids field exists in the index
        IIndex index = GetPhysicalIndex(IndexAlias);

        // Get all documents
        ISearchResults allResults = index.Searcher.CreateQuery().All().Execute();

        Assert.That(allResults.TotalItemCount, Is.GreaterThan(0), "Index should have documents");

        // Get first document and check for the custom guids field
        ISearchResult firstDoc = allResults.First();
        var fieldNames = firstDoc.AllValues.Keys.ToList();

        var expectedFieldName = $"Field_{CustomGuidFieldName}_guids_keywords";
        Assert.That(fieldNames, Does.Contain(expectedFieldName), $"Expected field '{expectedFieldName}' not found. Available fields: {string.Join(", ", fieldNames)}");
    }

    [Test]
    public void AllDocumentsHaveCustomGuidFieldIndexed()
    {
        IIndex index = GetPhysicalIndex(IndexAlias);

        // Verify each document has its custom guids indexed
        foreach (KeyValuePair<int, Guid[]> kvp in DocumentCustomGuids)
        {
            var docNumber = kvp.Key;
            Guid[] expectedGuids = kvp.Value;

            foreach (Guid expectedGuid in expectedGuids)
            {
                var fieldName = $"Field_{CustomGuidFieldName}_guids_keywords";
                ISearchResults results = index.Searcher
                    .CreateQuery()
                    .Field(fieldName, expectedGuid.ToString())
                    .Execute();

                Assert.That(
                    results.TotalItemCount,
                    Is.EqualTo(1),
                    $"Document {docNumber} should have guid {expectedGuid} indexed");
            }
        }
    }

    [Test]
    public void CanSearchForDocumentByCustomGuid()
    {
        IIndex index = GetPhysicalIndex(IndexAlias);

        // Pick a random document and search for it by its custom guid
        var targetDocNumber = 5;
        Guid targetGuid = DocumentCustomGuids[targetDocNumber][0];
        Guid expectedDocId = DocumentIds[targetDocNumber];

        var fieldName = $"Field_{CustomGuidFieldName}_guids_keywords";
        ISearchResults results = index.Searcher
            .CreateQuery()
            .Field(fieldName, targetGuid.ToString())
            .Execute();

        Assert.Multiple(() =>
        {
            Assert.That(results.TotalItemCount, Is.EqualTo(1));

            ISearchResult result = results.First();
            // The document ID in Examine is the guid (lowercase)
            Assert.That(result.Id, Is.EqualTo(expectedDocId.ToString().ToLowerInvariant()));
        });
    }

    [Test]
    public void CustomSearcherIsResolved()
    {
        // Verify that our custom searcher is being used
        ISearcher searcher = GetRequiredService<ISearcher>();
        Assert.That(searcher, Is.InstanceOf<CustomSearcher>(), $"Expected CustomSearcher but got {searcher.GetType().FullName}");
    }

    [Test]
    public async Task CanFilterDocumentsUsingCustomGuidFilter()
    {
        // Use the custom searcher with a custom GuidFilter
        ISearcher searcher = GetRequiredService<ISearcher>();

        // Search for document 3 using its custom guid
        Guid targetGuid = DocumentCustomGuids[3][0];
        Guid expectedDocId = DocumentIds[3];

        SearchResult result = await searcher.SearchAsync(
            IndexAlias,
            query: null,
            filters: [new GuidFilter(CustomGuidFieldName, [targetGuid], Negate: false)],
            facets: null,
            sorters: null,
            culture: null,
            segment: null,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Documents.First().Id, Is.EqualTo(expectedDocId));
        });
    }

    [Test]
    public async Task CanFilterDocumentsUsingMultipleGuidsInCustomFilter()
    {
        ISearcher searcher = GetRequiredService<ISearcher>();

        // Search for documents 2 and 7 using their custom guids
        Guid guid2 = DocumentCustomGuids[2][0];
        Guid guid7 = DocumentCustomGuids[7][1]; // Use second guid for variety

        SearchResult result = await searcher.SearchAsync(
            IndexAlias,
            query: null,
            filters: [new GuidFilter(CustomGuidFieldName, [guid2, guid7], Negate: false)],
            facets: null,
            sorters: null,
            culture: null,
            segment: null,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Documents.Select(d => d.Id), Does.Contain(DocumentIds[2]));
            Assert.That(result.Documents.Select(d => d.Id), Does.Contain(DocumentIds[7]));
        });
    }

    [Test]
    public async Task CanFilterDocumentsUsingNegatedCustomGuidFilter()
    {
        ISearcher searcher = GetRequiredService<ISearcher>();

        // Search for all documents EXCEPT document 1
        Guid excludeGuid = DocumentCustomGuids[1][0];

        SearchResult result = await searcher.SearchAsync(
            IndexAlias,
            query: null,
            filters: [new GuidFilter(CustomGuidFieldName, [excludeGuid], Negate: true)],
            facets: null,
            sorters: null,
            culture: null,
            segment: null,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(9)); // 10 total - 1 excluded = 9
            Assert.That(result.Documents.Select(d => d.Id), Does.Not.Contain(DocumentIds[1]));
        });
    }

    [Test]
    public async Task CustomFilterReturnsNoResultsForNonExistentGuid()
    {
        ISearcher searcher = GetRequiredService<ISearcher>();

        // Search for a guid that doesn't exist
        Guid nonExistentGuid = Guid.NewGuid();

        SearchResult result = await searcher.SearchAsync(
            IndexAlias,
            query: null,
            filters: [new GuidFilter(CustomGuidFieldName, [nonExistentGuid], Negate: false)],
            facets: null,
            sorters: null,
            culture: null,
            segment: null,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(result.Total, Is.EqualTo(0));
    }

    private async Task EnsureIndex() => await DeleteIndex();

    private async Task DeleteIndex()
        => await GetRequiredService<IIndexer>().ResetAsync(IndexAlias);

    private IIndex GetPhysicalIndex(string indexAlias)
    {
        var activeIndexManager = GetRequiredService<IActiveIndexManager>();
        var physicalName = activeIndexManager.ResolveActiveIndexName(indexAlias);
        return GetRequiredService<IExamineManager>().GetIndex(physicalName);
    }

    private T GetRequiredService<T>() where T : notnull
        => _serviceProvider.GetRequiredService<T>();
}

/// <summary>
/// A custom IndexValue subclass that adds support for indexing Guids.
/// This demonstrates the extensibility of the IndexValue record.
/// </summary>
public record CustomIndexValue : IndexValue
{
    /// <summary>
    /// Collection of Guids to be indexed.
    /// </summary>
    public IEnumerable<Guid>? Guids { get; init; }
}

/// <summary>
/// A custom Indexer that handles <see cref="CustomIndexValue"/> by indexing the Guids property.
/// </summary>
public class CustomIndexer : Indexer
{
    // Use "keywords" suffix so the guids are indexed as RAW (unanalyzed) for exact matching
    private const string GuidsFieldSuffix = "keywords";

    public CustomIndexer(IExamineManager examineManager, IOptions<FieldOptions> fieldOptions, IActiveIndexManager activeIndexManager)
        : base(examineManager, fieldOptions, activeIndexManager)
    {
    }

    protected override IndexValue MergeIndexValue(IndexValue original, IndexValue toMerge)
    {
        // First merge the base properties
        IndexValue baseMerged = base.MergeIndexValue(original, toMerge);

        // Then handle our custom Guids property
        var originalCustom = original as CustomIndexValue;
        var toMergeCustom = toMerge as CustomIndexValue;

        // If neither has custom guids, return the base merge result
        if (originalCustom?.Guids is null && toMergeCustom?.Guids is null)
        {
            return baseMerged;
        }

        // Create a CustomIndexValue with merged guids
        return new CustomIndexValue
        {
            Keywords = baseMerged.Keywords,
            Integers = baseMerged.Integers,
            Decimals = baseMerged.Decimals,
            DateTimeOffsets = baseMerged.DateTimeOffsets,
            Texts = baseMerged.Texts,
            TextsR1 = baseMerged.TextsR1,
            TextsR2 = baseMerged.TextsR2,
            TextsR3 = baseMerged.TextsR3,
            Guids = MergeValues(originalCustom?.Guids, toMergeCustom?.Guids)
        };
    }

    protected override void AppendCustomIndexValues(IndexField field, Dictionary<string, IEnumerable<object>> result)
    {
        if (field.Value is CustomIndexValue customValue && customValue.Guids?.Any() == true)
        {
            // Index the Guids as keyword values (RAW/unanalyzed) for exact matching
            // We use a distinct field name to avoid conflicts with regular Keywords
            var fieldName = FieldNameHelper.FieldName($"{field.FieldName}_guids", GuidsFieldSuffix, field.Segment);
            result.Add(fieldName, customValue.Guids.Select(g => g.ToString()).ToList());
        }
    }
}

/// <summary>
/// Extension methods for setting up test services with the custom indexer.
/// </summary>
internal static class CustomIndexerServiceCollectionExtensions
{
    public static IServiceCollection AddCustomIndexerServicesForTest<TIndex, TDirectoryFactory>(
        this IServiceCollection services)
        where TIndex : LuceneIndex
        where TDirectoryFactory : class, IDirectoryFactory
    {
        // Configure base test options plus our custom field
        services.ConfigureOptions<TestIndexConfigureOptions>();
        services.ConfigureOptions<CustomIndexValueFieldOptions>();
        services.Configure<SearcherOptions>(options => options.MaxFacetValues = 250);
        services.AddSingleton<TDirectoryFactory>();

        // Register dual indexes (_a and _b) per logical alias for zero-downtime reindexing
        string[] aliases =
        [
            global::Umbraco.Cms.Core.Constants.IndexAliases.DraftContent,
            global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent,
            global::Umbraco.Cms.Core.Constants.IndexAliases.DraftMedia,
            global::Umbraco.Cms.Core.Constants.IndexAliases.DraftMembers,
        ];
        foreach (var alias in aliases)
        {
            services.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(alias + ActiveIndexManager.SuffixA, _ => { });
            services.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(alias + ActiveIndexManager.SuffixB, _ => { });
        }

        // Add base Examine services first
        services.AddExamineSearchProviderServices();

        // Override to use ActiveIndexManager for zero-downtime reindexing in integration tests
        services.AddSingleton<IActiveIndexManager, ActiveIndexManager>();

        // Override with our custom indexer and searcher
        services.AddTransient<IExamineIndexer, CustomIndexer>();
        services.AddTransient<IIndexer, CustomIndexer>();
        services.AddTransient<IExamineSearcher, CustomSearcher>();
        services.AddTransient<ISearcher, CustomSearcher>();

        return services;
    }
}

/// <summary>
/// Configures field options for the custom guids field.
/// </summary>
internal class CustomIndexValueFieldOptions : IConfigureOptions<FieldOptions>
{
    public void Configure(FieldOptions fieldOptions)
    {
        // Add our custom guids field configuration
        // The field name is "customGuids_guids" with Keywords field type
        var existingFields = fieldOptions.Fields.ToList();
        existingFields.Add(new FieldOptions.Field
        {
            PropertyName = "customGuids_guids",
            FieldValues = FieldValues.Keywords,
        });
        fieldOptions.Fields = existingFields.ToArray();
    }
}

/// <summary>
/// A custom Filter that filters by Guid values.
/// This demonstrates the extensibility of the Filter model.
/// </summary>
public record GuidFilter(string FieldName, Guid[] Values, bool Negate)
    : Filter(FieldName, Negate)
{
}

/// <summary>
/// A custom Searcher that handles <see cref="GuidFilter"/> by querying the custom guids field.
/// This demonstrates the extensibility of the Searcher class.
/// </summary>
public class CustomSearcher : Searcher
{
    public CustomSearcher(IExamineManager examineManager, IOptions<SearcherOptions> searcherOptions, IActiveIndexManager activeIndexManager)
        : base(examineManager, searcherOptions, activeIndexManager)
    {
    }

    protected override void AddCustomFilter(IBooleanOperation searchQuery, Filter filter, string? culture, string? segment)
    {
        if (filter is GuidFilter guidFilter)
        {
            // Build the field name for the custom guids field
            // The field was indexed as: Field_{fieldName}_guids_keywords
            var fieldName = FieldNameHelper.FieldName($"{filter.FieldName}_guids", "keywords", segment);

            // Convert guids to strings for the query
            var guidStrings = guidFilter.Values.Select(g => g.ToString()).ToArray();

            if (guidFilter.Negate)
            {
                searchQuery.Not().GroupedOr([fieldName], guidStrings);
            }
            else
            {
                searchQuery.And().GroupedOr([fieldName], guidStrings);
            }
        }
    }
}
