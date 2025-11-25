using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Profiling;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Profiling;

public class GetStatusProfilingControllerTests : ManagementApiUserGroupTestBase<GetStatusProfilingController>
{
    protected override Expression<Func<GetStatusProfilingController, object>> MethodSelector =>
        x => x.Status(CancellationToken.None);

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
