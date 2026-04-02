using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

public class UpdateUserGroupsUserControllerTests : ManagementApiUserGroupTestBase<UpdateUserGroupsUserController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private Guid _userKey;
    private Guid _writerUserGroupKey;

    [SetUp]
    public async Task SetUp()
    {
        var adminUserGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);

        var userGroups = await UserGroupService.GetAllAsync(0, 100);
        var writerUserGroup = userGroups.Items.FirstOrDefault(x => x.Name.Contains("Writer", StringComparison.OrdinalIgnoreCase));
        _writerUserGroupKey = writerUserGroup.Key;

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
    }

    protected override Expression<Func<UpdateUserGroupsUserController, object>> MethodSelector => x => x.UpdateUserGroups(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateUserGroupsOnUserRequestModel updateUserGroupsOnUserRequestModel = new()
        {
            UserIds = new HashSet<ReferenceByIdModel> { new ReferenceByIdModel(_userKey) }, UserGroupIds = new HashSet<ReferenceByIdModel> { new ReferenceByIdModel(_writerUserGroupKey) },
        };
        return await Client.PutAsync(Url, JsonContent.Create(updateUserGroupsOnUserRequestModel));
    }
}
