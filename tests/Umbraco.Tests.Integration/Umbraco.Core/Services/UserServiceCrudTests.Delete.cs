using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class UserServiceCrudTests
{
    [Test]
    public async Task Delete_Returns_Not_Found_If_Not_Found()
    {
        var userService = CreateUserService();
        var result = await userService.DeleteAsync(Constants.Security.SuperUserKey, Guid.NewGuid());
        Assert.AreEqual(UserOperationStatus.UserNotFound, result);
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
        Assert.IsTrue(creationResult.Success);
        var createdUser = creationResult.Result.CreatedUser;

        createdUser!.LastLoginDate = DateTime.Now;
        userService.Save(createdUser);

        var result = await userService.DeleteAsync(Constants.Security.SuperUserKey, createdUser.Key);
        Assert.AreEqual(UserOperationStatus.CannotDelete, result);

        // Asset that it is in fact not deleted
        var postDeletedUser = await userService.GetAsync(createdUser.Key);
        Assert.IsNotNull(postDeletedUser);
        Assert.AreEqual(createdUser.Key, postDeletedUser.Key);
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
        Assert.IsTrue(creationResult.Success);

        var deletionResult = await userService.DeleteAsync(Constants.Security.SuperUserKey, creationResult.Result.CreatedUser!.Key);
        Assert.AreEqual(UserOperationStatus.Success, deletionResult);
        // Make sure it's actually deleted
        var postDeletedUser = await userService.GetAsync(creationResult.Result.CreatedUser.Key);
        Assert.IsNull(postDeletedUser);
    }
}
