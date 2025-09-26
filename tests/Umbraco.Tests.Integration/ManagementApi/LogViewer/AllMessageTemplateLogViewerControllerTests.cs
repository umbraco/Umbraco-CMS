using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer;

public class AllMessageTemplateLogViewerControllerTests : ManagementApiUserGroupTestBase<AllMessageTemplateLogViewerController>
{
    protected override Expression<Func<AllMessageTemplateLogViewerController, object>> MethodSelector => x => x.AllMessageTemplates(CancellationToken.None, 0, 100, null, null);

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
