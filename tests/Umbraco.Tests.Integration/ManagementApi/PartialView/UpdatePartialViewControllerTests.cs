using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.PartialView;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView;

public class UpdatePartialViewControllerTests : ManagementApiUserGroupTestBase<UpdatePartialViewController>
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

    protected override Expression<Func<UpdatePartialViewController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, _partialViewPath, null);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdatePartialViewRequestModel updatePartialViewRequestModel = new() { Content = "NewContent" };

        return await Client.PutAsync(Url, JsonContent.Create(updatePartialViewRequestModel));
    }
}
