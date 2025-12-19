using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentTypeSchemaServiceTests
{
    private Mock<IContentTypeService> _contentTypeServiceMock = null!;
    private Mock<IMediaTypeService> _mediaTypeServiceMock = null!;
    private Mock<IPublishedContentTypeCache> _publishedContentTypeCacheMock = null!;
    private ContentTypeSchemaService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _contentTypeServiceMock = new Mock<IContentTypeService>();
        _mediaTypeServiceMock = new Mock<IMediaTypeService>();
        _publishedContentTypeCacheMock = new Mock<IPublishedContentTypeCache>();

        _sut = new ContentTypeSchemaService(
            _contentTypeServiceMock.Object,
            _mediaTypeServiceMock.Object,
            _publishedContentTypeCacheMock.Object,
            new DefaultShortStringHelper(new DefaultShortStringHelperConfig()));
    }

    [Test]
    public void GetDocumentTypes_SkipsContentTypesWhenCacheThrows()
    {
        // Arrange
        var cachedType = Mock.Of<IContentType>(x => x.Alias == "cachedType" && x.PropertyTypes == Array.Empty<IPropertyType>());
        var uncachedType = Mock.Of<IContentType>(x => x.Alias == "uncachedType");

        _contentTypeServiceMock.Setup(x => x.GetAll()).Returns([cachedType, uncachedType]);
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, "cachedType"))
            .Returns(Mock.Of<IPublishedContentType>(x =>
                x.Alias == "cachedType" &&
                x.IsElement == false &&
                x.CompositionAliases == new HashSet<string>() &&
                x.PropertyTypes == Array.Empty<IPublishedPropertyType>()));
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, "uncachedType"))
            .Throws<Exception>();

        // Act
        var result = _sut.GetDocumentTypes();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Alias, Is.EqualTo("cachedType"));
        Assert.That(result.First().SchemaId, Is.EqualTo("Cachedtype"));
    }

    [Test]
    public void GetDocumentTypes_CorrectlyIdentifiesInheritedProperties()
    {
        // Arrange
        var contentType = Mock.Of<IContentType>(x =>
            x.Alias == "composedType" &&
            x.PropertyTypes == new[] { Mock.Of<IPropertyType>(p => p.Alias == "ownProperty") });

        var publishedContentType = Mock.Of<IPublishedContentType>(x =>
            x.Alias == "composedType" &&
            x.IsElement == false &&
            x.CompositionAliases == new HashSet<string> { "baseComposition" } &&
            x.PropertyTypes == new[]
            {
                Mock.Of<IPublishedPropertyType>(p => p.Alias == "ownProperty" && p.EditorAlias == "test" && p.DeliveryApiModelClrType == typeof(string)),
                Mock.Of<IPublishedPropertyType>(p => p.Alias == "inheritedProperty" && p.EditorAlias == "test" && p.DeliveryApiModelClrType == typeof(string)),
            });

        _contentTypeServiceMock.Setup(x => x.GetAll()).Returns([contentType]);
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, "composedType")).Returns(publishedContentType);

        // Act
        var result = _sut.GetDocumentTypes();

        // Assert
        var props = result.First().Properties;
        Assert.That(props.First(p => p.Alias == "ownProperty").Inherited, Is.False);
        Assert.That(props.First(p => p.Alias == "inheritedProperty").Inherited, Is.True);
    }

    [Test]
    public void GetDocumentTypes_IncludesCompositionSchemaIds()
    {
        // Arrange
        var contentType = Mock.Of<IContentType>(x => x.Alias == "articlePage" && x.PropertyTypes == Array.Empty<IPropertyType>());
        var publishedContentType = Mock.Of<IPublishedContentType>(x =>
            x.Alias == "articlePage" &&
            x.IsElement == false &&
            x.CompositionAliases == new HashSet<string> { "basePage", "seoComposition" } &&
            x.PropertyTypes == Array.Empty<IPublishedPropertyType>());

        _contentTypeServiceMock.Setup(x => x.GetAll()).Returns([contentType]);
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, "articlePage")).Returns(publishedContentType);

        // Act
        var result = _sut.GetDocumentTypes();

        // Assert
        var schema = result.First();
        Assert.That(schema.SchemaId, Is.EqualTo("Articlepage"));
        Assert.That(schema.CompositionSchemaIds, Is.EquivalentTo(new[] { "Basepage", "Seocomposition" }));
    }

    [Test]
    public void GetDocumentTypes_ReturnsEmptyCollectionWhenNoContentTypes()
    {
        // Arrange
        _contentTypeServiceMock.Setup(x => x.GetAll()).Returns(Array.Empty<IContentType>());

        // Act
        var result = _sut.GetDocumentTypes();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetDocumentTypes_CorrectlySetsIsElementProperty()
    {
        // Arrange
        var documentType = Mock.Of<IContentType>(x => x.Alias == "documentType" && x.PropertyTypes == Array.Empty<IPropertyType>());
        var elementType = Mock.Of<IContentType>(x => x.Alias == "elementType" && x.PropertyTypes == Array.Empty<IPropertyType>());

        _contentTypeServiceMock.Setup(x => x.GetAll()).Returns([documentType, elementType]);
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, "documentType"))
            .Returns(Mock.Of<IPublishedContentType>(x =>
                x.Alias == "documentType" &&
                x.IsElement == false &&
                x.CompositionAliases == new HashSet<string>() &&
                x.PropertyTypes == Array.Empty<IPublishedPropertyType>()));
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Content, "elementType"))
            .Returns(Mock.Of<IPublishedContentType>(x =>
                x.Alias == "elementType" &&
                x.IsElement == true &&
                x.CompositionAliases == new HashSet<string>() &&
                x.PropertyTypes == Array.Empty<IPublishedPropertyType>()));

        // Act
        var result = _sut.GetDocumentTypes();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.First(x => x.Alias == "documentType").IsElement, Is.False);
        Assert.That(result.First(x => x.Alias == "elementType").IsElement, Is.True);
    }

    [Test]
    public void GetMediaTypes_SkipsMediaTypesWhenCacheThrows()
    {
        // Arrange
        var cachedType = Mock.Of<IMediaType>(x => x.Alias == "cachedType" && x.PropertyTypes == Array.Empty<IPropertyType>());
        var uncachedType = Mock.Of<IMediaType>(x => x.Alias == "uncachedType");

        _mediaTypeServiceMock.Setup(x => x.GetAll()).Returns([cachedType, uncachedType]);
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Media, "cachedType"))
            .Returns(Mock.Of<IPublishedContentType>(x =>
                x.Alias == "cachedType" &&
                x.IsElement == false &&
                x.CompositionAliases == new HashSet<string>() &&
                x.PropertyTypes == Array.Empty<IPublishedPropertyType>()));
        _publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Media, "uncachedType"))
            .Throws<Exception>();

        // Act
        var result = _sut.GetMediaTypes();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Alias, Is.EqualTo("cachedType"));
        Assert.That(result.First().SchemaId, Is.EqualTo("Cachedtype"));
    }
}
