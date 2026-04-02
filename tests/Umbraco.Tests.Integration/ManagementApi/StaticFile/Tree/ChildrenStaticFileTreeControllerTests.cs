using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.StaticFile.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.StaticFile.Tree;

public class ChildrenStaticFileTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenStaticFileTreeController>
{
    protected override Expression<Func<ChildrenStaticFileTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, "wwwroot", 0, 100);

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
