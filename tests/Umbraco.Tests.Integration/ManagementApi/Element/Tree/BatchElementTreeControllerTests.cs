using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.Tree;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Tree;

public class BatchElementTreeControllerTests : ManagementApiUserGroupTestBase<BatchElementTreeController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    protected override Expression<Func<BatchElementTreeController, object>> MethodSelector =>
        x => x.Batch(CancellationToken.None, new HashSet<Guid> { Guid.NewGuid() }, null);

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
    public async Task User_With_Start_Node_Only_Gets_Items_At_Or_Below_The_Start_Node()
    {
        EntityContainer permittedFolder = await CreateFolderAsync(null);
        EntityContainer childFolder = await CreateFolderAsync(permittedFolder.Key);
        EntityContainer otherFolder = await CreateFolderAsync(null);

        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Test Group With Element Start Node")
            .WithAllowedSections(["library"])
            .WithPermissions(new HashSet<string> { ActionElementBrowse.ActionLetter, ActionElementContainerBrowse.ActionLetter })
            .WithStartElementId(permittedFolder.Id)
            .Build();
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        await AuthenticateClientAsync(Client, $"startnodetest{Guid.NewGuid():N}@umbraco.com", "1234567890", userGroup.Key);

        var url = GetManagementApiUrl<BatchElementTreeController>(x =>
            x.Batch(CancellationToken.None, new HashSet<Guid> { childFolder.Key, otherFolder.Key, permittedFolder.Key }, null));
        var response = await Client.GetAsync(url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        var result = await response.Content.ReadFromJsonAsync<BatchResponseModel<ElementTreeItemResponseModel>>(JsonSerializerOptions);
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Total);
        CollectionAssert.AreEqual(new[] { childFolder.Key, permittedFolder.Key }, result.Items.Select(x => x.Id));
        Assert.AreEqual(permittedFolder.Key, result.Items.First().Parent?.Id);
    }

    private async Task<EntityContainer> CreateFolderAsync(Guid? parentKey)
    {
        var result = await ElementContainerService.CreateAsync(
            null,
            $"Folder {Guid.NewGuid()}",
            parentKey,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result!;
    }
}
