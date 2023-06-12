using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class UserServiceCrudTests
{
    private ISet<Guid> GetKeysFromIds(IEnumerable<int>? ids, UmbracoObjectTypes type)
    {
        IEnumerable<Guid>? keys = ids?
            .Select(x => EntityService.GetKey(x, type))
            .Where(x => x.Success)
            .Select(x => x.Result);

        return keys is null
            ? new HashSet<Guid>()
            : new HashSet<Guid>(keys);
    }

    private async Task<UserUpdateModel> MapUserToUpdateModel(IUser user)
    {
        var groups = await UserGroupService.GetAsync(user.Groups.Select(x => x.Id).ToArray());
        return new UserUpdateModel
        {
            ExistingUserKey = user.Key,
            Email = user.Email,
            Name = user.Name,
            UserName = user.Username,
            LanguageIsoCode = user.Language,
            ContentStartNodeKeys = GetKeysFromIds(user.StartContentIds, UmbracoObjectTypes.Document),
            MediaStartNodeKeys = GetKeysFromIds(user.StartMediaIds, UmbracoObjectTypes.Media),
            UserGroupKeys = groups.Select(x=>x.Key).ToHashSet(),
        };
    }

    private async Task<(UserUpdateModel updateModel, IUser createdUser)> CreateUserForUpdate(
        IUserService userService,
        string email = "test@test.com",
        string userName = "test@test.com")
    {
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var createUserModel = new UserCreateModel
        {
            Email = email,
            UserName = userName,
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var createExistingUser = await userService.CreateAsync(Constants.Security.SuperUserKey, createUserModel, true);

        Assert.IsTrue(createExistingUser.Success);
        Assert.IsNotNull(createExistingUser.Result.CreatedUser);

        var savedUser = createExistingUser.Result.CreatedUser;
        var updateModel = await MapUserToUpdateModel(savedUser);
        return (updateModel, createExistingUser.Result.CreatedUser);
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task Cannot_Change_Email_When_Deny_Local_Login_Is_True(bool denyLocalLogin, bool shouldSucceed)
    {
        var localLoginSetting = new Mock<ILocalLoginSettingProvider>();
        localLoginSetting.Setup(x => x.HasDenyLocalLogin()).Returns(denyLocalLogin);

        var userService = CreateUserService(
            localLoginSettingProvider: localLoginSetting.Object,
            securitySettings: new SecuritySettings { UsernameIsEmail = false });

        var (updateModel, _) = await CreateUserForUpdate(userService);

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
        // We'll get the user again to ensure that the changes has been persisted
        var updatedUser = await userService.GetAsync(result.Result.Key);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(updatedEmail, updatedUser.Email);
    }

    [Test]
    [TestCase("same@email.com", "same@email.com", true)]
    [TestCase("different@email.com", "another@email.com", false)]
    [TestCase("notAnEmail", "some@email.com", false)]
    public async Task UserName_And_Email_Must_Be_same_When_UserNameIsEmail_Equals_True(string userName, string email, bool shouldSucceed)
    {
        var userService = CreateUserService(securitySettings: new SecuritySettings { UsernameIsEmail = true });

        var (updateModel, createdUser) = await CreateUserForUpdate(userService);

        updateModel.UserName = userName;
        updateModel.Email = email;

        var result = await userService.UpdateAsync(Constants.Security.SuperUserKey, updateModel);

        if (shouldSucceed is false)
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UserOperationStatus.UserNameIsNotEmail, result.Status);
            return;
        }

        Assert.IsTrue(result.Success);
        var updatedUser = await userService.GetAsync(createdUser.Key);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(userName, updatedUser.Username);
        Assert.AreEqual(email, updatedUser.Email);
    }

    [Test]
    public async Task Cannot_Change_Email_To_Duplicate_Email_On_Update()
    {
        var userService = CreateUserService();

        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var email = "thiswillbe@duplicate.com";
        var createModel = new UserCreateModel
        {
            Email = email,
            UserName = email,
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var createExisting = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);

        Assert.IsTrue(createExisting.Success);

        var (updateModel, _) = await CreateUserForUpdate(userService);

        updateModel.Email = email;
        updateModel.UserName = email;

        var updateAttempt = await userService.UpdateAsync(Constants.Security.SuperUserKey, updateModel);

        Assert.IsFalse(updateAttempt.Success);
        Assert.AreEqual(UserOperationStatus.DuplicateEmail, updateAttempt.Status);
    }

    [Test]
    [TestCase("TestUser", "test@user.com", "TestUser", "another@email.com")]
    [TestCase("test@email.com", "test@email.com", "test@email.com", "different@email.com")]
    [TestCase("SomeName", "test@email.com", "test@email.com", "different@email.com")]
    public async Task Cannot_Change_User_Name_To_Duplicate_UserName(string existingUserName, string existingEmail, string updateUserName, string updateEmail)
    {
        // We also ensure that your username cannot be the same as another users email.
        var userService = CreateUserService(new SecuritySettings { UsernameIsEmail = false });
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);

        var createModel = new UserCreateModel
        {
            Email = existingEmail,
            UserName = existingUserName,
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var createExisting = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);
        Assert.IsTrue(createExisting.Success);

        var (updateModel, _) = await CreateUserForUpdate(userService);

        updateModel.Email = updateEmail;
        updateModel.UserName = updateUserName;

        var updateAttempt = await userService.UpdateAsync(Constants.Security.SuperUserKey, updateModel);
        Assert.IsFalse(updateAttempt.Success);
        Assert.AreEqual(UserOperationStatus.DuplicateUserName, updateAttempt.Status);
    }

    [Test]
    [TestCase("en-US", true)]
    [TestCase("Very much not an ISO Code (:", false)]
    [TestCase("da-ZA", false)]
    public async Task Iso_Code_Is_Validated(string isoCode, bool shouldSucceed)
    {
        var userService = CreateUserService();

        var (updateModel, _) = await CreateUserForUpdate(userService);

        updateModel.LanguageIsoCode = isoCode;

        var result = await userService.UpdateAsync(Constants.Security.SuperUserKey, updateModel);

        if (shouldSucceed is false)
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UserOperationStatus.InvalidIsoCode, result.Status);
            return;
        }

        Assert.IsTrue(result.Success);
        // We'll get the user again to ensure that the changes has been persisted
        var updatedUser = await userService.GetAsync(result.Result.Key);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(isoCode, updatedUser.Language);
    }
}
