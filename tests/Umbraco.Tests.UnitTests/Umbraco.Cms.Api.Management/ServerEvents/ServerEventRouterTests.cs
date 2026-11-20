using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

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

        var sut = CreateSut(hubContextMock.Object, CreateAccessServiceMock().Object, new Mock<IServerMessenger>().Object);

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

        var sut = CreateSut(hubContextMock.Object, CreateAccessServiceMock().Object, new Mock<IServerMessenger>().Object, RuntimeLevel.Install);

        await sut.RouteEventAsync(serverEvent);

        // Should never be called when not in Run state
        hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
        hubMock.Verify(x => x.notify(serverEvent), Times.Never);
    }

    [Test]
    public async Task Can_Broadcast_Unfiltered_Source_Event_To_Group()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var groupName = "TestSource";
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = groupName, Key = Guid.Empty };
        hubClientsMock.Setup(x => x.Group(groupName)).Returns(hubMock.Object);

        var accessServiceMock = CreateAccessServiceMock();
        accessServiceMock.Setup(x => x.AppliesTo(groupName)).Returns(false);
        var messengerMock = new Mock<IServerMessenger>();

        var sut = CreateSut(hubContextMock.Object, accessServiceMock.Object, messengerMock.Object);

        await sut.RouteEventAsync(serverEvent, new ServerEventRoutingContext { EntityPath = "-1,123" });

        hubClientsMock.Verify(x => x.Group(groupName), Times.Once);
        hubMock.Verify(x => x.notify(serverEvent), Times.Once);
        VerifyNothingDistributed(messengerMock);
    }

    [Test]
    public async Task Can_Distribute_Filtered_Source_Event_To_All_Servers()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var source = Constants.ServerEvents.EventSource.Document;
        var path = "-1,123";
        var key = Guid.NewGuid();
        var serverEvent = new ServerEvent { EventType = "TestEvent", EventSource = source, Key = key };

        var accessServiceMock = CreateAccessServiceMock();
        accessServiceMock.Setup(x => x.AppliesTo(source)).Returns(true);
        var messengerMock = new Mock<IServerMessenger>();

        var sut = CreateSut(hubContextMock.Object, accessServiceMock.Object, messengerMock.Object);

        await sut.RouteEventAsync(serverEvent, new ServerEventRoutingContext { EntityPath = path });

        // Delivery to connections happens on each server when the distributed payload is handled, not here.
        hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
        hubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Never);
        hubMock.Verify(x => x.notify(It.IsAny<ServerEvent>()), Times.Never);
        accessServiceMock.Verify(x => x.GetAuthorizedConnectionsAsync(It.IsAny<string>(), It.IsAny<ServerEventRoutingContext>()), Times.Never);

        messengerMock.Verify(
            x => x.QueueRefresh(
                It.Is<ICacheRefresher>(r => r.RefresherUniqueId == ServerEventCacheRefresher.UniqueId),
                It.Is<ServerEventCacheRefresher.JsonPayload[]>(p =>
                    p.Length == 1
                    && p[0].EventSource == source
                    && p[0].EventType == "TestEvent"
                    && p[0].Key == key
                    && p[0].EntityPath == path
                    && p[0].UserKey == null)),
            Times.Once);
    }

    [Test]
    public async Task Can_Distribute_User_Notification_To_All_Servers()
    {
        var (hubMock, hubClientsMock, hubContextMock) = CreateMocks();
        var userKey = Guid.NewGuid();
        var serverEvent = new ServerEvent { EventSource = "Source", EventType = "Type", Key = userKey };
        var messengerMock = new Mock<IServerMessenger>();

        var sut = CreateSut(hubContextMock.Object, CreateAccessServiceMock().Object, messengerMock.Object);

        await sut.NotifyUserAsync(serverEvent, userKey);

        hubClientsMock.Verify(x => x.Clients(It.IsAny<IReadOnlyList<string>>()), Times.Never);
        hubMock.Verify(x => x.notify(It.IsAny<ServerEvent>()), Times.Never);

        messengerMock.Verify(
            x => x.QueueRefresh(
                It.Is<ICacheRefresher>(r => r.RefresherUniqueId == ServerEventCacheRefresher.UniqueId),
                It.Is<ServerEventCacheRefresher.JsonPayload[]>(p =>
                    p.Length == 1
                    && p[0].EventSource == "Source"
                    && p[0].EventType == "Type"
                    && p[0].Key == userKey
                    && p[0].UserKey == userKey)),
            Times.Once);
    }

    [Test]
    public async Task Cannot_Distribute_When_Not_In_Run_State()
    {
        var (_, _, hubContextMock) = CreateMocks();
        var source = Constants.ServerEvents.EventSource.Document;
        var accessServiceMock = CreateAccessServiceMock();
        accessServiceMock.Setup(x => x.AppliesTo(source)).Returns(true);
        var messengerMock = new Mock<IServerMessenger>();

        var sut = CreateSut(hubContextMock.Object, accessServiceMock.Object, messengerMock.Object, RuntimeLevel.Upgrade);

        await sut.RouteEventAsync(new ServerEvent { EventSource = source, EventType = "Type" }, new ServerEventRoutingContext { EntityPath = "-1,123" });
        await sut.NotifyUserAsync(new ServerEvent { EventSource = "Source", EventType = "Type" }, Guid.NewGuid());

        VerifyNothingDistributed(messengerMock);
    }

    private static ServerEventRouter CreateSut(
        IHubContext<ServerEventHub, IServerEventHub> hubContext,
        IServerEventAccessService accessService,
        IServerMessenger serverMessenger,
        RuntimeLevel level = RuntimeLevel.Run)
    {
        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.Setup(x => x.Level).Returns(level);

        var refresher = new ServerEventCacheRefresher(
            AppCaches.Disabled,
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IEventAggregator>(),
            Mock.Of<ICacheRefresherNotificationFactory>());
        var distributedCache = new DistributedCache(serverMessenger, new CacheRefresherCollection(() => [refresher]));

        return new ServerEventRouter(hubContext, runtimeState.Object, Mock.Of<ILogger<ServerEventRouter>>(), accessService, distributedCache);
    }

    private static void VerifyNothingDistributed(Mock<IServerMessenger> messengerMock)
        => messengerMock.Verify(
            x => x.QueueRefresh(It.IsAny<ICacheRefresher>(), It.IsAny<ServerEventCacheRefresher.JsonPayload[]>()),
            Times.Never);

    private static (Mock<IServerEventHub> HubMock, Mock<IHubClients<IServerEventHub>> HubClientsMock, Mock<IHubContext<ServerEventHub, IServerEventHub>> HubContextMock) CreateMocks()
    {
        var hubMock = new Mock<IServerEventHub>();
        var hubClients = new Mock<IHubClients<IServerEventHub>>();
        hubClients.Setup(x => x.All).Returns(hubMock.Object);
        var hubContext = new Mock<IHubContext<ServerEventHub, IServerEventHub>>();
        hubContext.Setup(x => x.Clients).Returns(hubClients.Object);
        return (hubMock, hubClients, hubContext);
    }

    private static Mock<IServerEventAccessService> CreateAccessServiceMock() => new();
}
