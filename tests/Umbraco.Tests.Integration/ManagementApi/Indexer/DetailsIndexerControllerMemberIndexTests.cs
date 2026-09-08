using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Indexer;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Indexer;

/// <summary>
/// Verifies that member index diagnostics require access to the members section.
/// </summary>
public class DetailsIndexerControllerMemberIndexTests : ManagementApiTest<DetailsIndexerController>
{
    protected override Expression<Func<DetailsIndexerController, object>> MethodSelector { get; set; }

    [Test]
    public async Task Cannot_View_Member_Index_Details_Without_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnlyForIndexDetails", Constants.Applications.Settings);

        HttpResponseMessage response = await Client.GetAsync(DetailsUrl(Constants.UmbracoIndexes.MembersIndexName));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_View_Member_Index_Details_With_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync(
            "settingsAndMembersForIndexDetails",
            Constants.Applications.Settings,
            Constants.Applications.Members);

        HttpResponseMessage response = await Client.GetAsync(DetailsUrl(Constants.UmbracoIndexes.MembersIndexName));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_View_Content_Index_Details_Without_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnlyForContentIndexDetails", Constants.Applications.Settings);

        HttpResponseMessage response = await Client.GetAsync(DetailsUrl(Constants.UmbracoIndexes.ExternalIndexName));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string DetailsUrl(string indexName)
        => GetManagementApiUrl<DetailsIndexerController>(x => x.Details(CancellationToken.None, indexName));
}
