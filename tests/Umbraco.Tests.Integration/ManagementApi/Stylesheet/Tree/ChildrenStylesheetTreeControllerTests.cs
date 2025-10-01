using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet.Tree;

public class ChildrenStylesheetTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenStylesheetTreeController>
{
    private IStylesheetFolderService StylesheetFolderService => GetRequiredService<IStylesheetFolderService>();

    private IStylesheetService StylesheetService => GetRequiredService<IStylesheetService>();

    private string _stylesheetFolderPath;

    [SetUp]
    public async Task SetUp()
    {
        // Stylesheet Folder
        var model = new StylesheetFolderCreateModel { Name = Guid.NewGuid().ToString() };
        var response = await StylesheetFolderService.CreateAsync(model);
        _stylesheetFolderPath = response.Result.Path;

        // Stylesheet
        var modelStylesheet = new StylesheetCreateModel { Name = Guid.NewGuid() + ".css" };
        await StylesheetService.CreateAsync(modelStylesheet, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenStylesheetTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _stylesheetFolderPath, 0, 100);

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
}
