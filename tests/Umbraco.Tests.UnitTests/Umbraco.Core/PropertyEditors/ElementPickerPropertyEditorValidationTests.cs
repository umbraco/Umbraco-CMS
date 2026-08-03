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
public class ElementPickerPropertyEditorValidationTests
{
    private ElementPickerPropertyEditor.ElementPickerPropertyValueEditor _valueEditor = null!;
    private Mock<IElementService> _elementServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _elementServiceMock = new Mock<IElementService>();

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

        _valueEditor = new ElementPickerPropertyEditor.ElementPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object,
            _elementServiceMock.Object,
            mockScopeProvider.Object)
        {
            ConfigurationObject = new ElementPickerConfiguration(),
        };
    }

    [Test]
    public void Can_Pass_Validation_When_No_Allowed_Type_Filter_Configured()
    {
        var elementKey = Guid.NewGuid();

        _elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, Guid.NewGuid())]);

        // AllowedContentTypeIds is null — no restriction
        _valueEditor.ConfigurationObject = new ElementPickerConfiguration { AllowedContentTypeIds = null };

        Assert.IsEmpty(Validate([elementKey]));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Can_Pass_Validation_When_Element_Matches_Allowed_Types(bool hasMultipleAllowedTypes)
    {
        var elementKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();

        _elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, allowedContentTypeKey)]);

        var extraAllowedKey = hasMultipleAllowedTypes ? Guid.NewGuid().ToString() : null;
        _valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = extraAllowedKey is not null
                ? $"{extraAllowedKey},{allowedContentTypeKey}"
                : allowedContentTypeKey.ToString(),
        };

        Assert.IsEmpty(Validate([elementKey]));
    }

    [Test]
    public void Cannot_Pass_Validation_When_Element_Does_Not_Match_Allowed_Type()
    {
        var elementKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();
        var actualContentTypeKey = Guid.NewGuid(); // different from allowed

        _elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, actualContentTypeKey)]);

        _valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = allowedContentTypeKey.ToString(),
        };

        Assert.That(Validate([elementKey]).Count(), Is.EqualTo(1));
    }

    [Test]
    public void Cannot_Pass_Validation_When_Element_Is_Not_Found()
    {
        var elementKey = Guid.NewGuid();

        // The selected element cannot be found, so its type cannot be verified against the allowed types.
        _elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([]);

        _valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = Guid.NewGuid().ToString(),
        };

        Assert.That(Validate([elementKey]).Count(), Is.EqualTo(1));
    }

    [Test]
    public void Can_Pass_Validation_When_Selection_Is_Empty()
    {
        // An empty selection is valid even when an allowed-type filter is configured.
        _valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = Guid.NewGuid().ToString(),
        };

        Assert.IsEmpty(Validate([]));
    }

    [Test]
    public void Can_Pass_Validation_When_Selection_Contains_Duplicate_Keys()
    {
        var elementKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();

        // The (deduplicated) key resolves to a single element.
        _elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, allowedContentTypeKey)]);

        _valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = allowedContentTypeKey.ToString(),
        };

        // The same key selected twice must not be reported as missing.
        Assert.IsEmpty(Validate([elementKey, elementKey]));
    }

    [Test]
    public void Ignores_Non_Guid_Entries_When_Checking_For_Missing_Elements()
    {
        var elementKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();

        _elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, allowedContentTypeKey)]);

        _valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = allowedContentTypeKey.ToString(),
        };

        // A non-GUID entry is not a resolvable key and must not be reported as missing.
        List<string> value = ["not-a-guid", elementKey.ToString()];
        Assert.IsEmpty(_valueEditor.Validate(value, false, null, PropertyValidationContext.Empty()));
    }

    private IEnumerable<ValidationResult> Validate(IEnumerable<Guid> elementKeys)
    {
        List<string> value = elementKeys.Select(k => k.ToString()).ToList();
        return _valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());
    }

    private static IElement CreateElement(Guid elementKey, Guid contentTypeKey)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Key).Returns(contentTypeKey);

        var elementMock = new Mock<IElement>();
        elementMock.Setup(x => x.Key).Returns(elementKey);
        elementMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);

        return elementMock.Object;
    }
}
