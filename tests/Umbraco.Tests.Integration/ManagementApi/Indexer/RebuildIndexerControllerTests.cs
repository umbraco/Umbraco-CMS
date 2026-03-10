using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Indexer;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Indexer;

public class RebuildIndexerControllerTests : ManagementApiUserGroupTestBase<RebuildIndexerController>
{
    protected override Expression<Func<RebuildIndexerController, object>> MethodSelector =>
        x => x.Rebuild(CancellationToken.None, "MembersIndex");

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.PostAsync(Url, null);
}
