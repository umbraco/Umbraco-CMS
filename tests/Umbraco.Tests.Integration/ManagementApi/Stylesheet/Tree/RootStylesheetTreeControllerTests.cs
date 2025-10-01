using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet.Tree;

public class RootStylesheetTreeControllerTests : ManagementApiUserGroupTestBase<RootStylesheetTreeController>
{
    private IStylesheetFolderService StylesheetFolderService => GetRequiredService<IStylesheetFolderService>();

    [SetUp]
    public async Task SetUp()
    {
        // Stylesheet Folder
        var model = new StylesheetFolderCreateModel { Name = Guid.NewGuid().ToString() };
        await StylesheetFolderService.CreateAsync(model);
    }

    protected override Expression<Func<RootStylesheetTreeController, object>> MethodSelector =>
        x => x.Root(CancellationToken.None, 0, 100);

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
