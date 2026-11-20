using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class UserServiceCrudTests
{
    private const string ResetPasswordUserEmail = "reset@test.com";

    [TestCase(ResetPasswordUserEmail)]
    [TestCase("unknown@test.com")]
    public async Task Reset_Password_Unavailable_Status_Does_Not_Depend_On_Whether_User_Exists(string email)
    {
        var senderMock = CreateForgotPasswordSenderMock(canSendNow: false);
        var userService = CreateUserService(forgotPasswordSender: senderMock.Object);
        await CreateResetPasswordUser(userService);

        var result = await userService.SendResetPasswordEmailAsync(email, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.PasswordResetUnavailable, result.Result);
        senderMock.Verify(x => x.SendForgotPassword(It.IsAny<UserForgotPasswordMessage>()), Times.Never);
    }

    [TestCase(ResetPasswordUserEmail)]
    [TestCase("unknown@test.com")]
    public async Task Reset_Password_Link_Unavailable_Status_Does_Not_Depend_On_Whether_User_Exists(string email)
    {
        GetRequiredService<TestForgotPasswordUriProviderState>().CanCreateStatus = UserOperationStatus.ApplicationUrlNotConfigured;
        var senderMock = CreateForgotPasswordSenderMock(canSendNow: true);
        var userService = CreateUserService(forgotPasswordSender: senderMock.Object);
        await CreateResetPasswordUser(userService);

        var result = await userService.SendResetPasswordEmailAsync(email, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.ApplicationUrlNotConfigured, result.Result);
        senderMock.Verify(x => x.SendForgotPassword(It.IsAny<UserForgotPasswordMessage>()), Times.Never);
    }

    [Test]
    public async Task Reset_Password_Send_Failure_Is_Returned_As_Failed_Attempt()
    {
        var senderMock = CreateForgotPasswordSenderMock(canSendNow: true);
        senderMock
            .Setup(x => x.SendForgotPassword(It.IsAny<UserForgotPasswordMessage>()))
            .ThrowsAsync(new SocketException());
        var userService = CreateUserService(forgotPasswordSender: senderMock.Object);
        await CreateResetPasswordUser(userService);

        var result = await userService.SendResetPasswordEmailAsync(ResetPasswordUserEmail, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.UnknownFailure, result.Result);
    }

    [Test]
    public async Task Reset_Password_With_Token_Unlocks_Locked_Out_User()
    {
        var userService = CreateUserService();
        IUser user = await CreateLockedOutResetPasswordUser(userService);
        var token = await GeneratePasswordResetToken(user);

        var result = await userService.ResetPasswordAsync(user.Key, token, "N3wP@ssw0rd!");

        Assert.IsTrue(result.Success);
        IUser? updatedUser = await userService.GetAsync(user.Key);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(updatedUser!.IsLockedOut);
            Assert.AreEqual(0, updatedUser.FailedPasswordAttempts);
        });
    }

    [Test]
    public async Task Reset_Password_With_Invalid_Token_Does_Not_Unlock_Locked_Out_User()
    {
        var userService = CreateUserService();
        IUser user = await CreateLockedOutResetPasswordUser(userService);

        var result = await userService.ResetPasswordAsync(user.Key, "invalid".ToUrlBase64()!, "N3wP@ssw0rd!");

        Assert.IsFalse(result.Success);
        IUser? updatedUser = await userService.GetAsync(user.Key);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(updatedUser!.IsLockedOut);
            Assert.AreEqual(5, updatedUser.FailedPasswordAttempts);
        });
    }

    private async Task<IUser> CreateLockedOutResetPasswordUser(IUserService userService)
    {
        IUser user = await CreateResetPasswordUser(userService);
        user.IsLockedOut = true;
        user.FailedPasswordAttempts = 5;
        user.LastLockoutDate = DateTime.UtcNow;
        userService.Save(user);
        return user;
    }

    private async Task<string> GeneratePasswordResetToken(IUser user)
    {
        using IServiceScope scope = GetRequiredService<IServiceScopeFactory>().CreateScope();
        ICoreBackOfficeUserManager userManager = scope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();
        Attempt<string, UserOperationStatus> tokenAttempt = await userManager.GeneratePasswordResetTokenAsync(user);
        Assert.IsTrue(tokenAttempt.Success);
        return tokenAttempt.Result.ToUrlBase64()!;
    }

    private static Mock<IUserForgotPasswordSender> CreateForgotPasswordSenderMock(bool canSendNow)
    {
        var senderMock = new Mock<IUserForgotPasswordSender>();
        senderMock.Setup(x => x.IsPasswordResetConfigured()).Returns(true);
        senderMock.Setup(x => x.IsPasswordResetAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(canSendNow);
        return senderMock;
    }

    private async Task<IUser> CreateResetPasswordUser(IUserService userService)
    {
        IUserGroup? userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var createModel = new UserCreateModel
        {
            UserName = ResetPasswordUserEmail,
            Email = ResetPasswordUserEmail,
            Name = "Reset Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup!.Key },
        };

        var createResult = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);
        Assert.IsTrue(createResult.Success);
        return createResult.Result.CreatedUser!;
    }
}
