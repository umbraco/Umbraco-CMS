using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Media.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media.Tree;

public class BatchMediaTreeControllerTests : ManagementApiUserGroupTestBase<BatchMediaTreeController>
{
    protected override Expression<Func<BatchMediaTreeController, object>> MethodSelector => x => x.Batch(CancellationToken.None, new HashSet<Guid> { Guid.NewGuid() }, null);

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
