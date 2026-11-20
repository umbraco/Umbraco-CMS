using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Api.Management.ServerEvents.AccessFilters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

[TestFixture]
public class ServerEventCacheRefresherNotificationHandlerTests
{
    private static readonly Guid _adminKey = Guid.NewGuid();
    private static readonly Guid _restrictedKey = Guid.NewGuid();

    // Node 2 ("Home") lives directly under the root and is the restricted user's start node.
    private const int HomeNodeId = 2;
    private const string HomeNodePath = "-1,2";

    [Test]
    public async Task Can_Deliver_Filtered_Event_To_Authorized_Connections_On_Every_Server()
    {
        var originatingServer = new TestServer();
        originatingServer.Connect(_adminKey, "admin-on-originating-server");

        var otherServer = new TestServer();
        otherServer.Connect(_adminKey, "admin-on-other-server");
        otherServer.Connect(_restrictedKey, "restricted-on-other-server");

        await RaiseOnOriginatingServerAndDeliverEverywhere(
            originatingServer,
            otherServer,
            new ServerEvent { EventSource = Constants.ServerEvents.EventSource.Document, EventType = "Updated", Key = Guid.NewGuid() },
            new ServerEventRoutingContext { EntityPath = "-1,2,4" });

        Assert.Multiple(() =>
        {
            Assert.That(originatingServer.NotifiedConnections, Is.EquivalentTo(new[] { "admin-on-originating-server" }));
            Assert.That(otherServer.NotifiedConnections, Is.EquivalentTo(new[] { "admin-on-other-server", "restricted-on-other-server" }));
        });
    }

    [Test]
    public async Task Cannot_Deliver_Filtered_Event_To_Connections_Without_Access_On_Another_Server()
    {
        var originatingServer = new TestServer();

        var otherServer = new TestServer();
        otherServer.Connect(_adminKey, "admin-on-other-server");
        otherServer.Connect(_restrictedKey, "restricted-on-other-server");

        // Node 3 sits under the root but outside the restricted user's "Home" start node.
        await RaiseOnOriginatingServerAndDeliverEverywhere(
            originatingServer,
            otherServer,
            new ServerEvent { EventSource = Constants.ServerEvents.EventSource.Document, EventType = "Updated", Key = Guid.NewGuid() },
            new ServerEventRoutingContext { EntityPath = "-1,3" });

        Assert.Multiple(() =>
        {
            Assert.That(originatingServer.NotifiedConnections, Is.Empty);
            Assert.That(otherServer.NotifiedConnections, Is.EquivalentTo(new[] { "admin-on-other-server" }));
        });
    }

    [Test]
    public async Task Can_Notify_User_On_Every_Server_Holding_Their_Connections()
    {
        var originatingServer = new TestServer();
        originatingServer.Connect(_restrictedKey, "restricted-on-originating-server");
        originatingServer.Connect(_adminKey, "admin-on-originating-server");

        var otherServer = new TestServer();
        otherServer.Connect(_restrictedKey, "restricted-on-other-server");
        otherServer.Connect(_adminKey, "admin-on-other-server");

        ServerEventCacheRefresher.JsonPayload[] payloads = await CaptureDistributedPayloads(
            originatingServer,
            router => router.NotifyUserAsync(
                new ServerEvent { EventSource = Constants.ServerEvents.EventSource.CurrentUser, EventType = "Updated", Key = _restrictedKey },
                _restrictedKey));
        await DeliverEverywhere(originatingServer, otherServer, payloads);

        Assert.Multiple(() =>
        {
            Assert.That(originatingServer.NotifiedConnections, Is.EquivalentTo(new[] { "restricted-on-originating-server" }));
            Assert.That(otherServer.NotifiedConnections, Is.EquivalentTo(new[] { "restricted-on-other-server" }));
        });
    }

    [Test]
    public async Task Cannot_Deliver_When_Not_In_Run_State()
    {
        var server = new TestServer(RuntimeLevel.Upgrade);
        server.Connect(_adminKey, "admin");

        var payload = new ServerEventCacheRefresher.JsonPayload
        {
            EventSource = Constants.ServerEvents.EventSource.Document,
            EventType = "Updated",
            EntityPath = "-1,2",
        };

        await server.Handler.HandleAsync(
            new ServerEventCacheRefresherNotification(new[] { payload }, MessageType.RefreshByPayload),
            CancellationToken.None);

        Assert.That(server.NotifiedConnections, Is.Empty);
    }

    private static async Task RaiseOnOriginatingServerAndDeliverEverywhere(
        TestServer originatingServer,
        TestServer otherServer,
        ServerEvent serverEvent,
        ServerEventRoutingContext context)
    {
        ServerEventCacheRefresher.JsonPayload[] payloads =
            await CaptureDistributedPayloads(originatingServer, router => router.RouteEventAsync(serverEvent, context));
        await DeliverEverywhere(originatingServer, otherServer, payloads);
    }

    private static async Task<ServerEventCacheRefresher.JsonPayload[]> CaptureDistributedPayloads(
        TestServer originatingServer,
        Func<ServerEventRouter, Task> raise)
    {
        ServerEventCacheRefresher.JsonPayload[]? captured = null;
        var messenger = new Mock<IServerMessenger>();
        messenger
            .Setup(x => x.QueueRefresh(It.IsAny<ICacheRefresher>(), It.IsAny<ServerEventCacheRefresher.JsonPayload[]>()))
            .Callback<ICacheRefresher, ServerEventCacheRefresher.JsonPayload[]>((_, payloads) => captured = payloads);

        var distributedCache = new DistributedCache(
            messenger.Object,
            new CacheRefresherCollection(() => [CreateRefresher(Mock.Of<IEventAggregator>())]));
        var router = new ServerEventRouter(
            originatingServer.HubContext,
            originatingServer.RuntimeState,
            Mock.Of<ILogger<ServerEventRouter>>(),
            originatingServer.AccessService,
            distributedCache);

        await raise(router);

        Assert.That(captured, Is.Not.Null);
        return captured!;
    }

