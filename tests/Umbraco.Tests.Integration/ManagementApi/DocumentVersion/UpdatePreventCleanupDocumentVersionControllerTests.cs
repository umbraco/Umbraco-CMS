using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentVersion;

public class UpdatePreventCleanupDocumentVersionControllerTests : DocumentVersionUserGroupTestBase<UpdatePreventCleanupDocumentVersionController>
{
    protected override Expression<Func<UpdatePreventCleanupDocumentVersionController, object>> MethodSelector =>
        x => x.Set(CancellationToken.None, VersionId, true);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    // The Writer group has access to the content section but no rollback permission.
    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.PutAsync(Url, null);
}
