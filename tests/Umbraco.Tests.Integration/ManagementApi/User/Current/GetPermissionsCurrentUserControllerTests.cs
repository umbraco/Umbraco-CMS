using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.User.Current;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class GetPermissionsCurrentUserControllerTests : ManagementApiUserGroupTestBase<GetPermissionsCurrentUserController>
{
    protected override Expression<Func<GetPermissionsCurrentUserController, object>> MethodSelector => x => x.GetPermissions(CancellationToken.None, new HashSet<Guid> { Guid.Empty });

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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
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
