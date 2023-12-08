using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Package.Created;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Package.Created;

public class ByKeyCreatedPackageControllerTests : ManagementApiUserGroupTestBase<ByKeyCreatedPackageController>
{
    protected override Expression<Func<ByKeyCreatedPackageController, object>> MethodSelector =>
        x => x.ByKey(Guid.NewGuid());

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
