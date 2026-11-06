using System.Net.Sockets;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Scoping;

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
    public async Task Reset_Password_Message_Is_Sent_And_Notification_Handled_Outside_A_Transaction()
    {
        bool? sentWithinTransaction = null;
        var senderMock = CreateForgotPasswordSenderMock(canSendNow: true);
        senderMock
            .Setup(x => x.SendForgotPassword(It.IsAny<UserForgotPasswordMessage>()))
            .Callback(() => sentWithinTransaction = ScopeAccessor.AmbientScope is not null)
            .Returns(Task.CompletedTask);
        var userService = CreateUserService(forgotPasswordSender: senderMock.Object);
        await CreateResetPasswordUser(userService);
        ForgotPasswordRequestedRecorder.Reset();

        var result = await userService.SendResetPasswordEmailAsync(ResetPasswordUserEmail, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.IsFalse(sentWithinTransaction);
        Assert.AreEqual(1, ForgotPasswordRequestedRecorder.HandledWithinTransaction.Count);
        Assert.IsFalse(ForgotPasswordRequestedRecorder.HandledWithinTransaction[0]);
    }

    private static Mock<IUserForgotPasswordSender> CreateForgotPasswordSenderMock(bool canSendNow)
    {
        var senderMock = new Mock<IUserForgotPasswordSender>();
        senderMock.Setup(x => x.IsPasswordResetConfigured()).Returns(true);
        senderMock.Setup(x => x.IsPasswordResetAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(canSendNow);
        return senderMock;
    }

    private async Task CreateResetPasswordUser(IUserService userService)
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
    }

    private sealed class ForgotPasswordRequestedRecorder : INotificationHandler<UserForgotPasswordRequestedNotification>
    {
        private readonly IScopeAccessor _scopeAccessor;

        public ForgotPasswordRequestedRecorder(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

        public static List<bool> HandledWithinTransaction { get; } = [];

        public static void Reset() => HandledWithinTransaction.Clear();

        public void Handle(UserForgotPasswordRequestedNotification notification)
            => HandledWithinTransaction.Add(_scopeAccessor.AmbientScope is not null);
    }
}