    // Mirrors how the server messenger delivers a payload refresh: the originating server refreshes with the
    // payload objects, while every other server refreshes with the serialized payload.
    private static async Task DeliverEverywhere(TestServer originatingServer, TestServer otherServer, ServerEventCacheRefresher.JsonPayload[] payloads)
    {
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        await DeliverAsync(originatingServer, refresher => refresher.Refresh(payloads), serializer);
        await DeliverAsync(otherServer, refresher => refresher.Refresh(serializer.Serialize(payloads)), serializer);
    }

    private static async Task DeliverAsync(TestServer server, Action<ServerEventCacheRefresher> refresh, IJsonSerializer serializer)
    {
        ServerEventCacheRefresherNotification? notification = null;
        var eventAggregator = new Mock<IEventAggregator>();
        eventAggregator
            .Setup(x => x.Publish(It.IsAny<CacheRefresherNotification>()))
            .Callback<CacheRefresherNotification>(n => notification = n as ServerEventCacheRefresherNotification);

        refresh(CreateRefresher(eventAggregator.Object, serializer));

        Assert.That(notification, Is.Not.Null);
        await server.Handler.HandleAsync(notification!, CancellationToken.None);
    }

    private static ServerEventCacheRefresher CreateRefresher(IEventAggregator eventAggregator, IJsonSerializer? serializer = null)
    {
        var notificationFactory = new Mock<ICacheRefresherNotificationFactory>();
        notificationFactory
            .Setup(x => x.Create<ServerEventCacheRefresherNotification>(It.IsAny<object>(), It.IsAny<MessageType>()))
            .Returns<object, MessageType>((messageObject, messageType) => new ServerEventCacheRefresherNotification(messageObject, messageType));

        return new ServerEventCacheRefresher(
            AppCaches.Disabled,
            serializer ?? Mock.Of<IJsonSerializer>(),
            eventAggregator,
            notificationFactory.Object);
    }

    /// <summary>
    /// A server with its own connections, as each server in a load-balanced setup only knows the connections it holds.
    /// </summary>
    private sealed class TestServer
    {
        private readonly UserConnectionManager _connectionManager = new();
        private readonly List<string> _notifiedConnections = [];

        public TestServer(RuntimeLevel level = RuntimeLevel.Run)
        {
            var runtimeState = new Mock<IRuntimeState>();
            runtimeState.Setup(x => x.Level).Returns(level);
            RuntimeState = runtimeState.Object;

            var hub = new Mock<IServerEventHub>();
            var hubClients = new Mock<IHubClients<IServerEventHub>>();
            hubClients
                .Setup(x => x.Clients(It.IsAny<IReadOnlyList<string>>()))
                .Callback<IReadOnlyList<string>>(connections => _notifiedConnections.AddRange(connections))
                .Returns(hub.Object);
            var hubContext = new Mock<IHubContext<ServerEventHub, IServerEventHub>>();
            hubContext.Setup(x => x.Clients).Returns(hubClients.Object);
            HubContext = hubContext.Object;

            AccessService = CreateAccessService(_connectionManager);
            Handler = new ServerEventCacheRefresherNotificationHandler(
                HubContext,
                _connectionManager,
                AccessService,
                RuntimeState,
                Mock.Of<ILogger<ServerEventCacheRefresherNotificationHandler>>());
        }

        public IRuntimeState RuntimeState { get; }

        public IHubContext<ServerEventHub, IServerEventHub> HubContext { get; }

        public IServerEventAccessService AccessService { get; }

        public ServerEventCacheRefresherNotificationHandler Handler { get; }

        public IReadOnlyList<string> NotifiedConnections => _notifiedConnections;

        public void Connect(Guid userKey, string connectionId)
        {
            _connectionManager.AddConnection(userKey, connectionId);
            _connectionManager.SetAuthorizedEventSources(userKey, [Constants.ServerEvents.EventSource.Document]);
        }
    }

    private static ServerEventAccessService CreateAccessService(UserConnectionManager connectionManager)
    {
        IUser admin = new UserBuilder().WithKey(_adminKey).WithStartContentIds(new[] { Constants.System.Root }).Build();
        IUser restricted = new UserBuilder().WithKey(_restrictedKey).WithStartContentIds(new[] { HomeNodeId }).Build();

        var userService = new Mock<IUserService>();
        userService
            .Setup(x => x.GetAsync(It.IsAny<IEnumerable<Guid>>()))
            .Returns<IEnumerable<Guid>>(keys => Task.FromResult(new[] { admin, restricted }.Where(u => keys.Contains(u.Key))));

        var entityService = new Mock<IEntityService>();
        entityService
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns<UmbracoObjectTypes, int[]>((_, ids) => ids.Contains(HomeNodeId)
                ? new[] { new TreeEntityPath { Id = HomeNodeId, Path = HomeNodePath } }
                : Array.Empty<TreeEntityPath>());

        var filters = new ServerEventAccessFilterCollection(() =>
        [
            new DocumentServerEventAccessFilter(entityService.Object, AppCaches.Disabled),
        ]);

        return new ServerEventAccessService(connectionManager, userService.Object, filters);
    }
}
