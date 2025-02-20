using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class UserServiceCrudTests
{
    [Test]
    public async Task Can_Reset_Password()
    {
        var securitySettings = new SecuritySettings();
        var userService = CreateUserService(securitySettings);

        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var creationModel = new UserCreateModel
        {
            UserName = "some@one",
            Email = "some@one",
            Name = "Some One",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var userKey = (await userService.CreateAsync(Constants.Security.SuperUserKey, creationModel, true)).Result.CreatedUser!.Key;

        var result = await userService.ResetPasswordAsync(Constants.Security.SuperUserKey, userKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(UserOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result.ResetPassword);
        });
    }

    [Test]
    public async Task Cannot_Reset_Password_For_Api_User()
    {
        var securitySettings = new SecuritySettings();
        var userService = CreateUserService(securitySettings);

        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var creationModel = new UserCreateModel
        {
            UserName = "some@one",
            Email = "some@one",
            Name = "Some One",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
            Kind = UserKind.Api
        };

        var userKey = (await userService.CreateAsync(Constants.Security.SuperUserKey, creationModel, true)).Result.CreatedUser!.Key;

        var result = await userService.ResetPasswordAsync(Constants.Security.SuperUserKey, userKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UserOperationStatus.InvalidUserType, result.Status);
            Assert.IsNull(result.Result.ResetPassword);
            Assert.IsNull(result.Exception);
        });
    }
}
