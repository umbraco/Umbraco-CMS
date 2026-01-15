using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Element;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element;

public class ConfigurationElementControllerTests : ManagementApiUserGroupTestBase<ConfigurationElementController>
{
    protected override Expression<Func<ConfigurationElementController, object>> MethodSelector =>
        x => x.Configuration(CancellationToken.None);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };
}
