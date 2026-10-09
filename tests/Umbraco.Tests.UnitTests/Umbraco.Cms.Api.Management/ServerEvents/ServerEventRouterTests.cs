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
    public async Task Can_Route_Event_To_Event_Source_Group()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var groupName = "TestSource";
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = groupName, Key = Guid.Empty };
        hubClientsMock.Setup(x => x.Group(groupName)).Returns(hubMock.Object);

        var sut = new ServerEventRouter(hubContextMock.Object, new UserConnectionManager(), CreateRuntimeStateMock().Object, CreateLoggerMock().Object, CreateFilterMock().Object);

        await sut.RouteEventAsync(serverEvent);

        // Group should only be called ONCE
        hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Once);
        // And that once time must be with the event source as group name
        hubClientsMock.Verify(x => x.Group(groupName), Times.Once);
        hubMock.Verify(x => x.notify(serverEvent), Times.Once);
    }

    [Test]
    public async Task Cannot_Route_Event_When_Not_In_Run_State()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var groupName = "TestSource";
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = groupName, Key = Guid.Empty };
        hubClientsMock.Setup(x => x.Group(groupName)).Returns(hubMock.Object);

        var runtimeStateMock = CreateRuntimeStateMock(RuntimeLevel.Install);
        var sut = new ServerEventRouter(hubContextMock.Object, new UserConnectionManager(), runtimeStateMock.Object, CreateLoggerMock().Object, CreateFilterMock().Object);

        await sut.RouteEventAsync(serverEvent);

        // Should never be called when not in Run state
        hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
        hubMock.Verify(x => x.notify(serverEvent), Times.Never);
    }

    [Test]
    public async Task Can_Broadcast_Non_Entity_Source_Event_To_Group()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var groupName = "TestSource";
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = groupName, Key = Guid.Empty };
        hubClientsMock.Setup(x => x.Group(groupName)).Returns(hubMock.Object);

        var filterMock = CreateFilterMock();
        filterMock.Setup(x => x.AppliesTo(groupName)).Returns(false);

        var sut = new ServerEventRouter(hubContextMock.Object, new UserConnectionManager(), CreateRuntimeStateMock().Object, CreateLoggerMock().Object, filterMock.Object);

        await sut.RouteEventAsync(serverEvent, new ServerEventRoutingContext { EntityPath = "-1,123" });

        hubClientsMock.Verify(x => x.Group(groupName), Times.Once);
        hubMock.Verify(x => x.notify(serverEvent), Times.Once);
        filterMock.Verify(x => x.GetAuthorizedConnectionsAsync(It.IsAny<string>(), It.IsAny<ServerEventRoutingContext>()), Times.Never);
    }

    [Test]
    public async Task Can_Route_Entity_Event_To_Authorized_Connections_Only()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var source = Constants.ServerEvents.EventSource.Document;
        var path = "-1,123";
        var authorizedConnections = new List<string> { "connection1", "connection2" };
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = source, Key = Guid.Empty };
        hubClientsMock.Setup(x => x.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(hubMock.Object);

        var filterMock = CreateFilterMock();
        filterMock.Setup(x => x.AppliesTo(source)).Returns(true);
        filterMock.Setup(x => x.GetAuthorizedConnectionsAsync(source, It.Is<ServerEventRoutingContext>(c => c.EntityPath == path))).ReturnsAsync(authorizedConnections);

        var sut = new ServerEventRouter(hubContextMock.Object, new UserConnectionManager(), CreateRuntimeStateMock().Object, CreateLoggerMock().Object, filterMock.Object);

        await sut.RouteEventAsync(serverEvent, new ServerEventRoutingContext { EntityPath = path });

        // Must NOT broadcast to the whole group.
        hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Never);

        // Must only notify the authorized connections.
        hubClientsMock.Verify(x => x.Clients(authorizedConnections), Times.Once);
        hubMock.Verify(x => x.notify(serverEvent), Times.Once);
    }

    [Test]
    public async Task Cannot_Route_Entity_Event_When_No_Authorized_Connections()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var source = Constants.ServerEvents.EventSource.Media;
        var path = "-1,456";
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = source, Key = Guid.Empty };
        hubClientsMock.Setup(x => x.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(hubMock.Object);

        var filterMock = CreateFilterMock();
        filterMock.Setup(x => x.AppliesTo(source)).Returns(true);
        filterMock.Setup(x => x.GetAuthorizedConnectionsAsync(source, It.Is<ServerEventRoutingContext>(c => c.EntityPath == path))).ReturnsAsync(new List<string>());

        var sut = new ServerEventRouter(hubContextMock.Object, new UserConnectionManager(), CreateRuntimeStateMock().Object, CreateLoggerMock().Object, filterMock.Object);

        await sut.RouteEventAsync(serverEvent, new ServerEventRoutingContext { EntityPath = path });

        hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
        hubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Never);
        hubMock.Verify(x => x.notify(serverEvent), Times.Never);
    }

    [Test]
    public async Task Can_Notify_Only_The_Specific_User()
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

        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        hubClientsMock.Setup(x => x.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(hubMock.Object);

        var serverEvent = new ServerEvent { EventSource = "Source", EventType = "Type", Key = Guid.Empty };
        var sut = new ServerEventRouter(hubContextMock.Object, connectionManager, CreateRuntimeStateMock().Object, CreateLoggerMock().Object, CreateFilterMock().Object);
        await sut.NotifyUserAsync(serverEvent, targetUserKey);

        hubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Once());
        hubClientsMock.Verify(x => x.Clients(targetUserConnections), Times.Once());
        hubMock.Verify(x => x.notify(serverEvent), Times.Once());
    }

    [Test]
    public async Task Cannot_Notify_User_When_No_Connections_Exist()
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
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();

        var sut = new ServerEventRouter(hubContextMock.Object, connectionManager, CreateRuntimeStateMock().Object, CreateLoggerMock().Object, CreateFilterMock().Object);

        await sut.NotifyUserAsync(serverEvent, targetUserKey);

        hubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Never());
        hubMock.Verify(x => x.notify(serverEvent), Times.Never());
    }

    [Test]
    public async Task Cannot_Notify_User_When_Not_In_Run_State()
    {
        var targetUserKey = Guid.NewGuid();
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(targetUserKey, "connection1");

        var serverEvent = new ServerEvent { EventSource = "Source", EventType = "Type", Key = Guid.Empty };
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        hubClientsMock.Setup(x => x.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(hubMock.Object);

        var runtimeStateMock = CreateRuntimeStateMock(RuntimeLevel.Upgrade);
        var sut = new ServerEventRouter(hubContextMock.Object, connectionManager, runtimeStateMock.Object, CreateLoggerMock().Object, CreateFilterMock().Object);

        await sut.NotifyUserAsync(serverEvent, targetUserKey);

        // Should never be called when not in Run state
        hubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Never());
        hubMock.Verify(x => x.notify(serverEvent), Times.Never());
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

    private static Mock<IServerEventAccessService> CreateFilterMock() => new Mock<IServerEventAccessService>();
}
