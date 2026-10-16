using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint.Folder;

public class UpdateDocumentBlueprintFolderControllerTests
    : ManagementApiUserGroupTestBase<UpdateDocumentBlueprintFolderController>
{
    private IContentBlueprintContainerService ContentBlueprintContainerService =>
        GetRequiredService<IContentBlueprintContainerService>();

    private Guid _folderKey;

    [SetUp]
    public new async Task Setup()
    {
        var result = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Test Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create the folder: {result.Status}");
        _folderKey = result.Result!.Key;
    }

    protected override Expression<Func<UpdateDocumentBlueprintFolderController, object>> MethodSelector =>
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
        var updateModel = new UpdateFolderResponseModel { Name = $"Test Folder {Guid.NewGuid()}" };

        return await Client.PutAsync(Url, JsonContent.Create(updateModel));
    }
}
