using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Template.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template.Tree;

public class ChildrenTemplateTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenTemplateTreeController>
{
    protected override Expression<Func<ChildrenTemplateTreeController, object>> MethodSelector => x => x.Children(CancellationToken.None, Guid.Empty, 0, 100);

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
