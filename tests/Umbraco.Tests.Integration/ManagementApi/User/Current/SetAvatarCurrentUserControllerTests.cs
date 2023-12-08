using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Api.Management.ViewModels.User;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class SetAvatarCurrentUserControllerTests : ManagementApiUserGroupTestBase<SetAvatarCurrentUserController>
{
    protected override Expression<Func<SetAvatarCurrentUserController, object>> MethodSelector => x => x.SetAvatar(null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
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
        SetAvatarRequestModel setAvatarRequestModel = new() { FileId = Guid.Empty };
        return await Client.PostAsync(Url, JsonContent.Create(setAvatarRequestModel));
    }
}
