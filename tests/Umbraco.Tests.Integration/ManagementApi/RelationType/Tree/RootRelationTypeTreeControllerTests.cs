using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.RelationType.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.RelationType.Tree;

public class RootRelationTypeTreeControllerTests : ManagementApiUserGroupTestBase<RootRelationTypeTreeController>
{
    protected override Expression<Func<RootRelationTypeTreeController, object>> MethodSelector =>
        x => x.Root(0, 100);

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
