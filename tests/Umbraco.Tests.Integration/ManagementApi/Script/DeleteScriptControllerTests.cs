using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Script;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script;

public class DeleteScriptControllerTests : ManagementApiUserGroupTestBase<DeleteScriptController>
{
    private IScriptService ScriptService => GetRequiredService<IScriptService>();

    private string _scriptPath;
    [SetUp]
    public async Task SetUp()
    {
        var model = new ScriptCreateModel() { Name = Guid.NewGuid() + ".js" };
        var response =await ScriptService.CreateAsync(model, Constants.Security.SuperUserKey);
        _scriptPath = response.Result.Path;

    }
    protected override Expression<Func<DeleteScriptController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, _scriptPath);

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);
}
