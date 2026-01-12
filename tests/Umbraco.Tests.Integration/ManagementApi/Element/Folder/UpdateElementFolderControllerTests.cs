using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Folder;

public class UpdateElementFolderControllerTests : ManagementApiUserGroupTestBase<UpdateElementFolderController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private Guid _folderKey;

    [SetUp]
    public async Task Setup()
    {
        var result = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        _folderKey = result.Result!.Key;
    }

    protected override Expression<Func<UpdateElementFolderController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, _folderKey, null!);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var updateModel = new UpdateFolderResponseModel
        {
            Name = Guid.NewGuid().ToString(),
        };

        return await Client.PutAsync(Url, JsonContent.Create(updateModel));
    }
}
