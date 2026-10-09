using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Api.Management.ServerEvents.AccessFilters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

[TestFixture]
public class ServerEventAccessServiceTests
{
    private static readonly Guid _adminKey = Guid.NewGuid();
    private static readonly Guid _restrictedKey = Guid.NewGuid();
    private const string AdminConnection = "admin-connection";
    private const string RestrictedConnection = "restricted-connection";

    // Node 2 ("Home") lives directly under the root at path "-1,2".
    private const int HomeNodeId = 2;
    private const string HomeNodePath = "-1,2";

    [Test]
    public async Task Cannot_Receive_Document_Event_Outside_Start_Node()
    {
        ServerEventAccessService sut = CreateSut();

        // Node 3 sits under the root but outside the restricted user's "Home" start node.
        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Document, PathContext("-1,3"));

        Assert.That(connections, Does.Contain(AdminConnection));
        Assert.That(connections, Does.Not.Contain(RestrictedConnection));
    }

    [Test]
    public async Task Can_Receive_Document_Event_Within_Start_Node()
    {
        ServerEventAccessService sut = CreateSut();

        // Node 4 sits below the restricted user's "Home" start node.
        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Document, PathContext("-1,2,4"));

        Assert.That(connections, Does.Contain(AdminConnection));
        Assert.That(connections, Does.Contain(RestrictedConnection));
    }

    [Test]
    public async Task Cannot_Receive_Media_Event_Outside_Start_Node()
    {
        ServerEventAccessService sut = CreateSut(mediaStartNodes: true);

        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Media, PathContext("-1,3"));

        Assert.That(connections, Does.Contain(AdminConnection));
        Assert.That(connections, Does.Not.Contain(RestrictedConnection));
    }

    [Test]
    public async Task Cannot_Receive_Event_Without_A_Path()
    {
        ServerEventAccessService sut = CreateSut();

        // A document event with no resolvable path must be delivered to nobody, not everybody.
        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Document, new ServerEventRoutingContext { EntityPath = null });

        Assert.That(connections, Is.Empty);
    }

    [Test]
    public async Task Cannot_Resolve_Connections_For_Unfiltered_Source()
    {
        ServerEventAccessService sut = CreateSut();

        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.DocumentType, PathContext("-1,3"));

        Assert.That(connections, Is.Empty);
    }

    [Test]
    public void Can_Apply_Only_To_Document_And_Media()
    {
        ServerEventAccessService sut = CreateSut();

        Assert.Multiple(() =>
        {
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.Document), Is.True);
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.Media), Is.True);
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.DocumentType), Is.False);
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.Language), Is.False);
        });
    }

    [Test]
    public async Task Cannot_Receive_Document_Event_When_Not_Authorized_For_Source()
    {
        // The user is connected and their start node covers the path, but they are not authorized for
        // the document source (source-level authorization is enforced separately from start-node access).
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(_restrictedKey, RestrictedConnection);
        connectionManager.SetAuthorizedEventSources(_restrictedKey, new[] { Constants.ServerEvents.EventSource.Media });

        IUser user = new UserBuilder().WithKey(_restrictedKey).WithStartContentIds(new[] { HomeNodeId }).Build();
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new[] { user });

        var entityService = CreateEntityServiceMock();
        var filters = new ServerEventAccessFilterCollection(() => new IServerEventAccessFilter[]
        {
            new DocumentServerEventAccessFilter(entityService.Object, AppCaches.Disabled),
        });
        var sut = new ServerEventAccessService(connectionManager, userService.Object, filters);

        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Document, PathContext("-1,2,4"));

        Assert.That(connections, Is.Empty);
    }

    [Test]
    public async Task Can_Grant_Document_Access_Only_Within_Start_Node()
    {
        var entityService = CreateEntityServiceMock();
        var filter = new DocumentServerEventAccessFilter(entityService.Object, AppCaches.Disabled);

        IUser restricted = new UserBuilder().WithStartContentIds(new[] { HomeNodeId }).Build();

        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,2,4")), Is.True);
        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,3")), Is.False);
    }

    [Test]
    public async Task Can_Grant_Media_Access_Only_Within_Start_Node()
    {
        var entityService = CreateEntityServiceMock();
        var filter = new MediaServerEventAccessFilter(entityService.Object, AppCaches.Disabled);

        IUser restricted = new UserBuilder().WithStartMediaIds(new[] { HomeNodeId }).Build();

        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,2,4")), Is.True);
        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,3")), Is.False);
    }

    private static ServerEventRoutingContext PathContext(string entityPath) => new() { EntityPath = entityPath };

    private static ServerEventAccessService CreateSut(bool mediaStartNodes = false)
    {
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(_adminKey, AdminConnection);
        connectionManager.AddConnection(_restrictedKey, RestrictedConnection);

        // Both users are authorized (at connect time) for the document and media sources; the per-entity
        // start-node filter then decides delivery.
        string[] authorizedSources =
        [
            Constants.ServerEvents.EventSource.Document,
            Constants.ServerEvents.EventSource.Media,
        ];
        connectionManager.SetAuthorizedEventSources(_adminKey, authorizedSources);
        connectionManager.SetAuthorizedEventSources(_restrictedKey, authorizedSources);

        // The admin has root access (start node -1); the restricted user is scoped to "Home".
        IUser admin = mediaStartNodes
            ? new UserBuilder().WithKey(_adminKey).WithStartMediaIds(new[] { Constants.System.Root }).Build()
            : new UserBuilder().WithKey(_adminKey).WithStartContentIds(new[] { Constants.System.Root }).Build();
        IUser restricted = mediaStartNodes
            ? new UserBuilder().WithKey(_restrictedKey).WithStartMediaIds(new[] { HomeNodeId }).Build()
            : new UserBuilder().WithKey(_restrictedKey).WithStartContentIds(new[] { HomeNodeId }).Build();

        var userService = new Mock<IUserService>();
        userService
            .Setup(x => x.GetAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new[] { admin, restricted });

        var entityService = CreateEntityServiceMock();

        var filters = new ServerEventAccessFilterCollection(() => new IServerEventAccessFilter[]
        {
            new DocumentServerEventAccessFilter(entityService.Object, AppCaches.Disabled),
            new MediaServerEventAccessFilter(entityService.Object, AppCaches.Disabled),
        });

        return new ServerEventAccessService(connectionManager, userService.Object, filters);
    }

    private static Mock<IEntityService> CreateEntityServiceMock()
    {
        var entityService = new Mock<IEntityService>();
        entityService
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns<UmbracoObjectTypes, int[]>((_, ids) => ids.Contains(HomeNodeId)
                ? new[] { new TreeEntityPath { Id = HomeNodeId, Path = HomeNodePath } }
                : Array.Empty<TreeEntityPath>());
        return entityService;
    }
}
