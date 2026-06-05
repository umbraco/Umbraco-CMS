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
    [Test]
    public void Can_Pass_Validation_When_No_Allowed_Type_Filter_Configured()
    {
        var documentKey = Guid.NewGuid();
        var (valueEditor, contentServiceMock) = CreateValueEditor();

        contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(CreateContent(documentKey, Guid.NewGuid()));

        // AllowedContentTypeIds is null — no restriction
        valueEditor.ConfigurationObject = new ContentPickerConfiguration { AllowedContentTypeIds = null };

        var result = Validate(valueEditor, documentKey);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Can_Pass_Validation_When_Content_Matches_Allowed_Type()
    {
        var documentKey = Guid.NewGuid();
        var contentTypeKey = Guid.NewGuid();
        var (valueEditor, contentServiceMock) = CreateValueEditor();

        contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(CreateContent(documentKey, contentTypeKey));

        valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = contentTypeKey.ToString()
        };

        var result = Validate(valueEditor, documentKey);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Cannot_Pass_Validation_When_Content_Does_Not_Match_Allowed_Type()
    {
        var documentKey = Guid.NewGuid();
        var allowedContentTypeKey = Guid.NewGuid();
        var actualContentTypeKey = Guid.NewGuid(); // different from allowed
        var (valueEditor, contentServiceMock) = CreateValueEditor();

        contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(CreateContent(documentKey, actualContentTypeKey));

        valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = allowedContentTypeKey.ToString()
        };

        var result = Validate(valueEditor, documentKey);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Can_Pass_Validation_When_Content_Matches_One_Of_Multiple_Allowed_Types()
    {
        var documentKey = Guid.NewGuid();
        var allowedTypeA = Guid.NewGuid();
        var allowedTypeB = Guid.NewGuid();
        var (valueEditor, contentServiceMock) = CreateValueEditor();

        contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns(CreateContent(documentKey, allowedTypeB));

        valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = $"{allowedTypeA},{allowedTypeB}"
        };

        var result = Validate(valueEditor, documentKey);

        Assert.IsEmpty(result);
    }

    [Test]
    public void Cannot_Pass_Validation_When_Content_Is_Not_Found()
    {
        var documentKey = Guid.NewGuid();
        var (valueEditor, contentServiceMock) = CreateValueEditor();

        contentServiceMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((IContent?)null);

        valueEditor.ConfigurationObject = new ContentPickerConfiguration
        {
            AllowedContentTypeIds = Guid.NewGuid().ToString()
        };

        var result = Validate(valueEditor, documentKey);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    private static IEnumerable<ValidationResult> Validate(
        ContentPickerPropertyEditor.ContentPickerPropertyValueEditor valueEditor,
        Guid documentKey)
        => valueEditor.Validate(documentKey.ToString(), false, null, PropertyValidationContext.Empty());

    private static IContent CreateContent(Guid contentKey, Guid contentTypeKey)
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Key).Returns(contentTypeKey);

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.Key).Returns(contentKey);
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);

        return contentMock.Object;
    }

    private static (ContentPickerPropertyEditor.ContentPickerPropertyValueEditor ValueEditor, Mock<IContentService> ContentServiceMock) CreateValueEditor()
    {
        var contentServiceMock = new Mock<IContentService>();

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

        var valueEditor = new ContentPickerPropertyEditor.ContentPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            mockScopeProvider.Object,
            contentServiceMock.Object,
            localizedTextServiceMock.Object)
        {
            ConfigurationObject = new ContentPickerConfiguration()
        };

        return (valueEditor, contentServiceMock);
    }
}
