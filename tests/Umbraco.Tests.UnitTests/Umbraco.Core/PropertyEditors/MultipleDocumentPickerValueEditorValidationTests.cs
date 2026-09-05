using System.ComponentModel.DataAnnotations;
using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
internal sealed class MultipleDocumentPickerValueEditorValidationTests
{
    // A configured minimum is a floor on a selection that is in use, not a way of making the property required -
    // see #23486 - so an empty selection never fails it.
    [TestCase(0, 1, true)]
    [TestCase(1, 1, true)]
    [TestCase(2, 1, true)]
    [TestCase(1, 2, false)]
    [TestCase(0, 2, true)]
    [TestCase(0, 0, true)]
    [TestCase(1, 0, true)]
    public void Validates_The_Minimum_Number_Of_Documents(int documentCount, int min, bool succeed)
    {
        var valueEditor = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultipleDocumentPickerConfiguration
        {
            ValidationLimit = new MultipleDocumentPickerConfiguration.NumberRange { Min = min },
        };

        IEnumerable<ValidationResult> result = valueEditor.Validate(
            Value(documentCount),
            false,
            null,
            PropertyValidationContext.Empty());

        ValidateResult(succeed, result);
    }

    [TestCase(1, 2, true)]
    [TestCase(2, 2, true)]
    [TestCase(3, 2, false)]
    [TestCase(1, 0, true)]
    public void Validates_The_Maximum_Number_Of_Documents(int documentCount, int max, bool succeed)
    {
        var valueEditor = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultipleDocumentPickerConfiguration
        {
            ValidationLimit = new MultipleDocumentPickerConfiguration.NumberRange { Max = max },
        };

        IEnumerable<ValidationResult> result = valueEditor.Validate(
            Value(documentCount),
            false,
            null,
            PropertyValidationContext.Empty());

        ValidateResult(succeed, result);
    }

    [Test]
    public void Validates_That_Every_Picked_Document_Is_Of_An_Allowed_Type()
    {
        var allowedTypeKey = Guid.NewGuid();
        var documentKey = Guid.NewGuid();
        var valueEditor = CreateValueEditor(DocumentOfType(documentKey, Guid.NewGuid()));
        valueEditor.ConfigurationObject = new MultipleDocumentPickerConfiguration
        {
            AllowedContentTypeIds = allowedTypeKey.ToString(),
        };

        IEnumerable<ValidationResult> result = valueEditor.Validate(
            new List<string> { documentKey.ToString() },
            false,
            null,
            PropertyValidationContext.Empty());

        ValidateResult(false, result);
    }

    [Test]
    public void Allows_A_Picked_Document_Of_An_Allowed_Type()
    {
        var allowedTypeKey = Guid.NewGuid();
        var documentKey = Guid.NewGuid();
        var valueEditor = CreateValueEditor(DocumentOfType(documentKey, allowedTypeKey));
        valueEditor.ConfigurationObject = new MultipleDocumentPickerConfiguration
        {
            AllowedContentTypeIds = allowedTypeKey.ToString(),
        };

        IEnumerable<ValidationResult> result = valueEditor.Validate(
            new List<string> { documentKey.ToString() },
            false,
            null,
            PropertyValidationContext.Empty());

        ValidateResult(true, result);
    }

    [Test]
    public void Reports_A_Picked_Document_That_No_Longer_Exists()
    {
        var allowedTypeKey = Guid.NewGuid();
        var valueEditor = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultipleDocumentPickerConfiguration
        {
            AllowedContentTypeIds = allowedTypeKey.ToString(),
        };

        IEnumerable<ValidationResult> result = valueEditor.Validate(
            new List<string> { Guid.NewGuid().ToString() },
            false,
            null,
            PropertyValidationContext.Empty());

        ValidateResult(false, result);
    }

    // The backoffice JSON object converter resolves the array of keys into a typed list of strings before
    // validation runs, so that is the shape a validator is handed.
    private static List<string> Value(int documentCount)
        => Enumerable.Range(0, documentCount).Select(_ => Guid.NewGuid().ToString()).ToList();

    private static IContent DocumentOfType(Guid documentKey, Guid contentTypeKey)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.SetupGet(x => x.Key).Returns(contentTypeKey);

        var content = new Mock<IContent>();
        content.SetupGet(x => x.Key).Returns(documentKey);
        content.SetupGet(x => x.ContentType).Returns(contentType.Object);

        return content.Object;
    }

    private static void ValidateResult(bool succeed, IEnumerable<ValidationResult> result)
    {
        if (succeed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }

    private static MultipleDocumentPickerPropertyEditor.MultipleDocumentPickerPropertyValueEditor CreateValueEditor(
        params IContent[] documents)
    {
        var contentService = new Mock<IContentService>();
        contentService.Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>())).Returns(documents);

        var coreScopeProvider = new Mock<ICoreScopeProvider>();
        coreScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        return new MultipleDocumentPickerPropertyEditor.MultipleDocumentPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<ILocalizedTextService>(),
            contentService.Object,
            coreScopeProvider.Object)
        {
            ConfigurationObject = new MultipleDocumentPickerConfiguration(),
        };
    }
}
