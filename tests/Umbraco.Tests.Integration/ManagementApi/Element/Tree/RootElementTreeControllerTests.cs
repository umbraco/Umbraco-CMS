using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Element.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Tree;

public class RootElementTreeControllerTests : ManagementApiUserGroupTestBase<RootElementTreeController>
{
    protected override Expression<Func<RootElementTreeController, object>> MethodSelector =>
        x => x.Root(CancellationToken.None, 0, 100, false);

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
