using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.RedirectUrlManagement;

public class ByKeyRedirectUrlManagementControllerTests : ManagementApiUserGroupTestBase<ByKeyRedirectUrlManagementController>
{
    protected override Expression<Func<ByKeyRedirectUrlManagementController, object>> MethodSelector =>
        x => x.ByKey(CancellationToken.None, Guid.NewGuid(), 0, 100);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
