using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Folder;

public class DeletePartialViewFolderControllerTests : ManagementApiUserGroupTestBase<DeletePartialViewFolderController>
{
    protected override Expression<Func<DeletePartialViewFolderController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, "TestDeletePartialViewFolder");

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);
}
