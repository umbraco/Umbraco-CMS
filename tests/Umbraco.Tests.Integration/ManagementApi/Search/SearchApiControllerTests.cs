using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
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
    public async Task Search_Passes_Query_Culture_And_Segment_To_The_Searcher()
    {
        HttpResponseMessage response = await PostAsync(
            """{ "indexAlias": "TestIndex", "query": "some text", "culture": "da-DK", "segment": "segment-one" }""");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        Assert.Multiple(() =>
        {
            Assert.IsTrue(_searcher.WasCalled);
            Assert.AreEqual("some text", _searcher.Query);
            Assert.AreEqual("da-DK", _searcher.Culture);
            Assert.AreEqual("segment-one", _searcher.Segment);
        });
    }

    [Test]
    public async Task Search_Returns_NotFound_For_Unknown_Index()
    {
        HttpResponseMessage response = await PostAsync("""{ "indexAlias": "UnknownIndex" }""");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.IsFalse(_searcher.WasCalled);
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

        public string? Query { get; private set; }

        public string? Culture { get; private set; }

        public string? Segment { get; private set; }

        public void Reset()
        {
            WasCalled = false;
            Query = null;
            Culture = null;
            Segment = null;
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
            Query = query;
            Culture = culture;
            Segment = segment;
            return Task.FromResult(new SearchResult(0, [], []));
        }
    }
}
