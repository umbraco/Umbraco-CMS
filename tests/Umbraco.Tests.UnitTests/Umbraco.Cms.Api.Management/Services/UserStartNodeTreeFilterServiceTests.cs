using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
internal class UserStartNodeTreeFilterServiceTests
{
    private readonly Mock<IUserStartNodeEntitiesService> _userStartNodeEntitiesServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IDataTypeService> _dataTypeServiceMock = new(MockBehavior.Strict);

    [Test]
    public void ShouldBypassStartNodeFiltering_Returns_True_When_User_Has_Root_Access()
    {
        var sut = CreateService([Constants.System.Root], []);

        Assert.IsTrue(sut.ShouldBypassStartNodeFiltering());
    }

    [Test]
    public void ShouldBypassStartNodeFiltering_Returns_False_When_User_Has_No_Root_Access()
    {
        var sut = CreateService([1234], []);

        Assert.IsFalse(sut.ShouldBypassStartNodeFiltering());
    }

    [Test]
    public void ShouldBypassStartNodeFiltering_Returns_True_When_DataType_Ignores_Start_Nodes()
    {
        var dataTypeKey = Guid.NewGuid();
        SetupDataTypeIgnoringStartNodes(dataTypeKey, true);

        var sut = CreateService([1234], []);

        Assert.IsTrue(sut.ShouldBypassStartNodeFiltering(dataTypeKey));
    }

    [Test]
    public void ShouldBypassStartNodeFiltering_Returns_False_When_DataType_Does_Not_Ignore_Start_Nodes()
    {
        var dataTypeKey = Guid.NewGuid();
        SetupDataTypeIgnoringStartNodes(dataTypeKey, false);

        var sut = CreateService([1234], []);

        Assert.IsFalse(sut.ShouldBypassStartNodeFiltering(dataTypeKey));
    }

    [Test]
    public void MapWithAccessFiltering_Returns_Mapped_Entities_Based_On_Access()
    {
        var sut = CreateService([1234], []);

        var entityWithAccess = CreateEntity(Guid.NewGuid());
        var entityWithoutAccess = CreateEntity(Guid.NewGuid());
        var entityNotInMap = CreateEntity(Guid.NewGuid());

        var entities = new[] { entityWithAccess, entityWithoutAccess, entityNotInMap };
        var accessMap = new Dictionary<Guid, bool>
        {
            { entityWithAccess.Key, true },
            { entityWithoutAccess.Key, false },
        };

        var result = sut.MapWithAccessFiltering(
            entities,
            accessMap,
            e => $"access:{e.Key}",
            e => $"no-access:{e.Key}");

        Assert.AreEqual(2, result.Length);
        Assert.AreEqual($"access:{entityWithAccess.Key}", result[0]);
        Assert.AreEqual($"no-access:{entityWithoutAccess.Key}", result[1]);
    }

    [Test]
    public void MapWithAccessFiltering_Returns_Empty_When_No_Entities_In_Access_Map()
    {
        var sut = CreateService([1234], []);

        var entity = CreateEntity(Guid.NewGuid());
        var accessMap = new Dictionary<Guid, bool>();

        var result = sut.MapWithAccessFiltering(
            [entity],
            accessMap,
            e => $"access:{e.Key}",
            e => $"no-access:{e.Key}");

        Assert.IsEmpty(result);
    }

