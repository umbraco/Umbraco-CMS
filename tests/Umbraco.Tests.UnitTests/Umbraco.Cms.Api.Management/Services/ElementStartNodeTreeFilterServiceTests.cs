using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
internal class ElementStartNodeTreeFilterServiceTests
{
    private readonly Mock<IUserStartNodeEntitiesService> _userStartNodeEntitiesServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IDataTypeService> _dataTypeServiceMock = new(MockBehavior.Strict);

    [Test]
    public void GetFilteredRootEntities_Queries_Both_Element_And_ElementContainer_ObjectTypes()
    {
        var expected = new[]
        {
            new UserAccessEntity(CreateEntity(Guid.NewGuid()), true),
        };

        UmbracoObjectTypes[]? capturedObjectTypes = null;
        _userStartNodeEntitiesServiceMock
            .Setup(x => x.RootUserAccessEntities(
                It.IsAny<UmbracoObjectTypes[]>(),
                It.IsAny<int[]>()))
            .Callback<UmbracoObjectTypes[], int[]>((types, _) => capturedObjectTypes = types)
            .Returns(expected);

        var sut = new MultiObjectTypeFilterService(
            _userStartNodeEntitiesServiceMock.Object,
            _dataTypeServiceMock.Object,
            [100],
            [],
            [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer]);

        sut.GetFilteredRootEntities(out _);

        Assert.IsNotNull(capturedObjectTypes);
        Assert.AreEqual(2, capturedObjectTypes!.Length);
        Assert.That(capturedObjectTypes, Contains.Item(UmbracoObjectTypes.Element));
        Assert.That(capturedObjectTypes, Contains.Item(UmbracoObjectTypes.ElementContainer));
    }

    private static IEntitySlim CreateEntity(Guid key)
    {
        var mock = new Mock<IEntitySlim>();
        mock.Setup(e => e.Key).Returns(key);
        return mock.Object;
    }

    /// <summary>
    /// Test service that overrides <see cref="UserStartNodeTreeFilterService.TreeObjectTypes"/>
    /// to return multiple object types, verifying that the base class routes queries through
    /// the array overload.
    /// </summary>
    private sealed class MultiObjectTypeFilterService : UserStartNodeTreeFilterService
    {
        private readonly int[] _startNodeIds;
        private readonly string[] _startNodePaths;
        private readonly UmbracoObjectTypes[] _treeObjectTypes;

        public MultiObjectTypeFilterService(
            IUserStartNodeEntitiesService userStartNodeEntitiesService,
            IDataTypeService dataTypeService,
            int[] startNodeIds,
            string[] startNodePaths,
            UmbracoObjectTypes[] treeObjectTypes)
            : base(userStartNodeEntitiesService, dataTypeService)
        {
            _startNodeIds = startNodeIds;
            _startNodePaths = startNodePaths;
            _treeObjectTypes = treeObjectTypes;
        }

        protected override UmbracoObjectTypes TreeObjectType => _treeObjectTypes[0];

        protected override UmbracoObjectTypes[] TreeObjectTypes => _treeObjectTypes;

        protected override int[] CalculateUserStartNodeIds() => _startNodeIds;

        protected override string[] CalculateUserStartNodePaths() => _startNodePaths;
    }
}
