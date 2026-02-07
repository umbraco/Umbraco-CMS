using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class DocumentCollectionPresentationFactoryTests
{
    private Mock<IUmbracoMapper> _mapper = null!;
    private Mock<IPublicAccessService> _publicAccessService = null!;
    private Mock<IEntityService> _entityService = null!;
    private DocumentCollectionPresentationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _mapper = new Mock<IUmbracoMapper>();
        _publicAccessService = new Mock<IPublicAccessService>();
        _entityService = new Mock<IEntityService>();

        _factory = new DocumentCollectionPresentationFactory(
            _mapper.Object,
            new FlagProviderCollection(() => Enumerable.Empty<IFlagProvider>()),
            _publicAccessService.Object,
            _entityService.Object);
    }

    [Test]
    public async Task SetUnmappedProperties_Sets_IsProtected_Correctly()
    {
        // Arrange
        var contentKey1 = Guid.NewGuid();
        var contentKey2 = Guid.NewGuid();
        var contentKey3 = Guid.NewGuid();
        var ancestorKey = Guid.NewGuid();

        var content1 = Mock.Of<IContent>(c => c.Key == contentKey1);
        var content2 = Mock.Of<IContent>(c => c.Key == contentKey2);
        var content3 = Mock.Of<IContent>(c => c.Key == contentKey3);

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(3, new[] { content1, content2, content3 }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var responseModel1 = new DocumentCollectionResponseModel { Id = contentKey1 };
        var responseModel2 = new DocumentCollectionResponseModel { Id = contentKey2 };
        var responseModel3 = new DocumentCollectionResponseModel { Id = contentKey3 };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([responseModel1, responseModel2, responseModel3]);

        // content1 and content2 are protected, content3 is not
        _publicAccessService.Setup(x => x.IsProtected(content1))
            .Returns(Attempt<PublicAccessEntry?>.Succeed(null));
        _publicAccessService.Setup(x => x.IsProtected(content2))
            .Returns(Attempt<PublicAccessEntry?>.Succeed(null));
        _publicAccessService.Setup(x => x.IsProtected(content3))
            .Returns(Attempt<PublicAccessEntry?>.Fail());

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns([ancestorKey]);

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result[0].IsProtected, "Content 1 should be protected");
        Assert.IsTrue(result[1].IsProtected, "Content 2 should be protected");
        Assert.IsFalse(result[2].IsProtected, "Content 3 should not be protected");

        // Verify ancestors are set for all items
        Assert.AreEqual(ancestorKey, result[0].Ancestors.First().Id);
        Assert.AreEqual(ancestorKey, result[1].Ancestors.First().Id);
        Assert.AreEqual(ancestorKey, result[2].Ancestors.First().Id);
    }

    [Test]
    public async Task SetUnmappedProperties_Handles_No_Protected_Content()
    {
        // Arrange
        var contentKey1 = Guid.NewGuid();
        var contentKey2 = Guid.NewGuid();

        var content1 = Mock.Of<IContent>(c => c.Key == contentKey1);
        var content2 = Mock.Of<IContent>(c => c.Key == contentKey2);

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(2, new[] { content1, content2 }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var responseModel1 = new DocumentCollectionResponseModel { Id = contentKey1 };
        var responseModel2 = new DocumentCollectionResponseModel { Id = contentKey2 };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([responseModel1, responseModel2]);

        // Neither item is protected
        _publicAccessService.Setup(x => x.IsProtected(It.IsAny<IContent>()))
            .Returns(Attempt<PublicAccessEntry?>.Fail());

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns(Array.Empty<Guid>());

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsFalse(result[0].IsProtected);
        Assert.IsFalse(result[1].IsProtected);
    }

    [Test]
    public async Task SetUnmappedProperties_Skips_Items_With_No_Matching_Content()
    {
        // Arrange - response model has a key that doesn't match any content item
        var contentKey = Guid.NewGuid();
        var orphanKey = Guid.NewGuid();

        var content = Mock.Of<IContent>(c => c.Key == contentKey);

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(1, new[] { content }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var matchingResponseModel = new DocumentCollectionResponseModel { Id = contentKey };
        var orphanResponseModel = new DocumentCollectionResponseModel { Id = orphanKey };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([matchingResponseModel, orphanResponseModel]);

        _publicAccessService.Setup(x => x.IsProtected(content))
            .Returns(Attempt<PublicAccessEntry?>.Succeed(null));

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns(Array.Empty<Guid>());

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result[0].IsProtected, "Matching content should be protected");
        Assert.IsFalse(result[1].IsProtected, "Orphan item should remain default (not protected)");

        // IsProtected should only be called once (for the matching content)
        _publicAccessService.Verify(x => x.IsProtected(It.IsAny<IContent>()), Times.Once);
    }
}
