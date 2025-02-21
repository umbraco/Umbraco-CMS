using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Template;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template;

public class ByKeyTemplateControllerTests : ManagementApiUserGroupTestBase<ByKeyTemplateController>
{
    protected override Expression<Func<ByKeyTemplateController, object>> MethodSelector => x => x.ByKey(CancellationToken.None, Guid.Empty);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
