using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ContentPickerPropertyEditorValidationTests
{
    private ContentPickerPropertyEditor.ContentPickerPropertyValueEditor _valueEditor = null!;
    private Mock<IContentService> _contentServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _contentServiceMock = new Mock<IContentService>();

        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock
            .Setup(x => x.Localize(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CultureInfo?>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns("The chosen content is of invalid type.");

        var mockScope = new Mock<ICoreScope>();
        var mockScopeProvider = new Mock<ICoreScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(mockScope.Object);

        _valueEditor = new ContentPickerPropertyEditor.ContentPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            mockScopeProvider.Object,
            _contentServiceMock.Object,
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new ContentPickerConfiguration(),
        };
    }

    [Test]
    public void Can_Pass_Validation_When_No_Allowed_Type_Filter_Configured()
    {
        var documentKey = Guid.NewGuid();

        _contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(CreateContent(documentKey, Guid.NewGuid()));

        // AllowedContentTypeIds is null — no restriction
        _valueEditor.ConfigurationObject = new ContentPickerConfiguration { AllowedContentTypeIds = null };

        Assert.IsEmpty(Validate(documentKey));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Can_Pass_Validation_When_Content_Matches_Allowed_Types(bool hasMultipleAllowedTypes)
    {
        var documentKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();

        _contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(CreateContent(documentKey, allowedContentTypeKey));

        var extraAllowedKey = hasMultipleAllowedTypes ? Guid.NewGuid().ToString() : null;
        _valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = extraAllowedKey is not null
                ? $"{extraAllowedKey},{allowedContentTypeKey}"
                : allowedContentTypeKey.ToString(),
        };

        Assert.IsEmpty(Validate(documentKey));
    }

    [Test]
    public void Cannot_Pass_Validation_When_Content_Does_Not_Match_Allowed_Type()
    {
        var documentKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();
        var actualContentTypeKey = Guid.NewGuid(); // different from allowed

        _contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(CreateContent(documentKey, actualContentTypeKey));

        _valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = allowedContentTypeKey.ToString(),
        };

        Assert.That(Validate(documentKey).Count(), Is.EqualTo(1));
    }

    [Test]
    public void Cannot_Pass_Validation_When_Content_Is_Not_Found()
    {
        var documentKey = Guid.NewGuid();

        _contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((IContent?)null);

        _valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = Guid.NewGuid().ToString(),
        };

        Assert.That(Validate(documentKey).Count(), Is.EqualTo(1));
    }

    [Test]
    public void Can_Pass_Validation_When_Value_Is_Empty()
    {
        // An empty selection is valid even when an allowed-type filter is configured.
        _valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = Guid.NewGuid().ToString(),
        };

        Assert.IsEmpty(_valueEditor.Validate(string.Empty, false, null, PropertyValidationContext.Empty()));
    }

    private IEnumerable<ValidationResult> Validate(Guid documentKey)
        => _valueEditor.Validate(documentKey.ToString(), false, null, PropertyValidationContext.Empty());

    private static IContent CreateContent(Guid contentKey, Guid contentTypeKey)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Key).Returns(contentTypeKey);

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        contentMock.Setup(x => x.Key).Returns(contentKey);

        return contentMock.Object;
    }
}
