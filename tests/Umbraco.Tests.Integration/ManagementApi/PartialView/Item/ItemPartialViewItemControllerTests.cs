using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Item;

public class ItemPartialViewItemControllerTests : ManagementApiUserGroupTestBase<ItemPartialViewItemController>
{
    private IPartialViewService PartialViewService => GetRequiredService<IPartialViewService>();

    private string _partialViewPath;

    [SetUp]
    public async Task SetUp()
    {
        var model = new PartialViewCreateModel { Name = Guid.NewGuid() + ".cshtml" };
        var response = await PartialViewService.CreateAsync(model, Constants.Security.SuperUserKey);
        _partialViewPath = response.Result.Path;
    }

    protected override Expression<Func<ItemPartialViewItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<string> { _partialViewPath });

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
