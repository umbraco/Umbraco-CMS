using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.RedirectUrlManagement;

public class GetStatusRedirectUrlManagementControllerTests : ManagementApiUserGroupTestBase<GetStatusRedirectUrlManagementController>
{
    protected override Expression<Func<GetStatusRedirectUrlManagementController, object>> MethodSelector =>
        x => x.GetStatus(CancellationToken.None);

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
