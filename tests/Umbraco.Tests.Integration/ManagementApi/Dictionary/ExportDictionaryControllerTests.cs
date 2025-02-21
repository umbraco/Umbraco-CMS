using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

public class ExportDictionaryControllerTests : ManagementApiUserGroupTestBase<ExportDictionaryController>
{
    protected override Expression<Func<ExportDictionaryController, object>> MethodSelector =>
            x => x.Export(CancellationToken.None, Guid.NewGuid(), false);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        ExpectedStatusCode = HttpStatusCode.NotFound
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
