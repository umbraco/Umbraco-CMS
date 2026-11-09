using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Search;
using Umbraco.Cms.Api.Management.ViewModels.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Provider.Examine.Controllers;
using Umbraco.Cms.Search.Provider.Examine.DependencyInjection;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Search;

/// <summary>
/// Verifies that searching an index and reading its raw field values require administrator access by default,
/// regardless of the sections a user has access to, while listing indexes remains available with settings section access.
/// </summary>
public class SearchQueryAccessTests : ManagementApiTest<SearchApiController>
{
    protected override Expression<Func<SearchApiController, object>> MethodSelector { get; set; }
        = x => x.Search(null!, 0, 100);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddExamineSearchProvider();
        builder.Services.AddMvcCore().AddApplicationPart(typeof(ExamineApiController).Assembly);
    }

    [TestCase(Constants.Search.IndexAliases.DraftContent)]
    [TestCase(Constants.Search.IndexAliases.DraftMembers)]
    public async Task Cannot_Search_Index_Without_Admin_Access(string indexAlias)
    {
        await AuthenticateWithAllSectionsNotAdminAsync(indexAlias);

        HttpResponseMessage response = await Client.PostAsJsonAsync(Url, CreateRequest(indexAlias));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [TestCase(Constants.Search.IndexAliases.DraftContent)]
    [TestCase(Constants.Search.IndexAliases.DraftMembers)]
    public async Task Can_Search_Index_With_Admin_Access(string indexAlias)
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", UserPassword, true);

        HttpResponseMessage response = await Client.PostAsJsonAsync(Url, CreateRequest(indexAlias));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Read_Index_Document_Fields_Without_Admin_Access()
    {
        await AuthenticateWithAllSectionsNotAdminAsync("documentFields");

        HttpResponseMessage response = await Client.GetAsync(
            $"/umbraco/examine/api/v1/{Constants.Search.IndexAliases.DraftContent}/document/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_List_Indexes_Without_Admin_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnly", Constants.Applications.Settings);

        HttpResponseMessage response = await Client.GetAsync("/umbraco/management/api/v1/search/indexes");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private Task AuthenticateWithAllSectionsNotAdminAsync(string groupAliasSuffix)
        => AuthenticateWithSectionsAsync(
            $"allSectionsNotAdmin{groupAliasSuffix}",
            Constants.Applications.Content,
            Constants.Applications.Media,
            Constants.Applications.Members,
            Constants.Applications.Settings);

    private static SearchRequestModel CreateRequest(string indexAlias)
        => new() { IndexAlias = indexAlias, Query = "umbraco" };
}
