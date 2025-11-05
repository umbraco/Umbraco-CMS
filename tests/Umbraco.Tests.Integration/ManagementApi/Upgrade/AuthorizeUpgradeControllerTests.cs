using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Upgrade;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Upgrade;

public class AuthorizeUpgradeControllerTests : ManagementApiUserGroupTestBase<AuthorizeUpgradeController>
{
    protected override Expression<Func<AuthorizeUpgradeController, object>> MethodSelector =>
        x => x.Authorize(CancellationToken.None);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.PostAsync(Url, null);
}
