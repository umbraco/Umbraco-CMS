using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Tree;

public class RootDataTypeTreeControllerTests : ManagementApiUserGroupTestBase<RootDataTypeTreeController>
{
    protected override Expression<Func<RootDataTypeTreeController, object>> MethodSelector =>
        x => x.Root(CancellationToken.None, 0, 100, false);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden,
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
