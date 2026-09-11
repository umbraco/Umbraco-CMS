using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint.Folder;

public class CreateDocumentBlueprintFolderControllerTests
    : ManagementApiUserGroupTestBase<CreateDocumentBlueprintFolderController>
{
    protected override Expression<Func<CreateDocumentBlueprintFolderController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var createModel = new CreateFolderRequestModel
        {
            Name = $"Test Folder {Guid.NewGuid()}",
            Parent = null,
            Id = Guid.NewGuid(),
        };

        return await Client.PostAsync(Url, JsonContent.Create(createModel));
    }
}
