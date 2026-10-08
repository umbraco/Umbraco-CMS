using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Folder;

public class MoveToRecycleBinElementFolderControllerTests : ManagementApiUserGroupTestBase<MoveToRecycleBinElementFolderController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private Guid _folderKey;

    [SetUp]
    public async Task Setup()
    {
        var result = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create folder: {result.Status}");
        _folderKey = result.Result!.Key;
    }

    protected override Expression<Func<MoveToRecycleBinElementFolderController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _folderKey);

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
        => await Client.PutAsync(Url, null);
}
