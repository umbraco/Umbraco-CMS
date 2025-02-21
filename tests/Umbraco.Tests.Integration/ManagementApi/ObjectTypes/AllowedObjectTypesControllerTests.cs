using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.ObjectTypes;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.ObjectTypes;

public class AllowedObjectTypesControllerTests : ManagementApiUserGroupTestBase<AllowedObjectTypesController>
{
    protected override Expression<Func<AllowedObjectTypesController, object>> MethodSelector =>
        x => x.Allowed(CancellationToken.None, 0, 100);

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
