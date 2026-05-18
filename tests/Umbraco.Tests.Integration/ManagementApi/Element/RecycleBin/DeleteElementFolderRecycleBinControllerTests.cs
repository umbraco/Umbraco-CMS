using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.RecycleBin;

public class DeleteElementFolderRecycleBinControllerTests : ElementRecycleBinControllerTestBase<DeleteElementFolderRecycleBinController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private Guid _folderKey;

    [SetUp]
    public async Task Setup()
    {
        var result = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create folder: {result.Status}");
        _folderKey = result.Result!.Key;

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(_folderKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success, $"Failed to move folder to recycle bin: {moveResult.Result}");
    }

    protected override Expression<Func<DeleteElementFolderRecycleBinController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, _folderKey);

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
        => await Client.DeleteAsync(Url);
}
