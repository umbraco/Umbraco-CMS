using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Indexer;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Indexer;

public class DetailsIndexerControllerTests : ManagementApiUserGroupTestBase<DetailsIndexerController>
{
    protected override Expression<Func<DetailsIndexerController, object>> MethodSelector =>
        x => x.Details(CancellationToken.None, "DeliveryApiContentIndex");

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
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
