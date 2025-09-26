using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet.Folder;

public class DeleteStylesheetFolderControllerTests : ManagementApiUserGroupTestBase<DeleteStylesheetFolderController>
{
    protected override Expression<Func<DeleteStylesheetFolderController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, "TestDeleteFolder");

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
