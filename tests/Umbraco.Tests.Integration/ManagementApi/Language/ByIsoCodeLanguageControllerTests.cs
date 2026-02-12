using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Language;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Language;

public class ByIsoCodeLanguageControllerTests : ManagementApiUserGroupTestBase<ByIsoCodeLanguageController>
{
    protected override Expression<Func<ByIsoCodeLanguageController, object>> MethodSelector => x => x.ByIsoCode(CancellationToken.None, "en-US");

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
}
