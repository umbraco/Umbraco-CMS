using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Script.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script.Tree;

public class ChildrenScriptTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenScriptTreeController>
{
    protected override Expression<Func<ChildrenScriptTreeController, object>> MethodSelector =>
        x => x.Children("TestParentFolder", 0, 100);

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
