using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.PublishedCache;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PublishedCache;

public class RebuildPublishedCacheControllerTests : ManagementApiUserGroupTestBase<RebuildPublishedCacheController>
{
    protected override Expression<Func<RebuildPublishedCacheController, object>> MethodSelector =>
        x => x.Rebuild(CancellationToken.None);

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
