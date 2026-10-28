using System.Linq.Expressions;
using System.Net;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Tree;

public class DocumentTypeTreeParentTests : ManagementApiTest<ChildrenDocumentTypeTreeController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private Guid _folderKey;
    private Guid _inFolderKey;
    private Guid _inheritingKey;
    private Guid _atRootKey;

    protected override Expression<Func<ChildrenDocumentTypeTreeController, object>> MethodSelector { get; set; } =
        x => x.Children(CancellationToken.None, Guid.Empty, 0, 100, false);

    [SetUp]
    public async Task CreateStructure()
    {
        var suffix = Guid.NewGuid().ToString("N");

        _folderKey = Guid.NewGuid();
        Assert.IsTrue((await ContentTypeContainerService.CreateAsync(_folderKey, $"Folder {suffix}", null, Constants.Security.SuperUserKey)).Success);

        _inFolderKey = Guid.NewGuid();
        Assert.IsTrue((await ContentTypeEditingService.CreateAsync(
            new ContentTypeCreateModel { Key = _inFolderKey, Name = $"In Folder {suffix}", Alias = $"inFolder{suffix}", ContainerKey = _folderKey },
            Constants.Security.SuperUserKey)).Success);

        _inheritingKey = Guid.NewGuid();
        Assert.IsTrue((await ContentTypeEditingService.CreateAsync(
            new ContentTypeCreateModel
            {
                Key = _inheritingKey,
                Name = $"Inheriting {suffix}",
                Alias = $"inheriting{suffix}",
                Compositions = [new Composition { Key = _inFolderKey, CompositionType = CompositionType.Inheritance }],
            },
            Constants.Security.SuperUserKey)).Success);

        _atRootKey = Guid.NewGuid();
        Assert.IsTrue((await ContentTypeEditingService.CreateAsync(
            new ContentTypeCreateModel { Key = _atRootKey, Name = $"At Root {suffix}", Alias = $"atRoot{suffix}" },
            Constants.Security.SuperUserKey)).Success);

        await AuthenticateClientAsync(Client, "admin@umbraco.com", UserPassword, isAdmin: true);
    }

    [Test]
    public async Task Children_Report_Whether_The_Parent_Is_A_Folder()
    {
        JsonNode folderChildren = await GetAsync<ChildrenDocumentTypeTreeController>(x => x.Children(CancellationToken.None, _folderKey, 0, 100, false));
        AssertParent(FindItem(folderChildren["items"]!, _inFolderKey), _folderKey, true);

        JsonNode itemChildren = await GetAsync<ChildrenDocumentTypeTreeController>(x => x.Children(CancellationToken.None, _inFolderKey, 0, 100, false));
        AssertParent(FindItem(itemChildren["items"]!, _inheritingKey), _inFolderKey, false);
    }

    [Test]
    public async Task Root_Items_Have_No_Parent()
    {
        JsonNode root = await GetAsync<RootDocumentTypeTreeController>(x => x.Root(CancellationToken.None, 0, 100, false));

        Assert.IsNull(FindItem(root["items"]!, _atRootKey)["parent"]);
    }

    [Test]
    public async Task Ancestors_Report_Whether_The_Parent_Is_A_Folder()
    {
        JsonNode ancestors = await GetAsync<AncestorsDocumentTypeTreeController>(x => x.Ancestors(CancellationToken.None, _inheritingKey));

        Assert.IsNull(FindItem(ancestors, _folderKey)["parent"]);
        AssertParent(FindItem(ancestors, _inFolderKey), _folderKey, true);
        AssertParent(FindItem(ancestors, _inheritingKey), _inFolderKey, false);
    }

    [Test]
    public async Task Siblings_Report_Whether_The_Parent_Is_A_Folder()
    {
        JsonNode inFolder = await GetSiblingsAsync(_inFolderKey);
        AssertParent(FindItem(inFolder["items"]!, _inFolderKey), _folderKey, true);

        JsonNode underItem = await GetSiblingsAsync(_inheritingKey);
        AssertParent(FindItem(underItem["items"]!, _inheritingKey), _inFolderKey, false);
    }

    // The siblings controller has no [ApiVersion], so the link generator can't build its URL.
    private async Task<JsonNode> GetSiblingsAsync(Guid target)
    {
        var childrenUrl = GetManagementApiUrl<ChildrenDocumentTypeTreeController>(x => x.Children(CancellationToken.None, Guid.Empty, 0, 100, false));
        var siblingsUrl = $"{childrenUrl[..childrenUrl.IndexOf("/children", StringComparison.Ordinal)]}/siblings?target={target}&before=0&after=0";
        return await GetAsync(siblingsUrl);
    }

    [Test]
    public async Task Search_Results_Report_Whether_The_Parent_Is_A_Folder()
    {
        JsonNode inFolder = await GetAsync<SearchDocumentTypeTreeController>(x => x.Search(CancellationToken.None, "In Folder", 0, 100, TreeItemKind.All));
        AssertParent(FindItem(inFolder["items"]!, _inFolderKey), _folderKey, true);

        JsonNode underItem = await GetAsync<SearchDocumentTypeTreeController>(x => x.Search(CancellationToken.None, "Inheriting", 0, 100, TreeItemKind.All));
        AssertParent(FindItem(underItem["items"]!, _inheritingKey), _inFolderKey, false);
    }

    private async Task<JsonNode> GetAsync<TController>(Expression<Func<TController, object>> methodSelector)
        where TController : ManagementApiControllerBase
        => await GetAsync(GetManagementApiUrl(methodSelector));

    private async Task<JsonNode> GetAsync(string url)
    {
        HttpResponseMessage response = await Client.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);
        return JsonNode.Parse(body)!;
    }

    private static JsonNode FindItem(JsonNode items, Guid id)
    {
        JsonNode? item = items.AsArray().SingleOrDefault(x => x!["id"]!.GetValue<Guid>() == id);
        Assert.IsNotNull(item, $"Item {id} was not found in the response.");
        return item!;
    }

    private static void AssertParent(JsonNode item, Guid expectedId, bool expectedIsFolder)
    {
        JsonNode? parent = item["parent"];
        Assert.IsNotNull(parent);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedId, parent!["id"]!.GetValue<Guid>());
            Assert.AreEqual(expectedIsFolder, parent["isFolder"]?.GetValue<bool>());
            Assert.IsNull(item["parentIsFolder"]);
        });
    }
}
