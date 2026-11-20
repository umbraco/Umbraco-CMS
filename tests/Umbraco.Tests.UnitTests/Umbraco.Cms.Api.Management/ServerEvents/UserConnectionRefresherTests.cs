using System.Security.Claims;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

[TestFixture]
public class UserConnectionRefresherTests
{
    [Test]
    public async Task Can_Refresh_Groups_Only_For_Users_Connected_To_This_Server()
    {
        var connectedUserKey = Guid.NewGuid();
        var unconnectedUserKey = Guid.NewGuid();
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(connectedUserKey, "connection1");

        var connectedPrincipal = new ClaimsPrincipal();
        var signInManager = new Mock<IBackOfficeSignInManager>();
        signInManager.Setup(x => x.CreateUserPrincipalAsync(connectedUserKey)).ReturnsAsync(connectedPrincipal);
        var userManager = new Mock<IServerEventUserManager>();

        var sut = new UserConnectionRefresher(signInManager.Object, userManager.Object, connectionManager);
        await sut.HandleAsync(CreateNotification(connectedUserKey, unconnectedUserKey), CancellationToken.None);

        userManager.Verify(x => x.RefreshGroupsAsync(connectedPrincipal), Times.Once);
        userManager.Verify(x => x.RefreshGroupsAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        signInManager.Verify(x => x.CreateUserPrincipalAsync(unconnectedUserKey), Times.Never);
    }

    [Test]
    public async Task Can_Refresh_Remaining_Users_When_A_User_Cannot_Be_Resolved()
    {
        var unresolvableUserKey = Guid.NewGuid();
        var resolvableUserKey = Guid.NewGuid();
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(unresolvableUserKey, "connection1");
        connectionManager.AddConnection(resolvableUserKey, "connection2");

        var resolvablePrincipal = new ClaimsPrincipal();
        var signInManager = new Mock<IBackOfficeSignInManager>();
        signInManager.Setup(x => x.CreateUserPrincipalAsync(unresolvableUserKey)).ReturnsAsync((ClaimsPrincipal?)null);
        signInManager.Setup(x => x.CreateUserPrincipalAsync(resolvableUserKey)).ReturnsAsync(resolvablePrincipal);
        var userManager = new Mock<IServerEventUserManager>();

        var sut = new UserConnectionRefresher(signInManager.Object, userManager.Object, connectionManager);
        await sut.HandleAsync(CreateNotification(unresolvableUserKey, resolvableUserKey), CancellationToken.None);

        userManager.Verify(x => x.RefreshGroupsAsync(resolvablePrincipal), Times.Once);
    }

    private static UserCacheRefresherNotification CreateNotification(params Guid[] userKeys)
        => new(
            userKeys.Select(key => new UserCacheRefresher.JsonPayload { Key = key }).ToArray(),
            MessageType.RefreshByPayload);
}
