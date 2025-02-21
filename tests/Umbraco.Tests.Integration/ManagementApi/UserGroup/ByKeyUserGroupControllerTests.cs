using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.UserGroup;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.UserGroup;

public class ByKeyUserGroupControllerTests : ManagementApiUserGroupTestBase<ByKeyUserGroupController>
{
    protected override Expression<Func<ByKeyUserGroupController, object>> MethodSelector => x => x.ByKey(CancellationToken.None, Guid.Empty);

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
