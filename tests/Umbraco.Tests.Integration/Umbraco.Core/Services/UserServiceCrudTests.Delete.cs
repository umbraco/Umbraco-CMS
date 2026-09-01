using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class UserServiceCrudTests
{
    [Test]
    public async Task Delete_Returns_Not_Found_If_Not_Found()
    {
        var userService = CreateUserService();
        var result = await userService.DeleteAsync(Constants.Security.SuperUserKey, Guid.NewGuid());
        Assert.That(result, Is.EqualTo(UserOperationStatus.UserNotFound));
    }

    [Test]
    public async Task Cannot_Delete_User_With_Login()
    {
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var userCreateModel = new UserCreateModel
        {
            Email = "test@test.com",
            UserName = "test@test.com",
            Name = "test@test.com",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var userService = CreateUserService();
        var creationResult = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);
        Assert.That(creationResult.Success, Is.True);
        var createdUser = creationResult.Result.CreatedUser;

        createdUser!.LastLoginDate = DateTime.UtcNow;
        userService.Save(createdUser);

        var result = await userService.DeleteAsync(Constants.Security.SuperUserKey, createdUser.Key);
        Assert.That(result, Is.EqualTo(UserOperationStatus.CannotDeleteUserWithLoginHistory));

        // Asset that it is in fact not deleted
        var postDeletedUser = await userService.GetAsync(createdUser.Key);
        Assert.That(postDeletedUser, Is.Not.Null);
        Assert.That(postDeletedUser.Key, Is.EqualTo(createdUser.Key));
    }

    [Test]
    public async Task Can_Delete_User_That_Has_Not_Logged_In()
    {
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var userCreateModel = new UserCreateModel
        {
            Email = "test@test.com",
            UserName = "test@test.com",
            Name = "test@test.com",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var userService = CreateUserService();
        var creationResult = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);
        Assert.That(creationResult.Success, Is.True);

        var deletionResult = await userService.DeleteAsync(Constants.Security.SuperUserKey, creationResult.Result.CreatedUser!.Key);
        Assert.That(deletionResult, Is.EqualTo(UserOperationStatus.Success));
        // Make sure it's actually deleted
        var postDeletedUser = await userService.GetAsync(creationResult.Result.CreatedUser.Key);
        Assert.That(postDeletedUser, Is.Null);
    }
}
