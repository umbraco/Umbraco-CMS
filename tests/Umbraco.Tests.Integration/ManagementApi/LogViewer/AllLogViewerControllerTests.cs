using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer;

public class AllLogViewerControllerTests : ManagementApiUserGroupTestBase<AllLogViewerController>
{
    protected override Expression<Func<AllLogViewerController, object>> MethodSelector => x => x.AllLogs(CancellationToken.None, 0, 100, Direction.Descending, null, null, null, null);

    // We get the InternalServerError for the admin because it has access, but there is no log file to view
    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.InternalServerError
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
