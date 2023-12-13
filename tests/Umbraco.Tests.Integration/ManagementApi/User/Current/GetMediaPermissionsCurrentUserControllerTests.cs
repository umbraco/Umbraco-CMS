using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.User.Current;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class GetMediaPermissionsCurrentUserControllerTests : ManagementApiUserGroupTestBase<GetMediaPermissionsCurrentUserController>
{
    protected override Expression<Func<GetMediaPermissionsCurrentUserController, object>> MethodSelector => x => x.GetPermissions(new HashSet<Guid> { Guid.Empty });

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
