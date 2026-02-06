using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.Folder;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Folder;

public class MoveElementFolderControllerTests : ManagementApiUserGroupTestBase<MoveElementFolderController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private Guid _folderKey;
    private Guid _targetFolderKey;

    [SetUp]
    public async Task Setup()
    {
        var folderResult = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        Assert.IsTrue(folderResult.Success, $"Failed to create folder: {folderResult.Status}");
        _folderKey = folderResult.Result!.Key;

        var targetResult = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        Assert.IsTrue(targetResult.Success, $"Failed to create target folder: {targetResult.Status}");
        _targetFolderKey = targetResult.Result!.Key;
    }

    protected override Expression<Func<MoveElementFolderController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _folderKey, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var moveModel = new MoveFolderRequestModel
        {
            Target = new ReferenceByIdModel(_targetFolderKey),
        };

        return await Client.PutAsync(Url, JsonContent.Create(moveModel));
    }
}