    [Test]
    public void GetFilteredRootEntities_Delegates_To_UserStartNodeEntitiesService()
    {
        var startNodeIds = new[] { 100, 200 };
        var expected = new[]
        {
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), true),
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), false),
        };

        _userStartNodeEntitiesServiceMock
            .Setup(x => x.RootUserAccessEntities(new[] { UmbracoObjectTypes.Document }, startNodeIds))
            .Returns(expected);

        var sut = CreateService(startNodeIds, []);

        var result = sut.GetFilteredRootEntities(out var totalItems);

        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(2, totalItems);
        Assert.IsTrue(result[0].HasAccess);
        Assert.IsFalse(result[1].HasAccess);
    }

    [Test]
    public void GetFilteredChildEntities_Delegates_To_UserStartNodeEntitiesService()
    {
        var startNodePaths = new[] { "-1,100" };
        var parentKey = Guid.NewGuid();
        var ordering = Ordering.By("sortOrder");
        var expected = new[]
        {
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), true),
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), false),
        };
        long totalItems = 2;

        _userStartNodeEntitiesServiceMock
            .Setup(x => x.ChildUserAccessEntities(
                new[] { UmbracoObjectTypes.Document },
                startNodePaths,
                parentKey,
                0,
                50,
                ordering,
                out totalItems))
            .Returns(expected);

        var sut = CreateService([1234], startNodePaths);

        var result = sut.GetFilteredChildEntities(parentKey, 0, 50, ordering, out var total);

        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(result[0].HasAccess);
        Assert.IsFalse(result[1].HasAccess);
    }

    [Test]
    public void GetFilteredSiblingEntities_Delegates_To_UserStartNodeEntitiesService()
    {
        var startNodePaths = new[] { "-1,100" };
        var target = Guid.NewGuid();
        var ordering = Ordering.By("sortOrder");
        var expected = new[]
        {
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), false),
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), true),
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), false),
        };
        long totalBefore = 1;
        long totalAfter = 2;

        _userStartNodeEntitiesServiceMock
            .Setup(x => x.SiblingUserAccessEntities(
                new[] { UmbracoObjectTypes.Document },
                startNodePaths,
                target,
                5,
                10,
                ordering,
                out totalBefore,
                out totalAfter))
            .Returns(expected);

        var sut = CreateService([1234], startNodePaths);

        var result = sut.GetFilteredSiblingEntities(target, 5, 10, ordering, out _, out _);

        Assert.AreEqual(3, result.Length);
        Assert.IsFalse(result[0].HasAccess);
        Assert.IsTrue(result[1].HasAccess);
        Assert.IsFalse(result[2].HasAccess);
    }

    private void SetupDataTypeIgnoringStartNodes(Guid dataTypeKey, bool ignores)
    {
        var dataTypeMock = new Mock<IDataType>();
        dataTypeMock.Setup(d => d.ConfigurationObject).Returns(
            ignores
                ? new StubIgnoreStartNodesConfig { IgnoreUserStartNodes = true }
                : null);
        _dataTypeServiceMock
            .Setup(x => x.GetAsync(dataTypeKey))
            .ReturnsAsync(dataTypeMock.Object);
    }

    private TestUserStartNodeTreeFilterService CreateService(int[] startNodeIds, string[] startNodePaths) =>
        new(_userStartNodeEntitiesServiceMock.Object, _dataTypeServiceMock.Object, startNodeIds, startNodePaths);

    private static IEntitySlim CreateEntity(Guid key)
    {
        var mock = new Mock<IEntitySlim>();
        mock.Setup(e => e.Key).Returns(key);
        return mock.Object;
    }

    private sealed class TestUserStartNodeTreeFilterService : UserStartNodeTreeFilterService
    {
        private readonly int[] _startNodeIds;
        private readonly string[] _startNodePaths;

        public TestUserStartNodeTreeFilterService(
            IUserStartNodeEntitiesService userStartNodeEntitiesService,
            IDataTypeService dataTypeService,
            int[] startNodeIds,
            string[] startNodePaths)
            : base(userStartNodeEntitiesService, dataTypeService)
        {
            _startNodeIds = startNodeIds;
            _startNodePaths = startNodePaths;
        }

        protected override UmbracoObjectTypes TreeObjectType => UmbracoObjectTypes.Document;

        protected override int[] CalculateUserStartNodeIds() => _startNodeIds;

        protected override string[] CalculateUserStartNodePaths() => _startNodePaths;
    }

    private class StubIgnoreStartNodesConfig : IIgnoreUserStartNodesConfig
    {
        public bool IgnoreUserStartNodes { get; set; }
    }
}
