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
    [Test]
    public void Allowed_Type_Passes_When_No_Filter_Configured()
    {
        var elementKey = Guid.NewGuid();
        var (valueEditor, elementServiceMock) = CreateValueEditor();

        elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, Guid.NewGuid())]);

        // AllowedContentTypeIds is null — no restriction
        valueEditor.ConfigurationObject = new ElementPickerConfiguration { AllowedContentTypeIds = null };

        var result = Validate(valueEditor, [elementKey]);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Allowed_Type_Passes_When_Element_Matches_Allowed_Type()
    {
        var elementKey = Guid.NewGuid();
        var contentTypeKey = Guid.NewGuid();
        var (valueEditor, elementServiceMock) = CreateValueEditor();

        elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, contentTypeKey)]);

        valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = contentTypeKey.ToString()
        };

        var result = Validate(valueEditor, [elementKey]);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Allowed_Type_Fails_When_Element_Does_Not_Match_Allowed_Type()
    {
        var elementKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();
        var actualContentTypeKey = Guid.NewGuid(); // different from allowed
        var (valueEditor, elementServiceMock) = CreateValueEditor();

        elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, actualContentTypeKey)]);

        valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = allowedContentTypeKey.ToString()
        };

        var result = Validate(valueEditor, [elementKey]);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Allowed_Type_Passes_With_Multiple_Allowed_Types_When_Element_Matches_One()
    {
        var elementKey = Guid.NewGuid();
        var allowedTypeA = Guid.NewGuid();
        var allowedTypeB = Guid.NewGuid();
        var (valueEditor, elementServiceMock) = CreateValueEditor();

        elementServiceMock
            .Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([CreateElement(elementKey, allowedTypeB)]);

        valueEditor.ConfigurationObject = new ElementPickerConfiguration
        {
            AllowedContentTypeIds = $"{allowedTypeA},{allowedTypeB}"
        };

        var result = Validate(valueEditor, [elementKey]);

        Assert.IsEmpty(result);
    }

    private static IEnumerable<ValidationResult> Validate(
        ElementPickerPropertyEditor.ElementPickerPropertyValueEditor valueEditor,
        IEnumerable<Guid> elementKeys)
    {
        IEnumerable<string> items = elementKeys.Select(k => $"{{\"type\":\"element\",\"unique\":\"{k}\"}}");
        var json = $"[{string.Join(",", items)}]";
        return valueEditor.Validate(json, false, null, PropertyValidationContext.Empty());
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

    private static (ElementPickerPropertyEditor.ElementPickerPropertyValueEditor ValueEditor, Mock<IElementService> ElementServiceMock) CreateValueEditor()
    {
        var elementServiceMock = new Mock<IElementService>();

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

        var valueEditor = new ElementPickerPropertyEditor.ElementPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object,
            elementServiceMock.Object,
            mockScopeProvider.Object)
        {
            ConfigurationObject = new ElementPickerConfiguration()
        };

        return (valueEditor, elementServiceMock);
    }
}
