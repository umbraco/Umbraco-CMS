using System.Linq.Expressions;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Searcher;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Searcher;

/// <summary>
/// Verifies that querying a member index requires access to the members section, on top of the
/// settings section access required by <see cref="SearcherControllerBase" />, when the
/// <see cref="AuthorizationPolicies.SearcherQueryAccess" /> policy is relaxed to settings section access.
/// </summary>
public class QuerySearcherControllerMemberIndexTests : ManagementApiTest<QuerySearcherController>
{
    protected override Expression<Func<QuerySearcherController, object>> MethodSelector { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.PostConfigure<AuthorizationOptions>(options =>
            options.AddPolicy(
                AuthorizationPolicies.SearcherQueryAccess,
                options.GetPolicy(AuthorizationPolicies.SectionAccessSettings)!));
    }

    [Test]
    public async Task Cannot_Query_Member_Index_Without_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnly", Constants.Applications.Settings);

        HttpResponseMessage response = await Client.GetAsync(QueryUrl(Constants.UmbracoIndexes.MembersIndexName, "umbraco"));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Query_Member_Index_With_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync(
            "settingsAndMembers",
            Constants.Applications.Settings,
            Constants.Applications.Members);

        HttpResponseMessage response = await Client.GetAsync(QueryUrl(Constants.UmbracoIndexes.MembersIndexName, string.Empty));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Query_Content_Index_Without_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnlyForContentIndex", Constants.Applications.Settings);

        HttpResponseMessage response = await Client.GetAsync(QueryUrl(Constants.UmbracoIndexes.ExternalIndexName, string.Empty));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string QueryUrl(string searcherName, string term)
        => GetManagementApiUrl<QuerySearcherController>(x => x.Query(CancellationToken.None, searcherName, term, 0, 100));
}
