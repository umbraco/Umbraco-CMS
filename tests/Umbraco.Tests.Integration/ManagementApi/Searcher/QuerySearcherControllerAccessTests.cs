using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Searcher;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Searcher;

/// <summary>
/// Verifies that querying a searcher requires administrator access by default, regardless of the sections a user has
/// access to, while the rest of the searcher endpoints remain available with settings section access.
/// </summary>
public class QuerySearcherControllerAccessTests : ManagementApiTest<QuerySearcherController>
{
    protected override Expression<Func<QuerySearcherController, object>> MethodSelector { get; set; }

    [TestCase(Constants.UmbracoIndexes.InternalIndexName)]
    [TestCase(Constants.UmbracoIndexes.ExternalIndexName)]
    [TestCase(Constants.UmbracoIndexes.MembersIndexName)]
    public async Task Cannot_Query_Searcher_Without_Admin_Access(string searcherName)
    {
        await AuthenticateWithSectionsAsync(
            $"allSectionsNotAdmin{searcherName}",
            Constants.Applications.Content,
            Constants.Applications.Media,
            Constants.Applications.Members,
            Constants.Applications.Settings);

        HttpResponseMessage response = await Client.GetAsync(QueryUrl(searcherName, "umbraco"));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [TestCase(Constants.UmbracoIndexes.InternalIndexName)]
    [TestCase(Constants.UmbracoIndexes.ExternalIndexName)]
    [TestCase(Constants.UmbracoIndexes.MembersIndexName)]
    public async Task Can_Query_Searcher_With_Admin_Access(string searcherName)
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", UserPassword, true);

        HttpResponseMessage response = await Client.GetAsync(QueryUrl(searcherName, "umbraco"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_List_Searchers_Without_Admin_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnly", Constants.Applications.Settings);

        HttpResponseMessage response = await Client.GetAsync(
            GetManagementApiUrl<AllSearcherController>(x => x.All(CancellationToken.None, 0, 100)));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string QueryUrl(string searcherName, string term)
        => GetManagementApiUrl<QuerySearcherController>(x => x.Query(CancellationToken.None, searcherName, term, 0, 100));
}
