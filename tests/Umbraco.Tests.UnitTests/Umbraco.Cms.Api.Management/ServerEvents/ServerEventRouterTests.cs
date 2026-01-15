using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

[TestFixture]
public class ServerEventRouterTests
{
    [Test]
    public async Task RouteEventRoutesToEventSourceGroup()
    {
        var mocks = CreateMocks();
        var groupName = "TestSource";
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = groupName, Key = Guid.Empty };
        mocks.HubClientsMock.Setup(x => x.Group(groupName)).Returns(mocks.HubMock.Object);

        var sut = new ServerEventRouter(mocks.HubContextMock.Object, new UserConnectionManager(), CreateRuntimeStateMock().Object, CreateLoggerMock().Object);

        await sut.RouteEventAsync(serverEvent);

        // Group should only be called ONCE
        mocks.HubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Once);
        // And that once time must be with the event source as group name
        mocks.HubClientsMock.Verify(x => x.Group(groupName), Times.Once);
        mocks.HubMock.Verify(x => x.notify(serverEvent), Times.Once);
    }

    [Test]
    public async Task RouteEventDoesNotRouteWhenNotInRunState()
    {
        var mocks = CreateMocks();
        var groupName = "TestSource";
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = groupName, Key = Guid.Empty };
        mocks.HubClientsMock.Setup(x => x.Group(groupName)).Returns(mocks.HubMock.Object);

        var runtimeStateMock = CreateRuntimeStateMock(RuntimeLevel.Install);
        var sut = new ServerEventRouter(mocks.HubContextMock.Object, new UserConnectionManager(), runtimeStateMock.Object, CreateLoggerMock().Object);

        await sut.RouteEventAsync(serverEvent);

        // Should never be called when not in Run state
        mocks.HubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
        mocks.HubMock.Verify(x => x.notify(serverEvent), Times.Never);
    }

    [Test]
    public async Task NotifyUserOnlyNotifiesSpecificUser()
    {
        var targetUserKey = Guid.NewGuid();
        var targetUserConnections = new List<string> { "connection1", "connection2", "connection3" };
        var nonTargetUsers = new Dictionary<Guid, List<string>>();
        nonTargetUsers.Add(Guid.NewGuid(), new List<string> { "connection4", "connection5" });
        nonTargetUsers.Add(Guid.NewGuid(), new List<string> { "connection6", "connection7" });

        var connectionManager = new UserConnectionManager();

        foreach (var connection in targetUserConnections)
        {
            connectionManager.AddConnection(targetUserKey, connection);
        }

        // Let's add some connections for other users
        foreach (var connectionSet in nonTargetUsers)
        {
            foreach (var connection in connectionSet.Value)
            {
                connectionManager.AddConnection(connectionSet.Key, connection);
            }
        }

        var mocks = CreateMocks();
        mocks.HubClientsMock.Setup(x => x.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(mocks.HubMock.Object);

        var serverEvent = new ServerEvent { EventSource = "Source", EventType = "Type", Key = Guid.Empty };
        var sut = new ServerEventRouter(mocks.HubContextMock.Object, connectionManager, CreateRuntimeStateMock().Object, CreateLoggerMock().Object);
        await sut.NotifyUserAsync(serverEvent, targetUserKey);

        mocks.HubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Once());
        mocks.HubClientsMock.Verify(x => x.Clients(targetUserConnections), Times.Once());
        mocks.HubMock.Verify(x => x.notify(serverEvent), Times.Once());
    }

    [Test]
    public async Task NotifyUserOnlyActsIfConnectionsExist()
    {
        var targetUserKey = Guid.NewGuid();
        var nonTargetUsers = new Dictionary<Guid, List<string>>();
        nonTargetUsers.Add(Guid.NewGuid(), new List<string> { "connection4", "connection5" });
        nonTargetUsers.Add(Guid.NewGuid(), new List<string> { "connection6", "connection7" });

        var connectionManager = new UserConnectionManager();

        foreach (var connectionSet in nonTargetUsers)
        {
            foreach (var connection in connectionSet.Value)
            {
                connectionManager.AddConnection(connectionSet.Key, connection);
            }
        }

        // Note that target user has no connections.
        var serverEvent = new ServerEvent { EventSource = "Source", EventType = "Type", Key = Guid.Empty };
        var mocks = CreateMocks();

        var sut = new ServerEventRouter(mocks.HubContextMock.Object, connectionManager, CreateRuntimeStateMock().Object, CreateLoggerMock().Object);

        await sut.NotifyUserAsync(serverEvent, targetUserKey);

        mocks.HubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Never());
        mocks.HubMock.Verify(x => x.notify(serverEvent), Times.Never());
    }

    [Test]
    public async Task NotifyUserDoesNotNotifyWhenNotInRunState()
    {
        var targetUserKey = Guid.NewGuid();
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(targetUserKey, "connection1");

        var serverEvent = new ServerEvent { EventSource = "Source", EventType = "Type", Key = Guid.Empty };
        var mocks = CreateMocks();
        mocks.HubClientsMock.Setup(x => x.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(mocks.HubMock.Object);

        var runtimeStateMock = CreateRuntimeStateMock(RuntimeLevel.Upgrade);
        var sut = new ServerEventRouter(mocks.HubContextMock.Object, connectionManager, runtimeStateMock.Object, CreateLoggerMock().Object);

        await sut.NotifyUserAsync(serverEvent, targetUserKey);

        // Should never be called when not in Run state
        mocks.HubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Never());
        mocks.HubMock.Verify(x => x.notify(serverEvent), Times.Never());
    }

    private (Mock<IServerEventHub> HubMock, Mock<IHubClients<IServerEventHub>> HubClientsMock, Mock<IHubContext<ServerEventHub, IServerEventHub>> HubContextMock) CreateMocks()
    {
        var hubMock = new Mock<IServerEventHub>();
        var hubClients = new Mock<IHubClients<IServerEventHub>>();
        hubClients.Setup(x => x.All).Returns(hubMock.Object);
        var hubContext = new Mock<IHubContext<ServerEventHub, IServerEventHub>>();
        hubContext.Setup(x => x.Clients).Returns(hubClients.Object);
        return (hubMock, hubClients, hubContext);
    }

    private Mock<IRuntimeState> CreateRuntimeStateMock(RuntimeLevel level = RuntimeLevel.Run)
    {
        var mock = new Mock<IRuntimeState>();
        mock.Setup(x => x.Level).Returns(level);
        return mock;
    }

    private Mock<ILogger<ServerEventRouter>> CreateLoggerMock() => new Mock<ILogger<ServerEventRouter>>();
}
