using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer.SavedSearch;

public class AllSavedSearchLogViewerControllerTests : ManagementApiUserGroupTestBase<AllSavedSearchLogViewerController>
{
    protected override Expression<Func<AllSavedSearchLogViewerController, object>> MethodSelector => x => x.AllSavedSearches(CancellationToken.None, 0, 100);

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
