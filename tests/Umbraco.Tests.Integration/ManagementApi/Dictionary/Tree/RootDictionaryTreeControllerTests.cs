using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary.Tree;

public class RootDictionaryTreeControllerTests : ManagementApiUserGroupTestBase<RootDictionaryTreeController>
{
    protected override Expression<Func<RootDictionaryTreeController, object>> MethodSelector =>
        x => x.Root(CancellationToken.None, 0, 100);

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
        ExpectedStatusCode = HttpStatusCode.OK
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
