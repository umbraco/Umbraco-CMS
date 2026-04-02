using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Install;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Install;

public class SettingsInstallControllerTests : ManagementApiUserGroupTestBase<SettingsInstallController>
{
    protected override Expression<Func<SettingsInstallController, object>> MethodSelector =>
        x => x.Settings(CancellationToken.None);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };
}
