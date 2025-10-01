using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet.Folder;

public class ByPathStylesheetFolderControllerTests : ManagementApiUserGroupTestBase<ByPathStylesheetFolderController>
{
    private IStylesheetFolderService StylesheetFolderService => GetRequiredService<IStylesheetFolderService>();

    private string _stylesheetFolderPath;

    [SetUp]
    public async Task SetUp()
    {
        var model = new StylesheetFolderCreateModel { Name = Guid.NewGuid().ToString()};
        var response =await StylesheetFolderService.CreateAsync(model);
        _stylesheetFolderPath = response.Result.Path;

    }

    protected override Expression<Func<ByPathStylesheetFolderController, object>> MethodSelector =>
        x => x.ByPath(CancellationToken.None, _stylesheetFolderPath);

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
