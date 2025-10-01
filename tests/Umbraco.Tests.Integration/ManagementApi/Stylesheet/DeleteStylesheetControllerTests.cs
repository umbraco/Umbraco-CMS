using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet;

public class DeleteStylesheetControllerTests : ManagementApiUserGroupTestBase<DeleteStylesheetController>
{
    private IStylesheetService StylesheetService => GetRequiredService<IStylesheetService>();

    private string _stylesheetPath;
    [SetUp]
    public async Task SetUp()
    {
        var model = new StylesheetCreateModel { Name = Guid.NewGuid() + ".css" };
        var response =await StylesheetService.CreateAsync(model, Constants.Security.SuperUserKey);
        _stylesheetPath = response.Result.Path;

    }

    protected override Expression<Func<DeleteStylesheetController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, _stylesheetPath);

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
