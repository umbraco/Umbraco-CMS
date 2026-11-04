using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;


namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Tree;

public class BatchDocumentTypeTreeControllerTests : ManagementApiUserGroupTestBase<BatchDocumentTypeTreeController>
{
    protected override Expression<Func<BatchDocumentTypeTreeController, object>> MethodSelector =>
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
