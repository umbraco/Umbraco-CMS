using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Package;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Package;

public class AllMigrationStatusPackageControllerTests : ManagementApiUserGroupTestBase<AllMigrationStatusPackageController>
{
    protected override Expression<Func<AllMigrationStatusPackageController, object>> MethodSelector =>
        x => x.AllMigrationStatuses(CancellationToken.None, 0, 100);

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
