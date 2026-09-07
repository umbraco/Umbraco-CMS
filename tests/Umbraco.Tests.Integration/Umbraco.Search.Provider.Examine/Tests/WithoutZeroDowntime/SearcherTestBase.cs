using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Extensions;
using Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.WithoutZeroDowntime;

[TestFixture]
public abstract class SearcherTestBase
{
    private ServiceProvider _serviceProvider;

    private const string IndexAlias = global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;
    protected const string FieldMultipleValues = "fieldMultipleValues";
    protected const string FieldSingleValue = "fieldSingleValues";

    protected Dictionary<int, Guid> DocumentIds { get; } = [];

    [OneTimeSetUp]
    protected async Task PerformOneTimeSetUpAsync()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddExamineSearchProviderServicesWithoutZeroDowntimeForTest<TestIndex, TestInMemoryDirectoryFactory>()
            .AddLogging();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        await GetRequiredService<IIndexer>().ResetAsync(IndexAlias);

        IIndexer indexer = GetRequiredService<IIndexer>();

        for (var i = 1; i <= 10; i++)
        {
            var id = Guid.NewGuid();
            DocumentIds[i] = id;

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
                    new IndexField(
                        FieldMultipleValues,
                        new IndexValue
                        {
                            Keywords = ["all", i % 2 == 0 ? "even" : "odd", $"single{i}"],
                            Texts = [$"document{i}"]
                        },
                        Culture: null,
                        Segment: null),
                    new IndexField(
                        FieldSingleValue,
                        new IndexValue
                        {
                            Keywords = [$"single{i}"],
                            Integers = [i]
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
        await GetRequiredService<IIndexer>().ResetAsync(IndexAlias);

        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    protected async Task<SearchResult> SearchAsync(
        string? query = null,
        IEnumerable<Filter>? filters = null,
        IEnumerable<Facet>? facets = null,
        IEnumerable<Sorter>? sorters = null,
        int skip = 0,
        int take = 100)
    {
        ISearcher searcher = GetRequiredService<ISearcher>();
        SearchResult result = await searcher.SearchAsync(
            IndexAlias,
            query,
            filters,
            facets,
            sorters,
            culture: null,
            segment: null,
            accessContext: null,
            skip,
            take);

        Assert.That(result, Is.Not.Null);
        return result;
    }

    protected T GetRequiredService<T>() where T : notnull
        => _serviceProvider.GetRequiredService<T>();
}
