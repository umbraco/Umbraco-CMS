using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet;
using Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet;

public class InterpolateRichTextRulesControllerTests : ManagementApiUserGroupTestBase<InterpolateRichTextRulesController>
{
    protected override Expression<Func<InterpolateRichTextRulesController, object>> MethodSelector =>
        x => x.InterpolateRichTextRules(null);

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
        InterpolateRichTextStylesheetRequestModel requestModel = new()
        {
            Content = "test content",
            Rules = new List<RichTextRuleViewModel>
            {
                new RichTextRuleViewModel
                {
                    Name = string.Empty,
                    Selector = string.Empty,
                    Styles = string.Empty
                }
            }
        };

        return await Client.PostAsync(Url, JsonContent.Create(requestModel));
    }
}
