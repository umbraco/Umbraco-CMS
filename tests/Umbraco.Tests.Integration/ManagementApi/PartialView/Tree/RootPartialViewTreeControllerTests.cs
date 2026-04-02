using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Tree;

public class RootPartialViewTreeControllerTests : ManagementApiUserGroupTestBase<RootPartialViewTreeController>
{
    private IPartialViewService PartialViewService => GetRequiredService<IPartialViewService>();

    [SetUp]
    public async Task SetUp()
    {
        var model = new PartialViewCreateModel { Name = Guid.NewGuid() + ".cshtml" };
        await PartialViewService.CreateAsync(model, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<RootPartialViewTreeController, object>> MethodSelector =>
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
