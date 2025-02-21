using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer.SavedSearch;

public class ByNameSavedSearchLogViewerControllerTests : ManagementApiUserGroupTestBase<ByNameSavedSearchLogViewerController>
{
    protected override Expression<Func<ByNameSavedSearchLogViewerController, object>> MethodSelector => x => x.ByName(CancellationToken.None, "Test");

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
