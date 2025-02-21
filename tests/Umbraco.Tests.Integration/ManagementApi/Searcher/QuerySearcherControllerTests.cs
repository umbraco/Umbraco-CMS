using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Searcher;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Searcher;

public class QuerySearcherControllerTests : ManagementApiUserGroupTestBase<QuerySearcherController>
{
    protected override Expression<Func<QuerySearcherController, object>> MethodSelector =>
        x => x.Query(CancellationToken.None, "TestSearcherName", string.Empty, 0, 100);

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
