using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint.Tree;

public class ChildrenDocumentBlueprintTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenDocumentBlueprintTreeController>
{
    private IContentBlueprintContainerService ContentBlueprintContainerService =>
        GetRequiredService<IContentBlueprintContainerService>();

    private IContentBlueprintEditingService ContentBlueprintEditingService =>
        GetRequiredService<IContentBlueprintEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _folderKey;

    private Guid _blueprintKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var folder = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Test Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(folder.Success, $"Failed to create the folder: {folder.Status}");
        _folderKey = folder.Result!.Key;

        var blueprint = await ContentBlueprintEditingService.CreateAsync(
            new ContentBlueprintCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = _folderKey,
                Variants = [new VariantModel { Name = $"Blueprint {Guid.NewGuid()}" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(blueprint.Success, $"Failed to create the blueprint: {blueprint.Status}");
        _blueprintKey = blueprint.Result.Content!.Key;
    }

    protected override Expression<Func<ChildrenDocumentBlueprintTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _folderKey, 0, 100, false);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    [Test]
    public async Task User_Without_A_Start_Node_Sees_No_Children()
    {
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Group Without A Blueprint Start Node {Guid.NewGuid()}")
            .WithAllowedSections([Constants.Applications.Library])
            .Build();

        // The builder defaults every start node to the root, and this test is about a group granted none.
        userGroup.StartDocumentBlueprintId = null;
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        await AuthenticateClientAsync(
            Client,
            $"noblueprintstartnode{Guid.NewGuid():N}@umbraco.com",
            UserPassword,
            userGroup.Key);

        var response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        var result = await response.Content
            .ReadFromJsonAsync<PagedViewModel<DocumentBlueprintTreeItemResponseModel>>(JsonSerializerOptions);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Total, "A user with no blueprint start node must not see any children.");
    }
}
