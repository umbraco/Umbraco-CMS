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
using Umbraco.Tests.Search.Examine.Integration.Extensions;
using Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

namespace Umbraco.Tests.Search.Examine.Integration.Tests;

[TestFixture]
public abstract class SearcherTestBase
{
    private ServiceProvider _serviceProvider;

    private const string IndexAlias = Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;
    protected const string FieldMultipleValues = "fieldMultipleValues";
    protected const string FieldSingleValue = "fieldSingleValues";
    protected const string FieldMultiSorting = "FieldThree";
    protected const string FieldTextRelevance = "FieldFour";

    protected Dictionary<int, Guid> DocumentIds { get; } = [];

    [OneTimeSetUp]
    protected async Task PerformOneTimeSetUpAsync()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddExamineSearchProviderServicesForTest<TestIndex, TestInMemoryDirectoryFactory>()
            .AddLogging();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        await EnsureIndex();

        IIndexer indexer = GetRequiredService<IIndexer>();

        for (var i = 1; i <= 100; i++)
        {
            var id = Guid.NewGuid();
            DocumentIds[i] = id;

            await indexer.AddOrUpdateAsync(
                IndexAlias,
                id,
                i <= 25
                    ? UmbracoObjectTypes.Document
                    : i <= 50
                        ? UmbracoObjectTypes.Media
                        : i <= 75
                            ? UmbracoObjectTypes.Member
                            : UmbracoObjectTypes.Unknown,
                [new Variation(Culture: null, Segment: null)],
                [
                    new IndexField(
                        Constants.FieldNames.PathIds,
                        new IndexValue { Keywords = [id.AsKeyword()], },
                        Culture: null,
                        Segment: null),
                    new IndexField(
                        FieldMultipleValues,
                        new IndexValue
                        {
                            Decimals = [i, i * 1.5m, i * -1m, i * -1.5m],
                            Integers = [i, i * 10, i * -1],
                            Keywords = ["all", i % 2 == 0 ? "even" : "odd", $"single{i}", $"common single{i}"],
                            DateTimeOffsets =
                            [
                                Date(2025, 01, 01),
                                StartDate().AddDays(i),
                                StartDate().AddDays(i * 2)
                            ],
                            Texts = ["all", i % 2 == 0 ? "even" : "odd", $"single{i}", $"phrase search single{i}"]
                        },
                        Culture: null,
                        Segment: null),
                    new IndexField(
                        FieldSingleValue,
                        new IndexValue
                        {
                            Decimals = [i],
                            Integers = [i],
                            Keywords = [$"single{i}"],
                            DateTimeOffsets = [StartDate().AddDays(i)],
                            Texts = [$"single{i}"]
                        },
                        Culture: null,
                        Segment: null),
                    new IndexField(
                        FieldMultiSorting,
                        new IndexValue
                        {
                            Decimals = [i % 2 == 0 ? 10m : 20m],
                            Integers = [i],
                            Keywords = [i % 2 == 0 ? "even" : "odd"],
                            DateTimeOffsets = [i % 2 == 0 ? StartDate().AddDays(1) : StartDate().AddDays(2)]
                        },
                        Culture: null,
                        Segment: null),
                    new IndexField(
                        FieldTextRelevance,
                        new IndexValue
                        {
                            Texts = [$"texts_{i}", i == 10 ? "special" : "common"],
                            TextsR1 = [$"texts_r1_{i}", i == 30 ? "special" : "common"],
                            TextsR2 = [$"texts_r2_{i}", i == 20 ? "special" : "common"],
                            TextsR3 = [$"texts_r3_{i}", i == 40 ? "special" : "common"]
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

    protected async Task<SearchResult> SearchAsync(
        string? query = null,
        IEnumerable<Filter>? filters = null,
        IEnumerable<Facet>? facets = null,
        IEnumerable<Sorter>? sorters = null,
        string? culture = null,
        string? segment = null,
        AccessContext? accessContext = null,
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
            culture,
            segment,
            accessContext,
            skip,
            take);

        Assert.That(result, Is.Not.Null);
        return result;
    }

    private async Task EnsureIndex() => await DeleteIndex();

    private async Task DeleteIndex()
        => await GetRequiredService<IIndexer>().ResetAsync(IndexAlias);

    protected DateTimeOffset StartDate()
        => Date(2025, 01, 01);

    private DateTimeOffset Date(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        => new(year, month, day, hour, minute, second, TimeSpan.Zero);

    protected int[] OddOrEvenIds(bool even)
        => Enumerable
            .Range(1, 50)
            .Select(i => i * 2)
            .Select(i => even ? i : i - 1)
            .ToArray();

    protected T GetRequiredService<T>() where T : notnull
        => _serviceProvider.GetRequiredService<T>();
}
