using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.HealthCheck.Group;

public class ByNameHealthCheckGroupControllerTests : ManagementApiUserGroupTestBase<ByNameHealthCheckGroupController>
{
    protected override Expression<Func<ByNameHealthCheckGroupController, object>> MethodSelector =>
        x => x.ByName(CancellationToken.None, "Configuration");

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
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
