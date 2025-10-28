using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Help;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Help;

public class GetHelpControllerTests : ManagementApiUserGroupTestBase<GetHelpController>
{
    protected override Expression<Func<GetHelpController, object>> MethodSelector =>
        x => x.Get(CancellationToken.None, "TestSection", "TestTree", 0, 100, "https://our.umbraco.com");

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
