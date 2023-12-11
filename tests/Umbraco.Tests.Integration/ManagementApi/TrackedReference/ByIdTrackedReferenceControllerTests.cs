using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.TrackedReference;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.TrackedReference;

public class ByIdTrackedReferenceControllerTests : ManagementApiUserGroupTestBase<ByIdTrackedReferenceController>
{
    protected override Expression<Func<ByIdTrackedReferenceController, object>> MethodSelector =>
        x => x.Get(Guid.Empty, 0, 100, false);

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
