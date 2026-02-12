using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentTypeInputSchemaServiceTests
{
    private Mock<IContentTypeService> _contentTypeServiceMock = null!;
    private Mock<IMediaTypeService> _mediaTypeServiceMock = null!;
    private Mock<IMemberTypeService> _memberTypeServiceMock = null!;
    private ContentTypeInputSchemaService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _contentTypeServiceMock = new Mock<IContentTypeService>();
        _mediaTypeServiceMock = new Mock<IMediaTypeService>();
        _memberTypeServiceMock = new Mock<IMemberTypeService>();

        _sut = new ContentTypeInputSchemaService(
            _contentTypeServiceMock.Object,
            _mediaTypeServiceMock.Object,
            _memberTypeServiceMock.Object);
    }

    [Test]
    public async Task GetDocumentTypeSchemasAsync_ReturnsRequestedTypes()
    {
        // Arrange
        var key = Guid.NewGuid();
        var contentType = CreateMockContentType(key, "testType");
        _contentTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns([contentType]);

        // Act
        var result = await _sut.GetDocumentTypeSchemasAsync([key]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Alias, Is.EqualTo("testType"));
        Assert.That(result.First().Key, Is.EqualTo(key));
    }

    [Test]
    public async Task GetDocumentTypeSchemasAsync_ReturnsEmptyWhenNoTypesFound()
    {
        // Arrange
        _contentTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns(Array.Empty<IContentType>());

        // Act
        var result = await _sut.GetDocumentTypeSchemasAsync([Guid.NewGuid()]);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetDocumentTypeSchemasAsync_ReturnsOnlyFoundTypes()
    {
        // Arrange
        var foundKey = Guid.NewGuid();
        var notFoundKey = Guid.NewGuid();
        var contentType = CreateMockContentType(foundKey, "foundType");
        _contentTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns([contentType]);

        // Act
        var result = await _sut.GetDocumentTypeSchemasAsync([foundKey, notFoundKey]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Key, Is.EqualTo(foundKey));
    }

    [Test]
    public async Task GetDocumentTypeSchemasAsync_IncludesAllProperties()
    {
        // Arrange
        var key = Guid.NewGuid();
        var dataTypeKey = Guid.NewGuid();
        var propertyType = Mock.Of<IPropertyType>(p =>
            p.Alias == "testProperty" &&
            p.DataTypeKey == dataTypeKey &&
            p.PropertyEditorAlias == "Umbraco.TextBox" &&
            p.Mandatory == true &&
            p.Variations == ContentVariation.Culture);

        var contentType = Mock.Of<IContentType>(x =>
            x.Key == key &&
            x.Alias == "testType" &&
            x.CompositionPropertyTypes == new[] { propertyType } &&
            x.CompositionKeys() == Array.Empty<Guid>() &&
            x.IsElement == false &&
            x.Variations == ContentVariation.Nothing);

        _contentTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns([contentType]);

        // Act
        var result = await _sut.GetDocumentTypeSchemasAsync([key]);

        // Assert
        var schema = result.First();
        Assert.That(schema.Properties, Has.Count.EqualTo(1));
        var prop = schema.Properties.First();
        Assert.That(prop.Alias, Is.EqualTo("testProperty"));
        Assert.That(prop.DataTypeKey, Is.EqualTo(dataTypeKey));
        Assert.That(prop.EditorAlias, Is.EqualTo("Umbraco.TextBox"));
        Assert.That(prop.Mandatory, Is.True);
        Assert.That(prop.Variations, Is.EqualTo(ContentVariation.Culture));
    }

    [Test]
    public async Task GetDocumentTypeSchemasAsync_CorrectlySetsIsElement()
    {
        // Arrange
        var documentKey = Guid.NewGuid();
        var elementKey = Guid.NewGuid();

        var documentType = CreateMockContentType(documentKey, "documentType", isElement: false);
        var elementType = CreateMockContentType(elementKey, "elementType", isElement: true);

        _contentTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns([documentType, elementType]);

        // Act
        var result = await _sut.GetDocumentTypeSchemasAsync([documentKey, elementKey]);

        // Assert
        Assert.That(result.First(x => x.Alias == "documentType").IsElement, Is.False);
        Assert.That(result.First(x => x.Alias == "elementType").IsElement, Is.True);
    }

    [Test]
    public async Task GetDocumentTypeSchemasAsync_CorrectlySetsVariations()
    {
        // Arrange
        var key = Guid.NewGuid();
        var contentType = Mock.Of<IContentType>(x =>
            x.Key == key &&
            x.Alias == "variantType" &&
            x.CompositionPropertyTypes == Array.Empty<IPropertyType>() &&
            x.CompositionKeys() == Array.Empty<Guid>() &&
            x.IsElement == false &&
            x.Variations == ContentVariation.CultureAndSegment);

        _contentTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns([contentType]);

        // Act
        var result = await _sut.GetDocumentTypeSchemasAsync([key]);

        // Assert
        Assert.That(result.First().Variations, Is.EqualTo(ContentVariation.CultureAndSegment));
    }

    [Test]
    public async Task GetMediaTypeSchemasAsync_ReturnsRequestedTypes()
    {
        // Arrange
        var key = Guid.NewGuid();
        var mediaType = CreateMockMediaType(key, "testMediaType");
        _mediaTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns([mediaType]);

        // Act
        var result = await _sut.GetMediaTypeSchemasAsync([key]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Alias, Is.EqualTo("testMediaType"));
    }

    [Test]
    public async Task GetMemberTypeSchemasAsync_ReturnsRequestedTypes()
    {
        // Arrange
        var key = Guid.NewGuid();
        var memberType = CreateMockMemberType(key, "testMemberType");
        _memberTypeServiceMock.Setup(x => x.GetMany(It.IsAny<Guid[]>())).Returns([memberType]);

        // Act
        var result = await _sut.GetMemberTypeSchemasAsync([key]);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Alias, Is.EqualTo("testMemberType"));
    }

    private static IContentType CreateMockContentType(Guid key, string alias, bool isElement = false)
        => Mock.Of<IContentType>(x =>
            x.Key == key &&
            x.Alias == alias &&
            x.CompositionPropertyTypes == Array.Empty<IPropertyType>() &&
            x.CompositionKeys() == Array.Empty<Guid>() &&
            x.IsElement == isElement &&
            x.Variations == ContentVariation.Nothing);

    private static IMediaType CreateMockMediaType(Guid key, string alias)
        => Mock.Of<IMediaType>(x =>
            x.Key == key &&
            x.Alias == alias &&
            x.CompositionPropertyTypes == Array.Empty<IPropertyType>() &&
            x.CompositionKeys() == Array.Empty<Guid>() &&
            x.IsElement == false &&
            x.Variations == ContentVariation.Nothing);

    private static IMemberType CreateMockMemberType(Guid key, string alias)
        => Mock.Of<IMemberType>(x =>
            x.Key == key &&
            x.Alias == alias &&
            x.CompositionPropertyTypes == Array.Empty<IPropertyType>() &&
            x.CompositionKeys() == Array.Empty<Guid>() &&
            x.IsElement == false &&
            x.Variations == ContentVariation.Nothing);
}
