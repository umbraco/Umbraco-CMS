using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Template.Query;
using Umbraco.Cms.Api.Management.ViewModels.Template.Query;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template.Query;

public class ExecuteTemplateQueryControllerTests : ManagementApiUserGroupTestBase<ExecuteTemplateQueryController>
{
    protected override Expression<Func<ExecuteTemplateQueryController, object>> MethodSelector => x => x.Execute(CancellationToken.None, null);

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

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        TemplateQueryExecuteModel templateQueryExecuteModel = new() { Take = 10 };

        return await Client.PostAsync(Url, JsonContent.Create(templateQueryExecuteModel));
    }
}
