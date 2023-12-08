using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Folder;

public class CreatePartialViewFolderControllerTests : ManagementApiUserGroupTestBase<CreatePartialViewFolderController>
{
    protected override Expression<Func<CreatePartialViewFolderController, object>> MethodSelector =>
        x => x.Create(null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        CreatePathFolderRequestModel createPathFolderRequestModel = new() { Name = "TestCreatedPartialViewFolder", ParentPath = "TestParentFolder" };

        return await Client.PostAsync(Url, JsonContent.Create(createPathFolderRequestModel));
    }
}
