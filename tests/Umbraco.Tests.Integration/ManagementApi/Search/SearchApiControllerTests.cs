using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Search.Querying.Faceting;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Search.Querying.Sorting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Search;

public class SearchApiControllerTests : ManagementApiTest<SearchApiController>
{
    private const string IndexAlias = "TestIndex";

    private static readonly DateTimeOffset _date = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    private readonly CapturingSearcher _searcher = new();

    protected override Expression<Func<SearchApiController, object>> MethodSelector { get; set; }
        = x => x.Search(null!, 0, 100);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.Services.AddUnique<ISearcherResolver>(new SearcherResolver(_searcher));

    [SetUp]
    public async Task Authenticate()
    {
        _searcher.Reset();
        await AuthenticateClientAsync(Client, "admin@umbraco.com", UserPassword, true);
    }

    [Test]
    public async Task Search_Passes_Every_Filter_Type_To_The_Searcher()
    {
        HttpResponseMessage response = await PostAsync(
            """
            {
              "indexAlias": "TestIndex",
              "filters": [
                { "$type": "KeywordFilterRequestModel", "fieldName": "keyword", "values": ["one", "two"], "negate": false },
                { "$type": "TextFilterRequestModel", "fieldName": "text", "values": ["some text"], "negate": true },
                { "$type": "IntegerExactFilterRequestModel", "fieldName": "integerExact", "values": [1, 2], "negate": false },
                { "$type": "DecimalExactFilterRequestModel", "fieldName": "decimalExact", "values": [1.5], "negate": false },
                { "$type": "DateTimeOffsetExactFilterRequestModel", "fieldName": "dateExact", "values": ["2026-10-07T12:00:00+00:00"], "negate": false },
                { "$type": "IntegerRangeFilterRequestModel", "fieldName": "integerRange", "ranges": [{ "minValue": 1, "maxValue": 10 }, { "minValue": null, "maxValue": 0 }], "negate": false },
                { "$type": "DecimalRangeFilterRequestModel", "fieldName": "decimalRange", "ranges": [{ "minValue": 0.5, "maxValue": null }], "negate": true },
                { "$type": "DateTimeOffsetRangeFilterRequestModel", "fieldName": "dateRange", "ranges": [{ "minValue": "2026-10-07T12:00:00+00:00", "maxValue": null }], "negate": false }
              ]
            }
            """);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        Filter[] filters = _searcher.Filters!.ToArray();
        Assert.AreEqual(8, filters.Length);
        Assert.Multiple(() =>
        {
            AssertExactFilter<KeywordFilter, string>(filters[0], "keyword", ["one", "two"], false);
            AssertExactFilter<TextFilter, string>(filters[1], "text", ["some text"], true);
            AssertExactFilter<IntegerExactFilter, int>(filters[2], "integerExact", [1, 2], false);
            AssertExactFilter<DecimalExactFilter, decimal>(filters[3], "decimalExact", [1.5m], false);
            AssertExactFilter<DateTimeOffsetExactFilter, DateTimeOffset>(filters[4], "dateExact", [_date], false);

            var integerRange = (IntegerRangeFilter)filters[5];
            Assert.AreEqual("integerRange", integerRange.FieldName);
            CollectionAssert.AreEqual(new[] { new IntegerRangeFilterRange(1, 10), new IntegerRangeFilterRange(null, 0) }, integerRange.Ranges);

            var decimalRange = (DecimalRangeFilter)filters[6];
            Assert.IsTrue(decimalRange.Negate);
            CollectionAssert.AreEqual(new[] { new DecimalRangeFilterRange(0.5m, null) }, decimalRange.Ranges);

            var dateRange = (DateTimeOffsetRangeFilter)filters[7];
            CollectionAssert.AreEqual(new[] { new DateTimeOffsetRangeFilterRange(_date, null) }, dateRange.Ranges);
        });
    }

