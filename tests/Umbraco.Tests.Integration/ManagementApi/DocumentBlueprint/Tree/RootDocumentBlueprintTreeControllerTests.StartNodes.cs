using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint.Tree;

public partial class RootDocumentBlueprintTreeControllerTests
{
    private IContentBlueprintContainerService ContentBlueprintContainerService =>
        GetRequiredService<IContentBlueprintContainerService>();

    private IContentBlueprintEditingService ContentBlueprintEditingService =>
        GetRequiredService<IContentBlueprintEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    [Test]
    public async Task Start_Node_Folder_Reports_Children_When_It_Holds_Only_Blueprints()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var startNodeResult = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Start Node Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(startNodeResult.Success, $"Failed to create the start node folder: {startNodeResult.Status}");
        var startNodeFolder = startNodeResult.Result!;

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = startNodeFolder.Key,
            Variants = [new VariantModel { Name = $"Blueprint {Guid.NewGuid()}" }],
        };
        var blueprintResult =
            await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(blueprintResult.Success, $"Failed to create the blueprint: {blueprintResult.Status}");

        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Test Group With Document Blueprint Start Node")
            .WithAllowedSections(["library"])
            .WithStartDocumentBlueprintId(startNodeFolder.Id)
            .Build();
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        await AuthenticateClientAsync(
            Client,
            $"blueprintstartnode{Guid.NewGuid():N}@umbraco.com",
            "1234567890",
            userGroup.Key);

        var response = await ClientRequest();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        var result = await response.Content
            .ReadFromJsonAsync<PagedViewModel<DocumentBlueprintTreeItemResponseModel>>(JsonSerializerOptions);

        Assert.IsNotNull(result);

        var folder = result.Items.SingleOrDefault(x => x.Id == startNodeFolder.Key);
        Assert.IsNotNull(folder, "The start node folder was not returned as a root item.");

        Assert.Multiple(() =>
        {
            Assert.IsTrue(folder.IsFolder);
            Assert.IsTrue(
                folder.HasChildren,
                "A start node folder holding a blueprint must report children, so that it can be expanded.");
            Assert.IsNull(folder.DocumentType, "A folder has no document type.");
        });
    }

    [Test]
    public async Task Ancestor_Of_A_Start_Node_Is_Returned_Without_Access()
    {
        var outerFolderResult = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Outer Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(outerFolderResult.Success, $"Failed to create the outer folder: {outerFolderResult.Status}");
        var outerFolder = outerFolderResult.Result!;

        var innerFolderResult = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Inner Folder {Guid.NewGuid()}",
            outerFolder.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(innerFolderResult.Success, $"Failed to create the inner folder: {innerFolderResult.Status}");
        var innerFolder = innerFolderResult.Result!;

        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Test Group With Inner Document Blueprint Start Node")
            .WithAllowedSections(["library"])
            .WithStartDocumentBlueprintId(innerFolder.Id)
            .Build();
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        await AuthenticateClientAsync(
            Client,
            $"blueprintinnerstartnode{Guid.NewGuid():N}@umbraco.com",
            "1234567890",
            userGroup.Key);

        var response = await ClientRequest();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        var result = await response.Content
            .ReadFromJsonAsync<PagedViewModel<DocumentBlueprintTreeItemResponseModel>>(JsonSerializerOptions);

        Assert.IsNotNull(result);

        // The ancestor has to be returned, or there is no way to browse down to the start node.
        var outerItem = result.Items.SingleOrDefault(x => x.Id == outerFolder.Key);
        Assert.IsNotNull(outerItem, "The ancestor of the start node was not returned as a root item.");

        Assert.Multiple(() =>
        {
            Assert.IsTrue(
                outerItem.NoAccess,
                "An ancestor of the start node is only there to be browsed through, so it must report no access.");
            Assert.IsTrue(outerItem.HasChildren, "The ancestor holds the start node, so it must report children.");
            Assert.IsFalse(
                result.Items.Any(x => x.Id == innerFolder.Key),
                "The start node itself is not a root item when it sits below one.");
        });
    }
}
