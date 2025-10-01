using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet;

public class ByPathStylesheetControllerTests : ManagementApiUserGroupTestBase<ByPathStylesheetController>
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
    protected override Expression<Func<ByPathStylesheetController, object>> MethodSelector =>
        x => x.ByPath(CancellationToken.None, _stylesheetPath);

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
}
