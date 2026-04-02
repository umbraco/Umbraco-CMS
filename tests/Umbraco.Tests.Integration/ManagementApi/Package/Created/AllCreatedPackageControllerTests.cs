using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Package.Created;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Package.Created;

public class AllCreatedPackageControllerTests : ManagementApiUserGroupTestBase<AllCreatedPackageController>
{
    protected override Expression<Func<AllCreatedPackageController, object>> MethodSelector =>
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
