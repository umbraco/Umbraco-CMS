using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Folder;

public class ByPathPartialViewFolderControllerTests : ManagementApiUserGroupTestBase<ByPathPartialViewFolderController>
{
    protected override Expression<Func<ByPathPartialViewFolderController, object>> MethodSelector =>
        x => x.ByPath(CancellationToken.None, "TestPartialViewFolderByKey");

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
}
