using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet.Item;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet.Item;

public class ItemStylesheetItemControllerTests : ManagementApiUserGroupTestBase<ItemStylesheetItemController>
{
    private IStylesheetFolderService StylesheetFolderService => GetRequiredService<IStylesheetFolderService>();

    private string _stylesheetFolderPath;

    [SetUp]
    public async Task SetUp()
    {
        var model = new StylesheetFolderCreateModel { Name = Guid.NewGuid().ToString() };
        var response = await StylesheetFolderService.CreateAsync(model);
        _stylesheetFolderPath = response.Result.Path;
    }

    protected override Expression<Func<ItemStylesheetItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<string> { _stylesheetFolderPath });

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
