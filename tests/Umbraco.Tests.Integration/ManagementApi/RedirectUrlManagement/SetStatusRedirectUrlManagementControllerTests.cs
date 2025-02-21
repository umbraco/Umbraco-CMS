using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;
using Umbraco.Cms.Core.Models.RedirectUrlManagement;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.RedirectUrlManagement;

public class SetStatusRedirectUrlManagementControllerTests : ManagementApiUserGroupTestBase<SetStatusRedirectUrlManagementController>
{
    protected override Expression<Func<SetStatusRedirectUrlManagementController, object>> MethodSelector =>
        x => x.SetStatus(CancellationToken.None, RedirectStatus.Enabled);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
