using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class UserServiceCrudTests
{
    private SortedSet<Guid> GetKeysFromIds(IEnumerable<int>? ids, UmbracoObjectTypes type)
    {
        IEnumerable<Guid>? keys = ids?
            .Select(x => EntityService.GetKey(x, type))
            .Where(x => x.Success)
            .Select(x => x.Result);

        return keys is null
            ? new SortedSet<Guid>()
            : new SortedSet<Guid>(keys);
    }

    private async Task<UserUpdateModel> MapUserToUpdateModel(IUser user)
    {
        var groups = await UserGroupService.GetAsync(user.Groups.Select(x => x.Id).ToArray());
        return new UserUpdateModel
        {
            ExistingUser = user,
            Email = user.Email,
            Name = user.Name,
            UserName = user.Username,
            Language = user.Language,
            ContentStartNodeKeys = GetKeysFromIds(user.StartContentIds, UmbracoObjectTypes.Document),
            MediaStartNodeKeys = GetKeysFromIds(user.StartMediaIds, UmbracoObjectTypes.Media),
            UserGroups = groups,
        };
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task Cannot_Change_Email_When_Deny_Local_Login_Is_True(bool denyLocalLogin, bool shouldSucceed)
    {
        var localLoginSetting = new Mock<ILocalLoginSettingProvider>();
        localLoginSetting.Setup(x => x.HasDenyLocalLogin()).Returns(denyLocalLogin);

        var userService = CreateUserService(localLoginSettingProvider: localLoginSetting.Object);

        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var createUserModel = new UserCreateModel
        {
            Email = "test@test.com",
            UserName = "test@test.com",
            Name = "Test Mc. Gee",
            UserGroups = new SortedSet<IUserGroup> { userGroup! }
        };

        var createExistingUser = await userService.CreateAsync(Constants.Security.SuperUserKey, createUserModel, true);

        Assert.IsTrue(createExistingUser.Success);
        Assert.IsNotNull(createExistingUser.Result.CreatedUser);

        var savedUser = createExistingUser.Result.CreatedUser;
        var updateModel = await MapUserToUpdateModel(savedUser);

        var updatedEmail = "updated@email.com"; 
        updateModel.Email = updatedEmail;

        var result = await userService.UpdateAsync(Constants.Security.SuperUserKey, updateModel);

        if (shouldSucceed is false)
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UserOperationStatus.EmailCannotBeChanged, result.Status);
            return;
        }

        Assert.IsTrue(result.Success);
        Assert.AreEqual(updatedEmail, result.Result.Email);
    }
}
