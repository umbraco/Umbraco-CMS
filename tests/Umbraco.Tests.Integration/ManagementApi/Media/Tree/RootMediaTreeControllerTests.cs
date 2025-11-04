using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Media.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media.Tree;

public class RootMediaTreeControllerTests : ManagementApiUserGroupTestBase<RootMediaTreeController>
{
    protected override Expression<Func<RootMediaTreeController, object>> MethodSelector => x => x.Root(CancellationToken.None, 0, 100, null);

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
