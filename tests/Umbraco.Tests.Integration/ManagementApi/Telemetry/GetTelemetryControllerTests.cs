using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Telemetry;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Telemetry;

public class GetTelemetryControllerTests : ManagementApiUserGroupTestBase<GetTelemetryController>
{
    protected override Expression<Func<GetTelemetryController, object>> MethodSelector =>
        x => x.Get(CancellationToken.None);

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
