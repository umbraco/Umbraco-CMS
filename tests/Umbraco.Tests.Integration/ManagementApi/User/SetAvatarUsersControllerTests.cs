using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

public class SetAvatarUsersControllerTests : ManagementApiUserGroupTestBase<SetAvatarUserController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private ITemporaryFileService TemporaryFileService => GetRequiredService<ITemporaryFileService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private Guid _userKey;
    private Guid _avatarKey = Guid.NewGuid();

    [SetUp]
    public async Task SetUp()
    {
        var adminUserGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);

        var stringKey = Guid.NewGuid();
        var model = new UserCreateModel()
        {
            Email = stringKey + "@test.com",
            UserName = stringKey + "@test.com",
            Name = stringKey.ToString(),
            UserGroupKeys = new HashSet<Guid> { adminUserGroup.Key },
        };
        var response = await UserService.CreateAsync(Constants.Security.SuperUserKey, model);
        _userKey = response.Result.CreatedUser.Key;

        await TemporaryFileService.CreateAsync(new CreateTemporaryFileModel { Key = _avatarKey, FileName = Guid.NewGuid() + ".png" });
    }

    protected override Expression<Func<SetAvatarUserController, object>> MethodSelector =>
        x => x.SetAvatar(CancellationToken.None, _userKey, null);

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
        SetAvatarRequestModel setAvatarRequestModel = new() { File = new ReferenceByIdModel(_avatarKey) };
        return await Client.PostAsync(Url, JsonContent.Create(setAvatarRequestModel));
    }
}
