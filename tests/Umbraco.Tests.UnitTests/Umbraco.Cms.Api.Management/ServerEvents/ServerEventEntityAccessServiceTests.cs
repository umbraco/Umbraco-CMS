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
public class ServerEventEntityAccessServiceTests
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
        IServerEventEntityAccessService sut = CreateSut();

        // Node 3 sits under the root but outside the restricted user's "Home" start node.
        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Document, PathContext("-1,3"));

        Assert.That(connections, Does.Contain(AdminConnection));
        Assert.That(connections, Does.Not.Contain(RestrictedConnection));
    }

    [Test]
    public async Task Can_Receive_Document_Event_Within_Start_Node()
    {
        IServerEventEntityAccessService sut = CreateSut();

        // Node 4 sits below the restricted user's "Home" start node.
        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Document, PathContext("-1,2,4"));

        Assert.That(connections, Does.Contain(AdminConnection));
        Assert.That(connections, Does.Contain(RestrictedConnection));
    }

    [Test]
    public async Task Cannot_Receive_Media_Event_Outside_Start_Node()
    {
        IServerEventEntityAccessService sut = CreateSut(mediaStartNodes: true);

        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Media, PathContext("-1,3"));

        Assert.That(connections, Does.Contain(AdminConnection));
        Assert.That(connections, Does.Not.Contain(RestrictedConnection));
    }

    [Test]
    public async Task Cannot_Receive_Event_Without_A_Path()
    {
        IServerEventEntityAccessService sut = CreateSut();

        // A document event with no resolvable path must be delivered to nobody, not everybody.
        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.Document, new ServerEventRoutingContext { EntityPath = null });

        Assert.That(connections, Is.Empty);
    }

    [Test]
    public async Task Cannot_Resolve_Connections_For_Unfiltered_Source()
    {
        IServerEventEntityAccessService sut = CreateSut();

        IReadOnlyList<string> connections =
            await sut.GetAuthorizedConnectionsAsync(Constants.ServerEvents.EventSource.DocumentType, PathContext("-1,3"));

        Assert.That(connections, Is.Empty);
    }

    [Test]
    public void Can_Apply_Only_To_Document_And_Media()
    {
        IServerEventEntityAccessService sut = CreateSut();

        Assert.Multiple(() =>
        {
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.Document), Is.True);
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.Media), Is.True);
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.DocumentType), Is.False);
            Assert.That(sut.AppliesTo(Constants.ServerEvents.EventSource.Language), Is.False);
        });
    }

    [Test]
    public async Task Can_Grant_Document_Access_Only_Within_Start_Node()
    {
        var entityService = CreateEntityServiceMock();
        var filter = new DocumentServerEventAccessFilter(entityService.Object, AppCaches.Disabled);

        IUser restricted = BuildContentUser(Guid.NewGuid(), HomeNodeId, Constants.Applications.Content);

        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,2,4")), Is.True);
        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,3")), Is.False);
    }

    [Test]
    public async Task Can_Grant_Media_Access_Only_Within_Start_Node()
    {
        var entityService = CreateEntityServiceMock();
        var filter = new MediaServerEventAccessFilter(entityService.Object, AppCaches.Disabled);

        IUser restricted = BuildMediaUser(Guid.NewGuid(), HomeNodeId, Constants.Applications.Media);

        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,2,4")), Is.True);
        Assert.That(await filter.HasAccessAsync(restricted, PathContext("-1,3")), Is.False);
    }

    [Test]
    public async Task Cannot_Grant_Document_Access_Without_Content_Section()
    {
        var entityService = CreateEntityServiceMock();
        var filter = new DocumentServerEventAccessFilter(entityService.Object, AppCaches.Disabled);

        // The start node covers the path, but the user only has the Media section, not Content.
        IUser user = BuildContentUser(Guid.NewGuid(), HomeNodeId, Constants.Applications.Media);

        Assert.That(await filter.HasAccessAsync(user, PathContext("-1,2,4")), Is.False);
    }

    private static ServerEventRoutingContext PathContext(string entityPath) => new() { EntityPath = entityPath };

    private static ServerEventEntityAccessService CreateSut(bool mediaStartNodes = false)
    {
        var connectionManager = new UserConnectionManager();
        connectionManager.AddConnection(_adminKey, AdminConnection);
        connectionManager.AddConnection(_restrictedKey, RestrictedConnection);

        // The admin has root access (start node -1); the restricted user is scoped to "Home".
        // Both are granted the relevant section so they pass the source-level authorization.
        IUser admin = mediaStartNodes
            ? BuildMediaUser(_adminKey, Constants.System.Root, Constants.Applications.Media)
            : BuildContentUser(_adminKey, Constants.System.Root, Constants.Applications.Content);
        IUser restricted = mediaStartNodes
            ? BuildMediaUser(_restrictedKey, HomeNodeId, Constants.Applications.Media)
            : BuildContentUser(_restrictedKey, HomeNodeId, Constants.Applications.Content);

        var userService = new Mock<IUserService>();
        userService
            .Setup(x => x.GetAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new[] { admin, restricted });

        var entityService = CreateEntityServiceMock();

        var filters = new ServerEventEntityAccessFilterCollection(() => new IServerEventEntityAccessFilter[]
        {
            new DocumentServerEventAccessFilter(entityService.Object, AppCaches.Disabled),
            new MediaServerEventAccessFilter(entityService.Object, AppCaches.Disabled),
        });

        return new ServerEventEntityAccessService(connectionManager, userService.Object, filters);
    }

    // Builds a user scoped to a single content start node and granted the given sections (via a group
    // whose start node matches, so it does not widen access to the root default).
    private static IUser BuildContentUser(Guid key, int startNode, params string[] sections) =>
        new UserBuilder()
            .WithKey(key)
            .WithStartContentIds(new[] { startNode })
            .AddUserGroup()
                .WithAllowedSections(sections)
                .WithStartContentId(startNode)
            .Done()
            .Build();

    private static IUser BuildMediaUser(Guid key, int startNode, params string[] sections) =>
        new UserBuilder()
            .WithKey(key)
            .WithStartMediaIds(new[] { startNode })
            .AddUserGroup()
                .WithAllowedSections(sections)
                .WithStartMediaId(startNode)
            .Done()
            .Build();

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
