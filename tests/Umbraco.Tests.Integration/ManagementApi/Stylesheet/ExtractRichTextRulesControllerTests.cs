using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet;
using Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet;

public class ExtractRichTextRulesControllerTests : ManagementApiUserGroupTestBase<ExtractRichTextRulesController>
{
    protected override Expression<Func<ExtractRichTextRulesController, object>> MethodSelector =>
        x => x.ExtractRichTextRules(null);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ExtractRichTextStylesheetRulesRequestModel requestModel = new() { Content = "test rich text rule content" };

        return await Client.PostAsync(Url, JsonContent.Create(requestModel));
    }
}
