using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Contains unit tests for the <see cref="ServerEventUserManager"/> class, which manages server event user operations in the Umbraco CMS API Management context.
/// </summary>
[TestFixture]
public class ServerEventUserManagerTests
{
    /// <summary>
    /// Verifies that a user is assigned to the correct event source group when AssignToGroupsAsync is called.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AssignsUserToEventSourceGroup()
    {
        var userKey = Guid.NewGuid();
        var user = CreateFakeUser(userKey);
        var authorizationService = CreateServeEventAuthorizationService(new FakeAuthorizer(["source"]));
        var mocks = CreateHubContextMocks();

        // Add a connection to the user
        var connection = "connection1";
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(userKey, connection);

        var sut = new ServerEventUserManager(connectionManager, authorizationService, mocks.HubContextMock.Object);
        await sut.AssignToGroupsAsync(user, connection);

        // Ensure AddToGroupAsync was called once, and only once with the expected parameters.
        mocks.GroupManagerMock.Verify(x => x.AddToGroupAsync(connection, "source", It.IsAny<CancellationToken>()), Times.Once);
        mocks.GroupManagerMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that a user is not assigned to an event source group when they are unauthorized.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoesNotAssignUserToEventSourceGroupWhenUnauthorized()
    {
        var userKey = Guid.NewGuid();
        var user = CreateFakeUser(userKey);
        var authorizationService = CreateServeEventAuthorizationService(new FakeAuthorizer(["source"], (_, _) => false));
        var mocks = CreateHubContextMocks();

        // Add a connection to the user
        var connection = "connection1";
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(userKey, connection);

        var sut = new ServerEventUserManager(connectionManager, authorizationService, mocks.HubContextMock.Object);
        await sut.AssignToGroupsAsync(user, connection);

        // Ensure AddToGroupAsync was never called.
        mocks.GroupManagerMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that RefreshGroupsAsync correctly refreshes user groups by adding and removing connections to groups based on authorization.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RefreshGroupsAsyncRefreshesUserGroups()
    {
        var userKey = Guid.NewGuid();
        var user = CreateFakeUser(userKey);
        var allowedSource = "allowedSource";
        var disallowedSource = "NotAllowed";
        var authorizationService = CreateServeEventAuthorizationService(new FakeAuthorizer([allowedSource]), new FakeAuthorizer([disallowedSource], (_, _) => false));
        var mocks = CreateHubContextMocks();

        // Add a connection to the user
        var connection = "connection1";
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(userKey, connection);

        var sut = new ServerEventUserManager(connectionManager, authorizationService, mocks.HubContextMock.Object);
        await sut.RefreshGroupsAsync(user);

        // Ensure AddToGroupAsync was called once, and only once with the expected parameters.
        mocks.GroupManagerMock.Verify(x => x.AddToGroupAsync(connection, allowedSource, It.IsAny<CancellationToken>()), Times.Once);
        mocks.GroupManagerMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        // Ensure RemoveToGroup was called for the disallowed source, and only the disallowed source.
        mocks.GroupManagerMock.Verify(x => x.RemoveFromGroupAsync(connection, disallowedSource, It.IsAny<CancellationToken>()), Times.Once());
        mocks.GroupManagerMock.Verify(x => x.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    /// <summary>
    /// Tests that refreshing user groups does nothing when there are no active connections for the user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task RefreshUserGroupsDoesNothingIfNoConnections()
    {
        var userKey = Guid.NewGuid();
        var user = CreateFakeUser(userKey);
        var authorizationService = CreateServeEventAuthorizationService(new FakeAuthorizer(["source"]), new FakeAuthorizer(["disallowedSource"], (_, _) => false)) ?? throw new ArgumentNullException("CreateServeEventAuthorizationService(new FakeAuthorizer([\"source\"]), new FakeAuthorizer([\"disallowedSource\"], (_, _) => false))");
        var mocks = CreateHubContextMocks();

        var connectionManager = new UserConnectionManager();

        var sut = new ServerEventUserManager(connectionManager, authorizationService, mocks.HubContextMock.Object);
        await sut.RefreshGroupsAsync(user);

        // Ensure AddToGroupAsync was never called.
        mocks.GroupManagerMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private ClaimsPrincipal CreateFakeUser(Guid key) =>
        new(new ClaimsIdentity([

            // This is the claim that's used to store the ID
            new Claim(Constants.Security.OpenIdDictSubClaimType, key.ToString())
        ]));

    private IServerEventAuthorizationService CreateServeEventAuthorizationService(params IEnumerable<IEventSourceAuthorizer> authorizers)
        => new ServerEventAuthorizationService(new EventSourceAuthorizerCollection(() => authorizers));

    private (Mock<IServerEventHub> HubMock, Mock<IHubClients<IServerEventHub>> HubClientsMock, Mock<IGroupManager> GroupManagerMock, Mock<IHubContext<ServerEventHub, IServerEventHub>> HubContextMock) CreateHubContextMocks()
    {
        var hubMock = new Mock<IServerEventHub>();

        var hubClients = new Mock<IHubClients<IServerEventHub>>();
        hubClients.Setup(x => x.All).Returns(hubMock.Object);

        var groupManagerMock = new Mock<IGroupManager>();

        var hubContext = new Mock<IHubContext<ServerEventHub, IServerEventHub>>();
        hubContext.Setup(x => x.Clients).Returns(hubClients.Object);
        hubContext.Setup(x => x.Groups).Returns(groupManagerMock.Object);
        return (hubMock, hubClients, groupManagerMock, hubContext);
    }

}
