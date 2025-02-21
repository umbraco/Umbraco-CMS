using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.HealthCheck.Group;

public class CheckHealthCheckGroupControllerTests : ManagementApiUserGroupTestBase<CheckHealthCheckGroupController>
{
    protected override Expression<Func<CheckHealthCheckGroupController, object>> MethodSelector =>
        x => x.ByNameWithResult(CancellationToken.None, "Configuration");

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.PostAsync(Url, null);
}
