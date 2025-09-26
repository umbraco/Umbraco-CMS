using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Template.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template.Item;

public class ItemTemplateItemControllerTests : ManagementApiUserGroupTestBase<ItemTemplateItemController>
{
    protected override Expression<Func<ItemTemplateItemController, object>> MethodSelector => x => x.Item(CancellationToken.None, new HashSet<Guid> { Guid.Empty });

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
