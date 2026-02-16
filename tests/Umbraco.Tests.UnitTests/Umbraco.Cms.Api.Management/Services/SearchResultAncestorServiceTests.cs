using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
public class SearchResultAncestorServiceTests
{
    private Mock<IEntityService> _entityServiceMock = null!;
    private Mock<ILanguageService> _languageServiceMock = null!;
    private SearchResultAncestorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _entityServiceMock = new Mock<IEntityService>();
        _languageServiceMock = new Mock<ILanguageService>();
        _service = new SearchResultAncestorService(_entityServiceMock.Object, _languageServiceMock.Object);
    }

    [Test]
    public async Task ResolveAsync_EmptyEntities_ReturnsEmptyDictionary()
    {
        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([], UmbracoObjectTypes.DataType);

        Assert.That(result, Is.Empty);
        _entityServiceMock.Verify(x => x.GetAll(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()), Times.Never);
        _entityServiceMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_FlatEntityType_ReturnsEmptyAncestors()
    {
        var entity = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100");

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.Member);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[entity.Key], Is.Empty);
        _entityServiceMock.Verify(x => x.GetAll(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()), Times.Never);
        _entityServiceMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_NoAncestors_ReturnsEmptyLists()
    {
        // Path "-1,100" has no ancestors (root is excluded, self is excluded)
        var entity = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100");

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.DataType);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[entity.Key], Is.Empty);
    }

    [Test]
    public async Task ResolveAsync_ContainerAncestor_UsesIndividualGet()
    {
        var entityKey = Guid.NewGuid();
        var ancestorKey = Guid.NewGuid();
        var entity = CreateEntitySlim(entityKey, 200, "-1,100,200");
        var ancestor = CreateEntitySlim(ancestorKey, 100, "-1,100", "Folder A", UmbracoObjectTypes.DataTypeContainer.GetGuid());

        _entityServiceMock
            .Setup(x => x.Get(100))
            .Returns(ancestor);

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.DataType);

        Assert.That(result[entityKey], Has.Count.EqualTo(1));
        Assert.That(result[entityKey][0].Id, Is.EqualTo(ancestorKey));
        Assert.That(result[entityKey][0].Name, Is.EqualTo("Folder A"));

        // Container types use Get(int), not GetAll.
        _entityServiceMock.Verify(x => x.Get(100), Times.Once);
        _entityServiceMock.Verify(x => x.GetAll(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_MultipleContainerAncestors_ReturnedInPathOrder()
    {
        var entityKey = Guid.NewGuid();
        var topKey = Guid.NewGuid();
        var midKey = Guid.NewGuid();
        var entity = CreateEntitySlim(entityKey, 300, "-1,100,200,300");
        var topAncestor = CreateEntitySlim(topKey, 100, "-1,100", "Top", UmbracoObjectTypes.DataTypeContainer.GetGuid());
        var midAncestor = CreateEntitySlim(midKey, 200, "-1,100,200", "Mid", UmbracoObjectTypes.DataTypeContainer.GetGuid());

        _entityServiceMock.Setup(x => x.Get(100)).Returns(topAncestor);
        _entityServiceMock.Setup(x => x.Get(200)).Returns(midAncestor);

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.DataType);

        Assert.That(result[entityKey], Has.Count.EqualTo(2));
        Assert.That(result[entityKey][0].Id, Is.EqualTo(topKey));
        Assert.That(result[entityKey][0].Name, Is.EqualTo("Top"));
        Assert.That(result[entityKey][1].Id, Is.EqualTo(midKey));
        Assert.That(result[entityKey][1].Name, Is.EqualTo("Mid"));
    }

    [Test]
    public async Task ResolveAsync_SharedContainerAncestors_ResolvedOncePerUniqueId()
    {
        var entityKey1 = Guid.NewGuid();
        var entityKey2 = Guid.NewGuid();
        var folderKey = Guid.NewGuid();
        var entity1 = CreateEntitySlim(entityKey1, 200, "-1,100,200");
        var entity2 = CreateEntitySlim(entityKey2, 300, "-1,100,300");
        var folder = CreateEntitySlim(folderKey, 100, "-1,100", "Shared Folder", UmbracoObjectTypes.DataTypeContainer.GetGuid());

        _entityServiceMock.Setup(x => x.Get(100)).Returns(folder);

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity1, entity2], UmbracoObjectTypes.DataType);

        // Both entities share the same ancestor.
        Assert.That(result[entityKey1], Has.Count.EqualTo(1));
        Assert.That(result[entityKey1][0].Id, Is.EqualTo(folderKey));
        Assert.That(result[entityKey2], Has.Count.EqualTo(1));
        Assert.That(result[entityKey2][0].Id, Is.EqualTo(folderKey));

        // Only one Get call for the shared ancestor ID.
        _entityServiceMock.Verify(x => x.Get(100), Times.Once);
    }

    [Test]
    public async Task ResolveAsync_MixedDepthPaths_CorrectAncestorsPerEntity()
    {
        var shallowKey = Guid.NewGuid();
        var deepKey = Guid.NewGuid();
        var folderAKey = Guid.NewGuid();
        var folderBKey = Guid.NewGuid();

        var shallow = CreateEntitySlim(shallowKey, 200, "-1,100,200");
        var deep = CreateEntitySlim(deepKey, 400, "-1,100,300,400");

        var folderA = CreateEntitySlim(folderAKey, 100, "-1,100", "A", UmbracoObjectTypes.DataTypeContainer.GetGuid());
        var folderB = CreateEntitySlim(folderBKey, 300, "-1,100,300", "B", UmbracoObjectTypes.DataTypeContainer.GetGuid());

        _entityServiceMock.Setup(x => x.Get(100)).Returns(folderA);
        _entityServiceMock.Setup(x => x.Get(300)).Returns(folderB);

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([shallow, deep], UmbracoObjectTypes.DataType);

        Assert.That(result[shallowKey], Has.Count.EqualTo(1));
        Assert.That(result[shallowKey][0].Name, Is.EqualTo("A"));

        Assert.That(result[deepKey], Has.Count.EqualTo(2));
        Assert.That(result[deepKey][0].Name, Is.EqualTo("A"));
        Assert.That(result[deepKey][1].Name, Is.EqualTo("B"));
    }

    [Test]
    public async Task ResolveAsync_DocumentType_UsesIndividualGetForContainerAncestors()
    {
        var entity = CreateEntitySlim(Guid.NewGuid(), 200, "-1,100,200");
        var ancestor = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100", "Folder", UmbracoObjectTypes.DocumentTypeContainer.GetGuid());

        _entityServiceMock.Setup(x => x.Get(100)).Returns(ancestor);

        await _service.ResolveAsync([entity], UmbracoObjectTypes.DocumentType);

        _entityServiceMock.Verify(x => x.Get(100), Times.Once);
        _entityServiceMock.Verify(x => x.GetAll(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_MediaType_UsesIndividualGetForContainerAncestors()
    {
        var entity = CreateEntitySlim(Guid.NewGuid(), 200, "-1,100,200");
        var ancestor = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100", "Folder", UmbracoObjectTypes.MediaTypeContainer.GetGuid());

        _entityServiceMock.Setup(x => x.Get(100)).Returns(ancestor);

        await _service.ResolveAsync([entity], UmbracoObjectTypes.MediaType);

        _entityServiceMock.Verify(x => x.Get(100), Times.Once);
        _entityServiceMock.Verify(x => x.GetAll(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_Document_UsesBatchGetAll()
    {
        var entity = CreateEntitySlim(Guid.NewGuid(), 200, "-1,100,200");
        var ancestor = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100", "Parent Doc", UmbracoObjectTypes.Document.GetGuid());

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<int[]>()))
            .Returns([ancestor]);
        _languageServiceMock
            .Setup(x => x.GetDefaultIsoCodeAsync())
            .ReturnsAsync("en-US");

        await _service.ResolveAsync([entity], UmbracoObjectTypes.Document);

        _entityServiceMock.Verify(
            x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<int[]>()),
            Times.Once);
        _entityServiceMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_DocumentWithVariantAncestor_ResolvesNameFromDefaultCulture()
    {
        var entityKey = Guid.NewGuid();
        var ancestorKey = Guid.NewGuid();
        var entity = CreateEntitySlim(entityKey, 200, "-1,100,200");

        var documentAncestor = new Mock<IDocumentEntitySlim>();
        documentAncestor.SetupGet(x => x.Key).Returns(ancestorKey);
        documentAncestor.SetupGet(x => x.Id).Returns(100);
        documentAncestor.SetupGet(x => x.Path).Returns("-1,100");
        documentAncestor.SetupGet(x => x.Name).Returns("Invariant Name");
        documentAncestor.SetupGet(x => x.NodeObjectType).Returns(UmbracoObjectTypes.Document.GetGuid());
        documentAncestor.SetupGet(x => x.CultureNames).Returns(
            new Dictionary<string, string>
            {
                { "en-US", "English Name" },
                { "da-DK", "Danish Name" },
            });

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<int[]>()))
            .Returns([documentAncestor.Object]);
        _languageServiceMock
            .Setup(x => x.GetDefaultIsoCodeAsync())
            .ReturnsAsync("en-US");

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.Document);

        Assert.That(result[entityKey], Has.Count.EqualTo(1));
        Assert.That(result[entityKey][0].Name, Is.EqualTo("English Name"));
    }

    [Test]
    public async Task ResolveAsync_DocumentWithInvariantAncestor_FallsBackToName()
    {
        var entityKey = Guid.NewGuid();
        var ancestorKey = Guid.NewGuid();
        var entity = CreateEntitySlim(entityKey, 200, "-1,100,200");

        var documentAncestor = new Mock<IDocumentEntitySlim>();
        documentAncestor.SetupGet(x => x.Key).Returns(ancestorKey);
        documentAncestor.SetupGet(x => x.Id).Returns(100);
        documentAncestor.SetupGet(x => x.Path).Returns("-1,100");
        documentAncestor.SetupGet(x => x.Name).Returns("Invariant Only");
        documentAncestor.SetupGet(x => x.NodeObjectType).Returns(UmbracoObjectTypes.Document.GetGuid());
        documentAncestor.SetupGet(x => x.CultureNames).Returns(new Dictionary<string, string>());

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<int[]>()))
            .Returns([documentAncestor.Object]);
        _languageServiceMock
            .Setup(x => x.GetDefaultIsoCodeAsync())
            .ReturnsAsync("en-US");

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.Document);

        Assert.That(result[entityKey], Has.Count.EqualTo(1));
        Assert.That(result[entityKey][0].Name, Is.EqualTo("Invariant Only"));
    }

    [Test]
    public async Task ResolveAsync_Media_UsesBatchGetAll()
    {
        var entity = CreateEntitySlim(Guid.NewGuid(), 200, "-1,100,200");
        var ancestor = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100", "Parent Media", UmbracoObjectTypes.Media.GetGuid());

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Media, It.IsAny<int[]>()))
            .Returns([ancestor]);

        await _service.ResolveAsync([entity], UmbracoObjectTypes.Media);

        _entityServiceMock.Verify(
            x => x.GetAll(UmbracoObjectTypes.Media, It.IsAny<int[]>()),
            Times.Once);
        _entityServiceMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_Template_UsesBatchGetAll()
    {
        var entity = CreateEntitySlim(Guid.NewGuid(), 200, "-1,100,200");
        var ancestor = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100", "Parent Template", UmbracoObjectTypes.Template.GetGuid());

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Template, It.IsAny<int[]>()))
            .Returns([ancestor]);

        await _service.ResolveAsync([entity], UmbracoObjectTypes.Template);

        _entityServiceMock.Verify(
            x => x.GetAll(UmbracoObjectTypes.Template, It.IsAny<int[]>()),
            Times.Once);
        _entityServiceMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task ResolveAsync_AncestorNotFoundInLookup_SkipsAncestor()
    {
        var entityKey = Guid.NewGuid();

        // Entity has ancestor IDs 100 and 200, but only 100 exists.
        var entity = CreateEntitySlim(entityKey, 300, "-1,100,200,300");
        var existingAncestor = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100", "Exists", UmbracoObjectTypes.DataTypeContainer.GetGuid());

        _entityServiceMock.Setup(x => x.Get(100)).Returns(existingAncestor);
        _entityServiceMock.Setup(x => x.Get(200)).Returns((IEntitySlim?)null);

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.DataType);

        // Only the found ancestor is included.
        Assert.That(result[entityKey], Has.Count.EqualTo(1));
        Assert.That(result[entityKey][0].Name, Is.EqualTo("Exists"));
    }

    [Test]
    public async Task ResolveAsync_NullEntityName_ReturnsEmptyString()
    {
        var entityKey = Guid.NewGuid();
        var entity = CreateEntitySlim(entityKey, 200, "-1,100,200");
        var ancestor = CreateEntitySlim(Guid.NewGuid(), 100, "-1,100", null, UmbracoObjectTypes.DataTypeContainer.GetGuid());

        _entityServiceMock.Setup(x => x.Get(100)).Returns(ancestor);

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> result =
            await _service.ResolveAsync([entity], UmbracoObjectTypes.DataType);

        Assert.That(result[entityKey][0].Name, Is.EqualTo(string.Empty));
    }

    private static IEntitySlim CreateEntitySlim(
        Guid key,
        int id,
        string path,
        string? name = null,
        Guid? nodeObjectType = null)
    {
        var mock = new Mock<IEntitySlim>();
        mock.SetupGet(x => x.Key).Returns(key);
        mock.SetupGet(x => x.Id).Returns(id);
        mock.SetupGet(x => x.Path).Returns(path);
        mock.SetupGet(x => x.Name).Returns(name);
        mock.SetupGet(x => x.NodeObjectType).Returns(nodeObjectType ?? Guid.Empty);
        return mock.Object;
    }
}
