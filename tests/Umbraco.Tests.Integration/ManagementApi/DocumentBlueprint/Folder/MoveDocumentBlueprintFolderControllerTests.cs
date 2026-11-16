using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint.Folder;

public class MoveDocumentBlueprintFolderControllerTests : ManagementApiUserGroupTestBase<MoveDocumentBlueprintFolderController>
{
    private IContentBlueprintContainerService ContentBlueprintContainerService => GetRequiredService<IContentBlueprintContainerService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _folderKey;

    private Guid _targetFolderKey;

    [SetUp]
    public new async Task Setup()
    {
        // The schema is shared across the fixture, so a fixed name would collide with a folder left by an earlier test.
        _folderKey = (await CreateFolderAsync(null)).Key;
        _targetFolderKey = (await CreateFolderAsync(null)).Key;
    }

    protected override Expression<Func<MoveDocumentBlueprintFolderController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _folderKey, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        MoveFolderRequestModel moveFolderModel = new() { Target = new ReferenceByIdModel(_targetFolderKey) };

        return await Client.PutAsync(Url, JsonContent.Create(moveFolderModel));
    }

    [Test]
    public async Task User_With_Start_Node_Can_Move_A_Folder_Within_It()
    {
        EntityContainer startNodeFolder = await CreateFolderAsync(null);
        _folderKey = (await CreateFolderAsync(startNodeFolder.Key)).Key;
        _targetFolderKey = (await CreateFolderAsync(startNodeFolder.Key)).Key;

        await AuthenticateAsUserScopedToFolderAsync(startNodeFolder.Id);

        HttpResponseMessage response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task User_With_Start_Node_Cannot_Move_A_Folder_Out_Of_It()
    {
        EntityContainer startNodeFolder = await CreateFolderAsync(null);
        _folderKey = (await CreateFolderAsync(startNodeFolder.Key)).Key;
        _targetFolderKey = (await CreateFolderAsync(null)).Key;

        await AuthenticateAsUserScopedToFolderAsync(startNodeFolder.Id);

        HttpResponseMessage response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task User_With_Start_Node_Cannot_Move_A_Folder_Into_It_From_Outside()
    {
        EntityContainer startNodeFolder = await CreateFolderAsync(null);
        _folderKey = (await CreateFolderAsync(null)).Key;
        _targetFolderKey = (await CreateFolderAsync(startNodeFolder.Key)).Key;

        await AuthenticateAsUserScopedToFolderAsync(startNodeFolder.Id);

        HttpResponseMessage response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<EntityContainer> CreateFolderAsync(Guid? parentKey)
    {
        var result = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Test Folder {Guid.NewGuid()}",
            parentKey,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create the folder: {result.Status}");
        return result.Result!;
    }

    private async Task AuthenticateAsUserScopedToFolderAsync(int folderId)
    {
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Group With Document Blueprint Start Node {Guid.NewGuid()}")
            .WithAllowedSections(["library"])
            .WithStartDocumentBlueprintId(folderId)
            .Build();
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        await AuthenticateClientAsync(
            Client,
            $"blueprintstartnode{Guid.NewGuid():N}@umbraco.com",
            UserPassword,
            userGroup.Key);
    }
}
