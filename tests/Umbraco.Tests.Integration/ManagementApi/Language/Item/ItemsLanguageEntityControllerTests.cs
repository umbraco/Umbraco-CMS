using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Language.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Language.Item;

public class ItemsLanguageEntityControllerTests: ManagementApiUserGroupTestBase<ItemsLanguageEntityController>
{
    protected override Expression<Func<ItemsLanguageEntityController, object>> MethodSelector => x => x.Items(new HashSet<string> { "da" });

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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.GetAsync(Url);
}
