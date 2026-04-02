using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType.Folder;

public class CreateMediaTypeFolderControllerTests : ManagementApiUserGroupTestBase<CreateMediaTypeFolderController>
{
    protected override Expression<Func<CreateMediaTypeFolderController, object>> MethodSelector => x => x.Create(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
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
        CreateFolderRequestModel createFolderRequestModel = new() { Name = "Test" };

        return await Client.PostAsync(Url, JsonContent.Create(createFolderRequestModel));
    }
}
