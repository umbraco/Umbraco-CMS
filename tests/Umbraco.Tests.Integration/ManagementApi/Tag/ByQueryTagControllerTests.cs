using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Tag;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Tag;

public class ByQueryTagControllerTests : ManagementApiUserGroupTestBase<ByQueryTagController>
{
    protected override Expression<Func<ByQueryTagController, object>> MethodSelector =>
        x => x.ByQuery(CancellationToken.None, string.Empty, string.Empty, string.Empty, 0, 100);

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
