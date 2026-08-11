using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType.Folder;

public class MoveMediaTypeFolderControllerTests : ManagementApiUserGroupTestBase<MoveMediaTypeFolderController>
{
    private IMediaTypeContainerService MediaTypeContainerService => GetRequiredService<IMediaTypeContainerService>();

    private Guid _folderKey;

    private Guid _targetFolderKey;

    [SetUp]
    public new async Task Setup()
    {
        var folder = await MediaTypeContainerService.CreateAsync(Guid.NewGuid(), "TestFolder", Constants.System.RootKey, Constants.Security.SuperUserKey);
        _folderKey = folder.Result!.Key;

        var targetFolder = await MediaTypeContainerService.CreateAsync(Guid.NewGuid(), "TestTargetFolder", Constants.System.RootKey, Constants.Security.SuperUserKey);
        _targetFolderKey = targetFolder.Result!.Key;
    }

    protected override Expression<Func<MoveMediaTypeFolderController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _folderKey, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        MoveFolderRequestModel moveFolderModel = new() { Target = new ReferenceByIdModel(_targetFolderKey) };

        return await Client.PutAsync(Url, JsonContent.Create(moveFolderModel));
    }
}
