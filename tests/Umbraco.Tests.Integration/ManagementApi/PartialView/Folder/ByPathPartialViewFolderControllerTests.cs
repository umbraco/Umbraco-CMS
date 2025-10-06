using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Folder;

public class ByPathPartialViewFolderControllerTests : ManagementApiUserGroupTestBase<ByPathPartialViewFolderController>
{
    private IPartialViewFolderService PartialViewFolderService => GetRequiredService<IPartialViewFolderService>();

    private string _partialViewFolderPath;

    [SetUp]
    public async Task SetUp()
    {
        var model = new PartialViewFolderCreateModel { Name = Guid.NewGuid().ToString() };
        var response = await PartialViewFolderService.CreateAsync(model);
        _partialViewFolderPath = response.Result.Path;
    }

    protected override Expression<Func<ByPathPartialViewFolderController, object>> MethodSelector =>
        x => x.ByPath(CancellationToken.None, _partialViewFolderPath);

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
