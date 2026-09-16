using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Indexer;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Indexer;

/// <summary>
/// Verifies that rebuilding a member index requires access to the members section.
/// </summary>
public class RebuildIndexerControllerMemberIndexTests : ManagementApiTest<RebuildIndexerController>
{
    protected override Expression<Func<RebuildIndexerController, object>> MethodSelector { get; set; }

    [Test]
    public async Task Cannot_Rebuild_Member_Index_Without_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnlyForIndexRebuild", Constants.Applications.Settings);

        HttpResponseMessage response =
            await Client.PostAsync(RebuildUrl(Constants.UmbracoIndexes.MembersIndexName), null);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Rebuild_Member_Index_With_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync(
            "settingsAndMembersForIndexRebuild",
            Constants.Applications.Settings,
            Constants.Applications.Members);

        HttpResponseMessage response =
            await Client.PostAsync(RebuildUrl(Constants.UmbracoIndexes.MembersIndexName), null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string RebuildUrl(string indexName)
        => GetManagementApiUrl<RebuildIndexerController>(x => x.Rebuild(CancellationToken.None, indexName));
}