    [Test]
    public async Task Search_Passes_Every_Facet_Type_To_The_Searcher()
    {
        HttpResponseMessage response = await PostAsync(
            """
            {
              "indexAlias": "TestIndex",
              "facets": [
                { "$type": "KeywordFacetRequestModel", "fieldName": "keyword" },
                { "$type": "IntegerExactFacetRequestModel", "fieldName": "integerExact" },
                { "$type": "DecimalExactFacetRequestModel", "fieldName": "decimalExact" },
                { "$type": "DateTimeOffsetExactFacetRequestModel", "fieldName": "dateExact" },
                { "$type": "IntegerRangeFacetRequestModel", "fieldName": "integerRange", "ranges": [{ "key": "low", "minValue": 0, "maxValue": 10 }] },
                { "$type": "DecimalRangeFacetRequestModel", "fieldName": "decimalRange", "ranges": [{ "key": "high", "minValue": 10.5, "maxValue": null }] },
                { "$type": "DateTimeOffsetRangeFacetRequestModel", "fieldName": "dateRange", "ranges": [{ "key": "recent", "minValue": "2026-10-07T12:00:00+00:00", "maxValue": null }] }
              ]
            }
            """);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        Facet[] facets = _searcher.Facets!.ToArray();
        Assert.AreEqual(7, facets.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(new KeywordFacet("keyword"), facets[0]);
            Assert.AreEqual(new IntegerExactFacet("integerExact"), facets[1]);
            Assert.AreEqual(new DecimalExactFacet("decimalExact"), facets[2]);
            Assert.AreEqual(new DateTimeOffsetExactFacet("dateExact"), facets[3]);

            var integerRange = (IntegerRangeFacet)facets[4];
            Assert.AreEqual("integerRange", integerRange.FieldName);
            CollectionAssert.AreEqual(new[] { new IntegerRangeFacetRange("low", 0, 10) }, integerRange.Ranges);

            var decimalRange = (DecimalRangeFacet)facets[5];
            CollectionAssert.AreEqual(new[] { new DecimalRangeFacetRange("high", 10.5m, null) }, decimalRange.Ranges);

            var dateRange = (DateTimeOffsetRangeFacet)facets[6];
            CollectionAssert.AreEqual(new[] { new DateTimeOffsetRangeFacetRange("recent", _date, null) }, dateRange.Ranges);
        });
    }

    [Test]
    public async Task Search_Passes_Every_Sorter_Type_To_The_Searcher()
    {
        HttpResponseMessage response = await PostAsync(
            """
            {
              "indexAlias": "TestIndex",
              "sorters": [
                { "$type": "ScoreSorterRequestModel", "direction": "Descending" },
                { "$type": "KeywordSorterRequestModel", "fieldName": "keyword", "direction": "Ascending" },
                { "$type": "TextSorterRequestModel", "fieldName": "text", "direction": "Descending" },
                { "$type": "IntegerSorterRequestModel", "fieldName": "integer", "direction": "Ascending" },
                { "$type": "DecimalSorterRequestModel", "fieldName": "decimal", "direction": "Ascending" },
                { "$type": "DateTimeOffsetSorterRequestModel", "fieldName": "date", "direction": "Descending" }
              ]
            }
            """);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        CollectionAssert.AreEqual(
            new Sorter[]
            {
                new ScoreSorter(Direction.Descending),
                new KeywordSorter("keyword", Direction.Ascending),
                new TextSorter("text", Direction.Descending),
                new IntegerSorter("integer", Direction.Ascending),
                new DecimalSorter("decimal", Direction.Ascending),
                new DateTimeOffsetSorter("date", Direction.Descending),
            },
            _searcher.Sorters!.ToArray());
    }

    [Test]
    public async Task Search_Returns_Type_Specific_Facet_Values()
    {
        _searcher.FacetResults =
        [
            new FacetResult("keyword", [new KeywordFacetValue("one", 3)]),
            new FacetResult("integerRange", [new IntegerRangeFacetValue("low", 0, 10, 4)]),
            new FacetResult("dateExact", [new DateTimeOffsetExactFacetValue(_date, 5)]),
        ];

        HttpResponseMessage response = await PostAsync("""{ "indexAlias": "TestIndex" }""");

        var body = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);

        using JsonDocument document = JsonDocument.Parse(body);
        JsonElement[] values = document.RootElement.GetProperty("facets")
            .EnumerateArray()
            .Select(facet => facet.GetProperty("values")[0])
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual("KeywordFacetValueResponseModel", values[0].GetProperty("$type").GetString());
            Assert.AreEqual("one", values[0].GetProperty("key").GetString());
            Assert.AreEqual(3, values[0].GetProperty("count").GetInt64());

            Assert.AreEqual("IntegerRangeFacetValueResponseModel", values[1].GetProperty("$type").GetString());
            Assert.AreEqual("low", values[1].GetProperty("key").GetString());
            Assert.AreEqual(0, values[1].GetProperty("min").GetInt32());
            Assert.AreEqual(10, values[1].GetProperty("max").GetInt32());
            Assert.AreEqual(4, values[1].GetProperty("count").GetInt64());

            Assert.AreEqual("DateTimeOffsetExactFacetValueResponseModel", values[2].GetProperty("$type").GetString());
            Assert.AreEqual(_date, values[2].GetProperty("key").GetDateTimeOffset());
            Assert.AreEqual(5, values[2].GetProperty("count").GetInt64());
        });
    }

    [Test]
    public async Task Search_Rejects_Filter_Without_Type_Discriminator()
    {
        HttpResponseMessage response = await PostAsync(
            """{ "indexAlias": "TestIndex", "filters": [{ "fieldName": "keyword", "values": ["one"], "negate": false }] }""");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsFalse(_searcher.WasCalled);
    }

    private static void AssertExactFilter<TFilter, TValue>(Filter filter, string fieldName, TValue[] values, bool negate)
        where TFilter : Filter
    {
        Assert.IsInstanceOf<TFilter>(filter);
        Assert.AreEqual(fieldName, filter.FieldName);
        Assert.AreEqual(negate, filter.Negate);
        TValue[] actualValues = filter switch
        {
            ExactFilter<TValue> exact => exact.Values,
            ContainsFilter<TValue> contains => contains.Values,
            _ => throw new AssertionException($"Unexpected filter type {filter.GetType().Name}."),
        };
        CollectionAssert.AreEqual(values, actualValues);
    }

    private async Task<HttpResponseMessage> PostAsync(string json)
        => await Client.PostAsync(Url, new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json)));

    private sealed class SearcherResolver(ISearcher searcher) : ISearcherResolver
    {
        public ISearcher? GetSearcher(string indexAlias) => indexAlias == IndexAlias ? searcher : null;
    }

    private sealed class CapturingSearcher : ISearcher
    {
        public bool WasCalled { get; private set; }

        public IEnumerable<Filter>? Filters { get; private set; }

        public IEnumerable<Facet>? Facets { get; private set; }

        public IEnumerable<Sorter>? Sorters { get; private set; }

        public FacetResult[] FacetResults { get; set; } = [];

        public void Reset()
        {
            WasCalled = false;
            Filters = null;
            Facets = null;
            Sorters = null;
            FacetResults = [];
        }

        public Task<SearchResult> SearchAsync(
            string indexAlias,
            string? query = null,
            IEnumerable<Filter>? filters = null,
            IEnumerable<Facet>? facets = null,
            IEnumerable<Sorter>? sorters = null,
            string? culture = null,
            string? segment = null,
            AccessContext? accessContext = null,
            int skip = 0,
            int take = 10,
            int maxSuggestions = 0)
        {
            WasCalled = true;
            Filters = filters?.ToArray();
            Facets = facets?.ToArray();
            Sorters = sorters?.ToArray();
            return Task.FromResult(new SearchResult(0, [], FacetResults));
        }
    }
}
