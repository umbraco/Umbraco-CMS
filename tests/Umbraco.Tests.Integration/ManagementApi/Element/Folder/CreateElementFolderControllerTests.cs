using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Element.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Folder;

public class CreateElementFolderControllerTests : ManagementApiUserGroupTestBase<CreateElementFolderController>
{
    protected override Expression<Func<CreateElementFolderController, object>> MethodSelector =>
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
            Name = Guid.NewGuid().ToString(),
            Parent = null,
            Id = Guid.NewGuid(),
        };

        return await Client.PostAsync(Url, JsonContent.Create(createModel));
    }
}
