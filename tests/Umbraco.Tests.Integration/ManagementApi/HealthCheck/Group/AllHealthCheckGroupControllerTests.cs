using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.HealthCheck.Group;

public class AllHealthCheckGroupControllerTests : ManagementApiUserGroupTestBase<AllHealthCheckGroupController>
{
    protected override Expression<Func<AllHealthCheckGroupController, object>> MethodSelector =>
        x => x.All(CancellationToken.None, 0, 100);

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
