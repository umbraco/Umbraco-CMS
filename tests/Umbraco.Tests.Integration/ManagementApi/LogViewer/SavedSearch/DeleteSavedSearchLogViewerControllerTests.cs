using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer.SavedSearch;

public class DeleteSavedSearchLogViewerControllerTests : ManagementApiUserGroupTestBase<DeleteSavedSearchLogViewerController>
{
    private ILogViewerService LogViewerService => GetRequiredService<ILogViewerService>();

    [SetUp]
    public async Task Setup()
    {
        await LogViewerService.AddSavedLogQueryAsync("Find All", "SELECT * FROM LogEntries");
    }

    protected override Expression<Func<DeleteSavedSearchLogViewerController, object>> MethodSelector => x => x.Delete(CancellationToken.None, "Find All");

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);
}
