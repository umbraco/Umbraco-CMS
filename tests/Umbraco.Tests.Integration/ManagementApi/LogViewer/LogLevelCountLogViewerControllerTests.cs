using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer;

public class LogLevelCountLogViewerControllerTests : LogViewerTestBase<LogLevelCountLogViewerController>
{
    protected override Expression<Func<LogLevelCountLogViewerController, object>> MethodSelector => x => x.LogLevelCounts(CancellationToken.None, null, null);

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
