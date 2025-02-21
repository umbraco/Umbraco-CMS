using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

public class AllDictionaryControllerTests : ManagementApiUserGroupTestBase<AllDictionaryController>
{
    protected override Expression<Func<AllDictionaryController, object>> MethodSelector =>
        x => x.All(CancellationToken.None, null, 0, 100);

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
