using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Relation;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Relation;

public class ByChildRelationControllerTests : ManagementApiUserGroupTestBase<ByChildRelationController>
{
    protected override Expression<Func<ByChildRelationController, object>> MethodSelector =>
        x => x.ByChild(Guid.NewGuid(), 0, 100, null);

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
