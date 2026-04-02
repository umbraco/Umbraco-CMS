using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Website.Caching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Caching;

[TestFixture]
public class RelationOutputCacheEvictionProviderTests
{
    private Mock<IRelationService> _relationServiceMock = null!;
    private Mock<IIdKeyMap> _idKeyMapMock = null!;
    private RelationOutputCacheEvictionProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _relationServiceMock = new Mock<IRelationService>();
        _relationServiceMock
            .Setup(r => r.GetByChildId(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Enumerable.Empty<IRelation>());

        _idKeyMapMock = new Mock<IIdKeyMap>();

        _provider = new RelationOutputCacheEvictionProvider(
            _relationServiceMock.Object,
            _idKeyMapMock.Object,
            NullLogger<RelationOutputCacheEvictionProvider>.Instance);
    }

    [Test]
    public async Task GetAdditionalEvictionTagsAsync_WhenNoRelations_ReturnsEmpty()
    {
        var context = CreateContext(contentId: 1, contentKey: Guid.NewGuid());

        IEnumerable<string> tags = await _provider.GetAdditionalEvictionTagsAsync(context);

        Assert.That(tags, Is.Empty);
    }

    [Test]
    public async Task GetAdditionalEvictionTagsAsync_WhenDocumentRelationExists_ReturnsParentTag()
    {
        var changedKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();
        const int changedId = 10;
        const int parentId = 20;

        SetupRelation(changedId, parentId, parentKey, Constants.Conventions.RelationTypes.RelatedDocumentAlias);

        var context = CreateContext(contentId: changedId, contentKey: changedKey);

        IEnumerable<string> tags = await _provider.GetAdditionalEvictionTagsAsync(context);

        Assert.That(tags, Does.Contain($"umb-content-{parentKey}"));
    }

    [Test]
    public async Task GetAdditionalEvictionTagsAsync_WhenMediaRelationExists_ReturnsEmpty()
    {
        // Media relations are handled by MediaOutputCacheEvictionHandler, not this provider.
        var changedKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();
        const int changedId = 10;
        const int parentId = 30;

        SetupRelation(changedId, parentId, parentKey, Constants.Conventions.RelationTypes.RelatedMediaAlias);

        var context = CreateContext(contentId: changedId, contentKey: changedKey);

        IEnumerable<string> tags = await _provider.GetAdditionalEvictionTagsAsync(context);

        Assert.That(tags, Is.Empty);
    }

    [Test]
    public async Task GetAdditionalEvictionTagsAsync_WhenMultipleRelations_ReturnsAllParentTags()
    {
        var changedKey = Guid.NewGuid();
        var parentKey1 = Guid.NewGuid();
        var parentKey2 = Guid.NewGuid();
        const int changedId = 10;
        const int parentId1 = 20;
        const int parentId2 = 30;

        var relation1 = CreateRelation(parentId1, changedId);
        var relation2 = CreateRelation(parentId2, changedId);

        _relationServiceMock
            .Setup(r => r.GetByChildId(changedId, Constants.Conventions.RelationTypes.RelatedDocumentAlias))
            .Returns(new[] { relation1, relation2 });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentId1, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentKey1));
        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentId2, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentKey2));

        var context = CreateContext(contentId: changedId, contentKey: changedKey);

        IEnumerable<string> tags = await _provider.GetAdditionalEvictionTagsAsync(context);
        var tagList = tags.ToList();

        Assert.That(tagList, Does.Contain($"umb-content-{parentKey1}"));
        Assert.That(tagList, Does.Contain($"umb-content-{parentKey2}"));
    }

    [Test]
    public async Task GetAdditionalEvictionTagsAsync_WhenParentKeyNotFound_SkipsRelation()
    {
        var changedKey = Guid.NewGuid();
        const int changedId = 10;
        const int parentId = 20;

        var relation = CreateRelation(parentId, changedId);
        _relationServiceMock
            .Setup(r => r.GetByChildId(changedId, Constants.Conventions.RelationTypes.RelatedDocumentAlias))
            .Returns(new[] { relation });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Fail());

        var context = CreateContext(contentId: changedId, contentKey: changedKey);

        IEnumerable<string> tags = await _provider.GetAdditionalEvictionTagsAsync(context);

        Assert.That(tags, Is.Empty);
    }

    private void SetupRelation(int childId, int parentId, Guid parentKey, string relationTypeAlias)
    {
        var relation = CreateRelation(parentId, childId);
        _relationServiceMock
            .Setup(r => r.GetByChildId(childId, relationTypeAlias))
            .Returns(new[] { relation });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentKey));
    }

    private static IRelation CreateRelation(int parentId, int childId)
    {
        var relation = new Mock<IRelation>();
        relation.Setup(r => r.ParentId).Returns(parentId);
        relation.Setup(r => r.ChildId).Returns(childId);
        return relation.Object;
    }

    private static OutputCacheContentChangedContext CreateContext(int contentId, Guid contentKey)
        => new(contentId, contentKey, Array.Empty<string>(), Array.Empty<string>());
}
