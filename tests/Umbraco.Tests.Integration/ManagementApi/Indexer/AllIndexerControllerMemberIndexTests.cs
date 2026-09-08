using System.Linq.Expressions;
using System.Net;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Indexer;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Indexer;

/// <summary>
/// Verifies that member indexes are excluded from the indexer listing for users without access to the
/// members section.
/// </summary>
public class AllIndexerControllerMemberIndexTests : ManagementApiTest<AllIndexerController>
{
    protected override Expression<Func<AllIndexerController, object>> MethodSelector { get; set; }

    [Test]
    public async Task Cannot_See_Member_Index_Without_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync("settingsOnlyForAllIndexes", Constants.Applications.Settings);

        var indexNames = await GetIndexNamesAsync();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(
                indexNames.Contains(Constants.UmbracoIndexes.MembersIndexName),
                $"Expected the member index to be excluded, got: {string.Join(", ", indexNames)}");
            Assert.IsTrue(
                indexNames.Contains(Constants.UmbracoIndexes.ExternalIndexName),
                $"Expected non-member indexes to still be listed, got: {string.Join(", ", indexNames)}");
        });
    }

    [Test]
    public async Task Can_See_Member_Index_With_Members_Section_Access()
    {
        await AuthenticateWithSectionsAsync(
            "settingsAndMembersForAllIndexes",
            Constants.Applications.Settings,
            Constants.Applications.Members);

        var indexNames = await GetIndexNamesAsync();

        Assert.IsTrue(
            indexNames.Contains(Constants.UmbracoIndexes.MembersIndexName),
            $"Expected the member index to be listed, got: {string.Join(", ", indexNames)}");
    }

    private async Task<List<string>> GetIndexNamesAsync()
    {
        var allUrl = GetManagementApiUrl<AllIndexerController>(x => x.All(CancellationToken.None, 0, 100));
        HttpResponseMessage response = await Client.GetAsync(allUrl);
        var body = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);

        // Read the names structurally rather than binding the whole model, which would require the
        // back office JSON options (string enums) to deserialize the health status.
        JsonArray? items = JsonNode.Parse(body)?["items"]?.AsArray();
        Assert.IsNotNull(items, $"Expected an items array, got: {body}");

        return items!.Select(item => item?["name"]?.GetValue<string>() ?? string.Empty).ToList();
    }
}
