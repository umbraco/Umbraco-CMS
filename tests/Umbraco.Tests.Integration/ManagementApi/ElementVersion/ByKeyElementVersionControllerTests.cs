using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.ElementVersion;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.ElementVersion;

public class ByKeyElementVersionControllerTests : ElementVersionUserGroupTestBase<ByKeyElementVersionController>
{
    protected override Expression<Func<ByKeyElementVersionController, object>> MethodSelector =>
        x => x.ByKey(CancellationToken.None, VersionId);

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
