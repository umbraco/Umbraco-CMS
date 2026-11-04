using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Tree;

public class BatchDataTypeTreeControllerTests : ManagementApiUserGroupTestBase<BatchDataTypeTreeController>
{
    protected override Expression<Func<BatchDataTypeTreeController, object>> MethodSelector =>
        x => x.Batch(CancellationToken.None, new HashSet<Guid> { Guid.NewGuid() });

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
