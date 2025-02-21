using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.StaticFile.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.StaticFile.Item;

public class ItemStaticFileItemControllerTests : ManagementApiUserGroupTestBase<ItemStaticFileItemController>
{
    protected override Expression<Func<ItemStaticFileItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<string> { "wwwroot" });

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
